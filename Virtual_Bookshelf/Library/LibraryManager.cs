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
            if (Mode == "library")
            {
                table.AddColumn("Status");
                foreach (var book in bookList)
                {
                    table.AddRow(book.Title, book.Author, book.PublicationDate.ToString(), book.PageCount.ToString(), book.Status);
                }
            }
            else
            {
                foreach (var book in bookList)
                {
                    table.AddRow(book.Title, book.Author, book.PublicationDate.ToString(), book.PageCount.ToString());
                }
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
                            LibrarySearch.SearchBookByISBN(FileName, Mode);
                            break;
                        case "Search by Title":
                            LibrarySearch.SearchBookByTitle(FileName, Mode);
                            break;
                        case "Search by Author":
                            LibrarySearch.SearchBookByAuthor(FileName, Mode);
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

            string publicationYear = GetValidYearInput("Publication Year (yyyy)");
            if (publicationYear.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

            string pageCountStr = GetRequiredInput("Page Count");
            if (pageCountStr.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

            Console.WriteLine($"Add book: {title} by {author}, publication year {publicationYear}, Page Count: {pageCountStr}?");
            var confirm = Prompt.Confirm("Confirm add book?");

            if (confirm)
            {
                int pageCount = int.TryParse(pageCountStr, out int parsedCount) ? parsedCount : 0;

                string status = Mode == "wishlist" ? "In wishlist" : "Not started";
                var book = new Book
                {
                    Title = title,
                    Author = author,
                    PublicationDate = int.Parse(publicationYear),
                    PageCount = pageCount,
                    Status = status,
                };
                LibraryStorage.SaveBook(book, FileName);
                Console.WriteLine("Book added successfully!");
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

            LibraryStorage.UpdateBooks(bookList, FileName);
            Console.WriteLine("Book updated successfully!");
        }

        public static void ChangeBookStatus(string selectedBook, List<Book> bookList, string FileName)
        {
            var book = bookList.FirstOrDefault(b => (b.Title + " by " + b.Author) == selectedBook);
            if (book == null) return;

            Console.WriteLine("Current status: " + book.Status);

            string newStatus = Prompt.Select("Select new status", new[] { "Not started", "Reading", "Finished" });

            book.Status = newStatus;
            if (newStatus == "Finished")
            {
                book.DateFinished = DateTime.Now;
            }
            else
            {
                book.DateFinished = null;
            }
            book.DateModified = DateTime.Now;

            LibraryStorage.UpdateBooks(bookList, FileName);
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
            LibraryStorage.UpdateBooks(bookList, FileName);
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
            LibraryStorage.UpdateBooks(wishlist, WishlistFileName);

            Console.WriteLine("Book moved to library successfully!");
        }

        public static void SearchFilterBooks(string FileName, string Mode)
        {
            var searchMenu = Prompt.Select("Search/filter books by", new[]
            {
                "Title",
                "Author",
                "Publication Year",
                "Status",
                "Return"
            });
            switch (searchMenu)
            {
                case "Title":
                    Console.Write("Enter Title: ");
                    string Title = Prompt.Input<string>("Title", validators: new[] { Validators.Required() });
                    LibrarySearch.SearchLibrary(Title, FileName, Mode);
                    break;
                case "Author":
                    Console.WriteLine("Search by author functionality is not implemented yet.");
                    break;
                case "Publication Year":
                    Console.WriteLine("Search by publication year functionality is not implemented yet.");
                    break;
                case "Status":
                    Console.WriteLine("Search by status functionality is not implemented yet.");
                    break;
                case "Return":
                    return;
            }
        }

        public static void ExportLibrary()
        {
            throw new NotImplementedException("Export library functionality is not implemented yet.");
        }

        public static void GoalsStatisticsMenu()
        {
            throw new NotImplementedException("Goals & statistics menu is not implemented yet.");
        }
    }
}
