using System;
using Spectre.Console;
using Sharprompt;
using Virtual_Bookshelf.Library.Models;
using Virtual_Bookshelf.Library.Services;

namespace Virtual_Bookshelf.Library
{
    public static class LibraryManager
    {
        public static void ViewBooks(string FileName, string Mode)
        {
            Console.WriteLine("Viewing " + Mode + "...");
            var bookList = LibraryStorage.LoadBookList(FileName);

            if (bookList.Count == 0)
            {
                Console.WriteLine("Your " + Mode + " is empty.");
                return;
            }

            Console.WriteLine("Your " + Mode + ":");
            var table = new Table();
            table.AddColumn("Title");
            table.AddColumn("Author");
            table.AddColumn("Publication Date");
            table.AddColumn("Page Count");
            table.AddColumn("Pages Read");
            table.AddColumn("Status");
            foreach (var book in bookList)
            {
                table.AddRow(book.Title, book.Author, book.PublicationDate.ToString(), book.PageCount.ToString(), book.PagesRead.ToString(), book.Status);
            }
            AnsiConsole.Write(table);
        }

        public static void AddBook(string FileName, string Mode)
        {
            var entryMethod = Prompt.Select("Add book by", new[]
            {
                "Manual entry",
                "Google Books search (requires API key)",
                "Return"
            });

            switch (entryMethod)
            {
                case "Manual entry":
                    AddBookManually(FileName, Mode);
                    break;
                case "Google Books search (requires API key)":
                    var apiKey = ApiKeyManager.GetOrCreateApiKey();
                    Program.BookService = new BookService(apiKey);

                    var method = Prompt.Select("Choose search method", new[]
                    {
                        "Search by ISBN", "Search by Title", "Search by Author", "new API key", "Return"
                    });
                    switch (method)
                    {
                        case "Search by ISBN":
                            LibrarySearch.GoogleBooksSearch(FileName, Mode, "ISBN");
                            break;
                        case "Search by Title":
                            LibrarySearch.GoogleBooksSearch(FileName, Mode, "Title");
                            break;
                        case "Search by Author":
                            LibrarySearch.GoogleBooksSearch(FileName, Mode, "Author");
                            break;
                        case "new API key":
                            string newApiKey = Prompt.Input<string>("Enter new API key", validators: new[] { Validators.Required() });
                            ApiKeyManager.SaveApiKey(newApiKey);
                            Program.BookService = new BookService(newApiKey);
                            Console.WriteLine("API key updated successfully!");
                            break;
                        case "Return":
                            return;
                    }
                    break;
                case "Return":
                    return;
            }
        }

        public static void AddBookManually(string FileName, string Mode)
        {
            Console.WriteLine("Adding book manually, enter 'exit' to stop...");

            string title = GetRequiredInput("Title");
            if (title.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

            string author = GetRequiredInput("Author");
            if (author.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

            string genre = Prompt.Input<string>("Genre (optional, e.g., Fiction, Mystery, Science Fiction)", defaultValue: "Unknown");
            if (genre.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

            string publicationYear = GetValidYearInput("Publication Year (yyyy)");
            if (publicationYear.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

            string pageCountStr = GetRequiredInput("Page Count");
            if (pageCountStr.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

            Console.WriteLine($"Add book: {title} by {author}, publication year {publicationYear}, Page Count: {pageCountStr}?");
            var confirm = Prompt.Confirm("Confirm add book?");

            if (confirm)
            {
                int pageCount = int.TryParse(pageCountStr, out int parsedCount) ? parsedCount : 0;

                // Get initial status from custom labels
                var availableLabels = StatusLabelManager.GetAllStatusLabelNames(FileName);
                string initialStatus;

                if (Mode == "wishlist")
                {
                    initialStatus = availableLabels.FirstOrDefault(l => l.Equals("In wishlist", StringComparison.OrdinalIgnoreCase)) ?? availableLabels.FirstOrDefault() ?? "Not started";
                }
                else
                {
                    initialStatus = availableLabels.FirstOrDefault(l => l.Equals("Not started", StringComparison.OrdinalIgnoreCase)) ?? availableLabels.FirstOrDefault() ?? "Not started";
                }

                var book = new Book
                {
                    Title = title,
                    Author = author,
                    Genre = string.IsNullOrWhiteSpace(genre) ? "Unknown" : genre,
                    PublicationDate = int.Parse(publicationYear),
                    PageCount = pageCount,
                    Status = initialStatus,
                };
                try
                {
                    LibraryStorage.SaveBook(book, FileName);
                    Console.WriteLine("Book added successfully!");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Book not added.");
            }
        }

        public static string GetRequiredInput(string FieldName)
        {
            return Prompt.Input<string>($"{FieldName}: ", validators: new[] { Validators.Required() });
        }

        public static string GetValidYearInput(string FieldName)
        {
            string input = Prompt.Input<string>($"{FieldName}: ", validators: new[] { Validators.Required(), Validators.RegularExpression(@"^\d{4}$", "Please enter a valid year in yyyy format") });

            if (int.TryParse(input, out int year) && year >= 1 && year <= DateTime.Now.Year)
            {
                return input;
            }

            Console.WriteLine("Invalid year. Please enter a year between 1 and " + DateTime.Now.Year + ".");
            return GetValidYearInput(FieldName);
        }

        public static void EditOrRemoveBook(string FileName, string Mode)
        {
            var bookList = LibraryStorage.LoadBookList(FileName);

            if (bookList.Count == 0)
            {
                Console.WriteLine("Your " + Mode + " is empty.");
                return;
            }

            Console.WriteLine("Book list:");
            var selectedBook = Prompt.Select("Select a book to edit/remove", bookList.Select(b => b.Title + " by " + b.Author).ToArray());
            var selection = Prompt.Select("Choose action", new[]
            {
                "Edit",
                "Change status",
                "Manage bookmarks",
                "Remove",
                "Return"
            });

            switch (selection)
            {
                case "Edit":
                    EditBook(selectedBook, bookList, FileName);
                    break;
                case "Change status":
                    ChangeBookStatus(selectedBook, bookList, FileName);
                    break;
                case "Manage bookmarks":
                    ManageBookmarks(selectedBook, bookList, FileName);
                    break;
                case "Remove":
                    RemoveBook(selectedBook, bookList, FileName);
                    break;
                case "Return":
                    return;
            }
        }

        public static void EditBook(string selectedBook, List<Book> bookList, string FileName)
        {
            var book = bookList.FirstOrDefault(b => (b.Title + " by " + b.Author) == selectedBook);
            if (book == null) return;

            Console.WriteLine("Editing book: " + selectedBook);
            string newTitle = Prompt.Input<string>("New Title (leave blank to keep current)", defaultValue: book.Title);
            string newAuthor = Prompt.Input<string>("New Author (leave blank to keep current)", defaultValue: book.Author);
            string newPublicationDate = Prompt.Input<string>("New Publication Date (yyyy, leave blank to keep current)", defaultValue: book.PublicationDate.ToString());
            string newPageCountStr = Prompt.Input<string>("New Page Count (leave blank to keep current)", defaultValue: book.PageCount.ToString());

            if (!string.IsNullOrWhiteSpace(newTitle)) book.Title = newTitle;
            if (!string.IsNullOrWhiteSpace(newAuthor)) book.Author = newAuthor;
            if (!string.IsNullOrWhiteSpace(newPublicationDate) && DateTime.TryParse(newPublicationDate, out DateTime pubDate)) book.PublicationDate = pubDate.Year;
            if (!string.IsNullOrWhiteSpace(newPageCountStr) && int.TryParse(newPageCountStr, out int pageCount)) book.PageCount = pageCount;

            LibraryStorage.SaveBookList(bookList, FileName);
            Console.WriteLine("Book updated successfully!");
        }

        public static void ChangeBookStatus(string selectedBook, List<Book> bookList, string FileName)
        {
            var book = bookList.FirstOrDefault(b => (b.Title + " by " + b.Author) == selectedBook);
            if (book == null) return;

            Console.WriteLine("Current status: " + book.Status);

            // Load available custom status labels
            var availableLabels = StatusLabelManager.GetAllStatusLabelNames(FileName);
            if (availableLabels.Count == 0)
            {
                Console.WriteLine("No custom status labels found. Please create some first.");
                return;
            }

            availableLabels.Add("Return");
            string newStatus = Prompt.Select("Select new status", availableLabels.ToArray());

            if (newStatus == "Return") return;

            string previousStatus = book.Status;
            int previousPagesRead = book.PagesRead;

            // If changing to "Finished", set PagesRead to PageCount and DateFinished to now. If changing from "Finished" to something else, clear DateFinished and reset PagesRead.
            book.Status = newStatus;
            if (newStatus.Equals("In progress", StringComparison.OrdinalIgnoreCase) || newStatus.Equals("Paused", StringComparison.OrdinalIgnoreCase))
            {

                string pagesReadStr = Prompt.Input<string>("Pages read so far (leave blank to keep current)", defaultValue: book.PagesRead.ToString());
                if (!string.IsNullOrWhiteSpace(pagesReadStr) && int.TryParse(pagesReadStr, out int pagesRead))
                {
                    book.PagesRead = pagesRead;
                }
            }

            // Check if the new status might indicate completion
            if (newStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                book.DateFinished = DateTime.Now;
                book.PagesRead = book.PageCount;
            }
            else if (newStatus.Equals("Not started", StringComparison.OrdinalIgnoreCase))
            {
                book.PagesRead = 0;
                book.DateFinished = null;
            }
            else
            {
                book.DateFinished = null;
            }

            // Log the status change
            var statusChange = new StatusChangeRecord
            {
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                ChangeDate = DateTime.Now,
                PagesReadAtChange = book.PagesRead
            };
            book.StatusChanges.Add(statusChange);

            book.DateModified = DateTime.Now;
            bookList.Remove(book);
            bookList.Add(book);

            LibraryStorage.SaveBookList(bookList, FileName);
            Console.WriteLine("Book status updated successfully!");
        }

        public static void RemoveBook(string selectedBook, List<Book> bookList, string FileName)
        {
            var book = bookList.FirstOrDefault(b => (b.Title + " by " + b.Author) == selectedBook);
            if (book == null) return;

            var confirm = Prompt.Confirm("Are you sure you want to remove this book?");
            if (!confirm)
            {
                Console.WriteLine("Book not removed.");
                return;
            }

            bookList.Remove(book);
            LibraryStorage.SaveBookList(bookList, FileName);
            Console.WriteLine("Book removed successfully!");
        }

        public static void ViewBookDetails(string selectedBook, List<Book> bookList)
        {
            var book = bookList.FirstOrDefault(b => (b.Title + " by " + b.Author) == selectedBook);
            if (book == null) return;

            Console.WriteLine("Title: " + book.Title);
            Console.WriteLine("Author: " + book.Author);
            Console.WriteLine("Publication Date: " + book.PublicationDate);
            Console.WriteLine("Page Count: " + book.PageCount);
            Console.WriteLine("Pages Read: " + book.PagesRead);
            Console.WriteLine("Status: " + book.Status);
            Console.WriteLine("Date Added: " + book.DateAdded);
            Console.WriteLine("Date Finished: " + (book.DateFinished.HasValue ? book.DateFinished.Value.ToString() : "N/A"));
        }

        public static void WishlistToLibrary(string WishlistFileName, string LibraryFileName)
        {
            var wishlist = LibraryStorage.LoadBookList(WishlistFileName);
            if (wishlist.Count == 0)
            {
                Console.WriteLine("Your wishlist is empty.");
                return;
            }

            var selectedBook = Prompt.Select("Select a book to move to library", wishlist.Select(b => b.Title + " by " + b.Author).ToArray());
            var book = wishlist.FirstOrDefault(b => (b.Title + " by " + b.Author) == selectedBook);
            if (book == null) return;

            var confirm = Prompt.Confirm($"Move '{book.Title}' by {book.Author} from wishlist to library?");
            if (!confirm)
            {
                Console.WriteLine("Book not moved.");
                return;
            }

            // Add to library
            book.Status = "Not started"; // Update status to reflect library ownership
            LibraryStorage.SaveBook(book, LibraryFileName);

            // Remove from wishlist
            wishlist.Remove(book);
            LibraryStorage.SaveBookList(wishlist, WishlistFileName);

            Console.WriteLine("Book moved to library successfully!");
        }

        public static void SearchFilterBooks(string FileName, string Mode)
        {
            var searchMenu = Prompt.Select("Search/filter books by", new[]
            {
                "Title",
                "Author",
                "Status",
                "Filter by Year",
                "Return"
            });
            switch (searchMenu)
            {
                case "Title":
                    Console.Write("Enter Title: ");
                    string Title = Prompt.Input<string>("Title", validators: new[] { Validators.Required() });
                    LibrarySearch.SearchLibrary(Title, FileName, "Title");
                    break;
                case "Author":
                    Console.Write("Enter Author: ");
                    string Author = Prompt.Input<string>("Author", validators: new[] { Validators.Required() });
                    LibrarySearch.SearchLibrary(Author, FileName, "Author");
                    break;
                case "Filter by Year":
                    Console.Write("Enter Start Year: ");
                    int StartYear = Prompt.Input<int>("Start Year", validators: new[] { Validators.Required(), Validators.RegularExpression(@"^\d{4}$", "Please enter a valid 4-digit year") });
                    Console.Write("Enter End Year: ");
                    int EndYear = Prompt.Input<int>("End Year", validators: new[] { Validators.Required(), Validators.RegularExpression(@"^\d{4}$", "Please enter a valid 4-digit year") });
                    LibrarySearch.FilterBooksByYears(FileName, StartYear, EndYear);
                    break;
                case "Status":
                    Console.Write("Enter Status: ");
                    string Status = Prompt.Input<string>("Status", validators: new[] { Validators.Required() });
                    LibrarySearch.SearchLibrary(Status, FileName, "Status");
                    break;
                case "Return":
                    return;
            }
        }
        public static void ImportLibrary(string FileName, string Mode)
        {
            string filePath = Prompt.Input<string>("Enter the JSON file path to import from", validators: new[] { Validators.Required() });
            try
            {
                var importedBooks = LibraryStorage.ImportFromJson(filePath);
                if (importedBooks.Count == 0)
                {
                    Console.WriteLine("No books found in the JSON file.");
                    return;
                }
                var existingBooks = LibraryStorage.LoadBookList(FileName);

                int addedCount = 0;
                foreach (var book in importedBooks)
                {
                    if (!existingBooks.Any(b => b.Title == book.Title && b.Author == book.Author && b.PublicationDate == book.PublicationDate && b.PageCount == book.PageCount))
                    {
                        existingBooks.Add(book);
                        addedCount++;
                    }
                }

                LibraryStorage.SaveBookList(existingBooks, FileName);
                Console.WriteLine($"Import completed. {addedCount} new books added to your {Mode}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error importing library: " + ex.Message);
            }
        }

        public static void ExportLibrary(string FileName, string Mode)
        {
            var bookList = LibraryStorage.LoadBookList(FileName);
            if (bookList.Count == 0)
            {
                Console.WriteLine("Your {0} is empty. Nothing to export.", Mode);
                return;
            }

            string filePath = Prompt.Input<string>("Enter the file path to export to (e.g., library)", validators: new[] { Validators.Required() });
            if (!filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                filePath += ".json";
            }

            try
            {
                LibraryStorage.ExportToJson(bookList, filePath);
                Console.WriteLine("Library exported successfully to " + filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error exporting library: " + ex.Message);
            }
        }

        public static void GoalsStatisticsMenu()
        {
            throw new NotImplementedException("Goals & statistics menu is not implemented yet.");
        }

        public static void ManageStatusLabels(string FileName)
        {
            do
            {
                var labels = StatusLabelManager.LoadStatusLabels(FileName);
                var labelNames = labels.Select(l => l.Name).ToList();
                labelNames.Add("Create new label");
                labelNames.Add("Return");

                string selection = Prompt.Select("Custom Status Labels", labelNames.ToArray());

                if (selection == "Return")
                {
                    return;
                }
                else if (selection == "Create new label")
                {
                    CreateNewStatusLabel(FileName);
                }
                else
                {
                    ManageStatusLabelDetails(FileName, selection);
                }
            } while (true);
        }

        public static void CreateNewStatusLabel(string FileName)
        {
            string name = Prompt.Input<string>("Enter label name", validators: new[] { Validators.Required() });
            string description = Prompt.Input<string>("Enter label description (optional)", defaultValue: "");

            try
            {
                StatusLabelManager.AddStatusLabel(FileName, name, description);
                Console.WriteLine($"Status label '{name}' created successfully!");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void ManageStatusLabelDetails(string FileName, string labelName)
        {
            var label = StatusLabelManager.GetStatusLabel(FileName, labelName);
            if (label == null)
            {
                Console.WriteLine("Label not found.");
                return;
            }

            Console.WriteLine($"Label: {label.Name}");
            Console.WriteLine($"Description: {label.Description}");
            Console.WriteLine($"Created: {label.DateCreated}");
            Console.WriteLine($"Modified: {label.DateModified}");

            string action = Prompt.Select("Action", new[] { "Edit", "Delete", "Return" });

            switch (action)
            {
                case "Edit":
                    EditStatusLabel(FileName, labelName);
                    break;
                case "Delete":
                    DeleteStatusLabel(FileName, labelName);
                    break;
                case "Return":
                    return;
            }
        }

        public static void EditStatusLabel(string FileName, string oldName)
        {
            string newName = Prompt.Input<string>("Enter new label name (leave blank to keep current)", defaultValue: oldName);
            string description = Prompt.Input<string>("Enter new description (leave blank to keep current)", defaultValue: "");

            try
            {
                if (string.IsNullOrWhiteSpace(newName))
                {
                    newName = oldName;
                }

                var currentLabel = StatusLabelManager.GetStatusLabel(FileName, oldName);
                if (currentLabel != null && string.IsNullOrWhiteSpace(description))
                {
                    description = currentLabel.Description;
                }

                StatusLabelManager.UpdateStatusLabel(FileName, oldName, newName, description);
                Console.WriteLine($"Status label updated successfully!");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void DeleteStatusLabel(string FileName, string labelName)
        {
            var confirm = Prompt.Confirm($"Are you sure you want to delete the '{labelName}' label? Books with this status will keep the label value.");
            if (!confirm)
            {
                Console.WriteLine("Label not deleted.");
                return;
            }

            try
            {
                StatusLabelManager.DeleteStatusLabel(FileName, labelName);
                Console.WriteLine($"Status label '{labelName}' deleted successfully!");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void ManageBookmarks(string selectedBook, List<Book> bookList, string FileName)
        {
            var book = bookList.FirstOrDefault(b => (b.Title + " by " + b.Author) == selectedBook);
            if (book == null) return;

            do
            {
                Console.WriteLine($"\nManaging bookmarks for: {book.Title}");
                Console.WriteLine($"Bookmarks: {BookmarkManager.GetBookmarkCount(book)}/3");

                var bookmarks = BookmarkManager.GetBookmarks(book);
                var options = new List<string>();

                if (bookmarks.Count > 0)
                {
                    foreach (var bookmark in bookmarks)
                    {
                        options.Add($"{bookmark.GetColorCode()} Page {bookmark.PageNumber}" +
                            (string.IsNullOrEmpty(bookmark.Notes) ? "" : $" - {bookmark.Notes}"));
                    }
                }

                if (BookmarkManager.GetRemainingBookmarkSlots(book) > 0)
                {
                    options.Add("Add bookmark");
                }

                options.Add("Return");

                string action = Prompt.Select("Select bookmark to manage or action", options.ToArray());

                if (action == "Return")
                {
                    LibraryStorage.SaveBookList(bookList, FileName);
                    return;
                }
                else if (action == "Add bookmark")
                {
                    AddBookmarkToBook(book);
                }
                else
                {
                    // Extract page number from the selection
                    var parts = action.Split(' ');
                    if (int.TryParse(parts[2], out int pageNumber))
                    {
                        ManageBookmarkDetails(book, pageNumber);
                    }
                }
            } while (true);
        }

        public static void AddBookmarkToBook(Book book)
        {
            string pageInput = Prompt.Input<string>("Enter page number", validators: new[] { Validators.Required() });
            if (!int.TryParse(pageInput, out int pageNumber))
            {
                Console.WriteLine("Invalid page number.");
                return;
            }

            var colorOptions = Bookmark.AvailableColors.Select(c => c.ToUpper()).ToArray();
            string selectedColor = Prompt.Select("Select bookmark color", colorOptions);

            string notes = Prompt.Input<string>("Add notes (optional)", defaultValue: "");

            try
            {
                BookmarkManager.AddBookmark(book, pageNumber, selectedColor.ToLower(), notes);
                Console.WriteLine($"Bookmark added at page {pageNumber}!");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void ManageBookmarkDetails(Book book, int pageNumber)
        {
            var bookmark = BookmarkManager.GetBookmark(book, pageNumber);
            if (bookmark == null)
            {
                Console.WriteLine("Bookmark not found.");
                return;
            }

            Console.WriteLine($"Bookmark at page {pageNumber}:");
            Console.WriteLine($"Color: {bookmark.GetColorCode()} {bookmark.Color}");
            Console.WriteLine($"Notes: {(string.IsNullOrEmpty(bookmark.Notes) ? "None" : bookmark.Notes)}");
            Console.WriteLine($"Created: {bookmark.DateCreated}");

            string action = Prompt.Select("Action", new[] { "Edit", "Delete", "Return" });

            switch (action)
            {
                case "Edit":
                    EditBookmark(book, pageNumber);
                    break;
                case "Delete":
                    DeleteBookmark(book, pageNumber);
                    break;
                case "Return":
                    return;
            }
        }

        public static void EditBookmark(Book book, int pageNumber)
        {
            var bookmark = BookmarkManager.GetBookmark(book, pageNumber);
            if (bookmark == null) return;

            var colorOptions = Bookmark.AvailableColors.Select(c => c.ToUpper()).ToArray();
            string selectedColor = Prompt.Select("Select new bookmark color", colorOptions);

            string notes = Prompt.Input<string>("Update notes (leave blank to keep current)", defaultValue: bookmark.Notes);

            try
            {
                BookmarkManager.UpdateBookmark(book, pageNumber, selectedColor.ToLower(), notes);
                Console.WriteLine("Bookmark updated successfully!");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void DeleteBookmark(Book book, int pageNumber)
        {
            var confirm = Prompt.Confirm($"Delete bookmark at page {pageNumber}?");
            if (!confirm)
            {
                Console.WriteLine("Bookmark not deleted.");
                return;
            }

            try
            {
                BookmarkManager.RemoveBookmark(book, pageNumber);
                Console.WriteLine("Bookmark deleted successfully!");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void ViewStatistics(string FileName)
        {
            var bookList = LibraryStorage.LoadBookList(FileName);

            if (bookList.Count == 0)
            {
                Console.WriteLine("Your library is empty. No statistics to display.");
                return;
            }

            do
            {
                Console.Clear();
                Console.WriteLine(StatisticsService.GetStatisticsSummary(bookList));

                string action = Prompt.Select("Statistics Options", new[]
                {
                    "View Books Per Month",
                    "View Books Per Year",
                    "View Reading History",
                    "Return"
                });

                switch (action)
                {
                    case "View Books Per Month":
                        ViewBooksPerMonth(bookList);
                        break;
                    case "View Books Per Year":
                        ViewBooksPerYear(bookList);
                        break;
                    case "View Reading History":
                        ViewReadingHistory(bookList);
                        break;
                    case "Return":
                        return;
                }
            } while (true);
        }

        public static void ViewBooksPerMonth(List<Book> bookList)
        {
            var booksPerMonth = StatisticsService.GetBooksPerMonth(bookList);

            if (booksPerMonth.Count == 0)
            {
                Console.WriteLine("No completed books yet.");
                return;
            }

            Console.WriteLine("\n========== BOOKS COMPLETED PER MONTH ==========");
            foreach (var month in booksPerMonth)
            {
                Console.WriteLine($"{month.Key}: {month.Value} book(s)");
            }
            Console.WriteLine("==============================================\n");

            Prompt.Input<string>("Press Enter to continue...", defaultValue: "");
        }

        public static void ViewBooksPerYear(List<Book> bookList)
        {
            var booksPerYear = StatisticsService.GetBooksPerYear(bookList);

            if (booksPerYear.Count == 0)
            {
                Console.WriteLine("No completed books yet.");
                return;
            }

            Console.WriteLine("\n========== BOOKS COMPLETED PER YEAR ==========");
            foreach (var year in booksPerYear)
            {
                Console.WriteLine($"{year.Key}: {year.Value} book(s)");
            }
            Console.WriteLine("==============================================\n");

            Prompt.Input<string>("Press Enter to continue...", defaultValue: "");
        }

        public static void ViewReadingHistory(List<Book> bookList)
        {
            Console.WriteLine("\n========== READING HISTORY ==========");

            var completedBooks = bookList
                .Where(b => b.StatusChanges.Any(sc =>
                    sc.NewStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                    sc.NewStatus.Equals("Finished", StringComparison.OrdinalIgnoreCase)
                ))
                .OrderByDescending(b => b.DateFinished)
                .ToList();

            if (completedBooks.Count == 0)
            {
                Console.WriteLine("No completed books yet.");
                return;
            }

            foreach (var book in completedBooks)
            {
                var completionRecord = book.StatusChanges.LastOrDefault(sc =>
                    sc.NewStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                    sc.NewStatus.Equals("Finished", StringComparison.OrdinalIgnoreCase)
                );

                if (completionRecord != null)
                {
                    Console.WriteLine($"✓ {book.Title} by {book.Author}");
                    Console.WriteLine($"  Completed: {completionRecord.ChangeDate:yyyy-MM-dd}");
                    Console.WriteLine($"  Pages: {book.PageCount} | Genre: {book.Genre}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("=====================================\n");

            Prompt.Input<string>("Press Enter to continue...", defaultValue: "");
        }
    }
}
