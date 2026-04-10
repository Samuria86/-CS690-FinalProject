using Google.Apis.Books.v1.Data;
using Spectre.Console;
using Sharprompt;
using Virtual_Bookshelf.Library.Models;

namespace Virtual_Bookshelf.Library
{
    public static class LibrarySearch
    {
        public static void SearchBookByISBN(string FileName, string Mode)
        {
            var isbn = Prompt.Input<string>("ISBN", validators: new[] { Validators.Required(), Validators.RegularExpression(@"^\d{10}(\d{3})?$", "Please enter a valid 10 or 13 digit ISBN") });
            var result = SearchSingleBook("isbn", isbn, "Searching for book...");

            if (result == null)
            {
                Console.WriteLine("No results found for the given ISBN.");
                return;
            }

            ConfirmAndSaveBook(result.VolumeInfo, FileName, Mode);
        }

        public static void SearchBookByTitle(string FileName, string Mode)
        {
            var title = Prompt.Input<string>("Title", validators: new[] { Validators.Required() });
            var result = SearchBooks("intitle", title, "Searching for books...");

            if (!HasResults(result))
            {
                Console.WriteLine("No results found for the given Title.");
                return;
            }

            var titleItems = result!.Items!;
            TryAddSelectedBooks(titleItems, FileName, Mode);
        }

        public static void SearchBookByAuthor(string FileName, string Mode)
        {
            var author = Prompt.Input<string>("Author", validators: new[] { Validators.Required() });
            var result = SearchBooks("inauthor", author, "Searching for books...");

            if (!HasResults(result))
            {
                Console.WriteLine("No results found for the given Author.");
                return;
            }

            var authorItems = result!.Items!;
            TryAddSelectedBooks(authorItems, FileName, Mode);
        }

        public static void SearchLibrary(string Query, string FileName, string Mode)
        {
            var bookList = LibraryStorage.LoadBookList(FileName);
            var results = bookList.Where(b => b.Title.Contains(Query, StringComparison.OrdinalIgnoreCase)).ToList();

            if (results.Count == 0)
            {
                Console.WriteLine("No books found with the given title.");
                return;
            }

            Console.WriteLine($"Books found with title containing '{Query}':");
            var selectedBook = Prompt.Select("Select a book to view/edit", results.Select(b => b.Title + " by " + b.Author).ToArray());

            while (true)
            {
                var selection = Prompt.Select("Choose action", new[]
                {
                    "View details",
                    "Edit",
                    "Change status",
                    "Remove",
                    "Return"
                });
                switch (selection)
                {
                    case "View details":
                        LibraryManager.ViewBookDetails(selectedBook, bookList);
                        break;
                    case "Edit":
                        LibraryManager.EditBook(selectedBook, bookList, FileName);
                        return;
                    case "Change status":
                        LibraryManager.ChangeBookStatus(selectedBook, bookList, FileName);
                        return;
                    case "Remove":
                        LibraryManager.RemoveBook(selectedBook, bookList, FileName);
                        return;
                    case "Return":
                        return;
                }
            }
        }

        private static Volume? SearchSingleBook(string parameter, string query, string statusMessage)
        {
            var bookService = Program.BookService ?? throw new InvalidOperationException("BookService is not initialized.");
            return AnsiConsole.Status().Start(statusMessage, ctx =>
            {
                return bookService.SearchBook(parameter, query).GetAwaiter().GetResult();
            });
        }

        private static Volumes? SearchBooks(string parameter, string query, string statusMessage)
        {
            var bookService = Program.BookService ?? throw new InvalidOperationException("BookService is not initialized.");
            return AnsiConsole.Status().Start(statusMessage, ctx =>
            {
                return bookService.SearchBooks(parameter, query).GetAwaiter().GetResult();
            });
        }

        private static bool HasResults(Volumes? result)
        {
            return result != null && result.Items != null && result.Items.Count > 0;
        }

        private static void TryAddSelectedBooks(IList<Volume> items, string fileName, string Mode)
        {
            try
            {
                var choices = GetBookChoices(items);
                var selections = Prompt.MultiSelect("Select books to add to " + Mode, choices);

                Console.WriteLine("Selected books:");
                foreach (var selected in selections)
                {
                    Console.WriteLine(selected);
                }

                if (!Prompt.Confirm("Confirm add selected books to " + Mode + "?"))
                {
                    Console.WriteLine("Books not added.");
                    return;
                }

                var bookList = LibraryStorage.LoadBookList(fileName);
                foreach (var item in items)
                {
                    var label = GetBookLabel(item);
                    if (selections.Contains(label))
                    {
                        bookList.Add(ToBook(item.VolumeInfo, Mode));
                    }
                }

                LibraryStorage.UpdateBooks(bookList, fileName);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Book selection cancelled. No books added.");
            }
        }

        private static string[] GetBookChoices(IEnumerable<Volume> items)
        {
            return items.Select(GetBookLabel).ToArray();
        }

        private static string GetBookLabel(Volume item)
        {
            var info = item.VolumeInfo;
            var title = info?.Title ?? string.Empty;
            var authors = string.Join(", ", info?.Authors ?? new List<string>());
            var publishedDate = info?.PublishedDate ?? "Unknown";
            return $"{title} by {authors} ({publishedDate})";
        }

        private static void ConfirmAndSaveBook(Volume.VolumeInfoData? volumeInfo, string fileName, string Mode)
        {
            if (volumeInfo == null)
            {
                Console.WriteLine("Book information is unavailable.");
                return;
            }

            var label = GetBookLabel(volumeInfo);
            if (!Prompt.Confirm($"Confirm add book {label} to {Mode}?"))
            {
                Console.WriteLine("Book not added.");
                return;
            }

            LibraryStorage.SaveBook(ToBook(volumeInfo, Mode), fileName);
        }

        private static string GetBookLabel(Volume.VolumeInfoData volumeInfo)
        {
            var title = volumeInfo.Title ?? string.Empty;
            var authors = string.Join(", ", volumeInfo.Authors ?? new List<string>());
            var publishedDate = volumeInfo.PublishedDate ?? "Unknown";
            return $"{title} by {authors} ({publishedDate})";
        }

        private static Book ToBook(Volume.VolumeInfoData volumeInfo, string Mode)
        {
            string status = Mode == "wishlist" ? "In wishlist" : "Not started";
            return new Book
            {
                Title = volumeInfo.Title,
                Author = string.Join(", ", volumeInfo.Authors ?? new List<string>()),
                PublicationDate = DateTime.TryParse(volumeInfo.PublishedDate, out DateTime pubDate) ? pubDate.Year : DateTime.MinValue.Year,
                PageCount = volumeInfo.PageCount ?? 0,
                Status = status
            };
        }
    }
}
