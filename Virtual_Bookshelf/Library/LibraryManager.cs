using Spectre.Console;
using Sharprompt;
using System;
using System.Collections.Generic;
using System.Linq;
using Virtual_Bookshelf.Library.Models;
using Virtual_Bookshelf.Library.Services;

namespace Virtual_Bookshelf.Library
{
    public static class LibraryManager
    {
        public static void ViewLibrary()
        {
            Console.WriteLine("Viewing library...");
            var bookList = LibraryStorage.LoadBookList();

            if (bookList.Count == 0)
            {
                Console.WriteLine("Your library is empty.");
                return;
            }

            Console.WriteLine("Your library:");
            var table = new Table();
            table.AddColumn("Title");
            table.AddColumn("Author");
            table.AddColumn("Publication Date");
            table.AddColumn("Page Count");
            table.AddColumn("Status");

            foreach (var book in bookList)
            {
                table.AddRow(book.Title, book.Author, book.PublicationDate.ToString(), book.PageCount.ToString(), book.Status);
            }

            AnsiConsole.Write(table);
        }

        public static void AddBook()
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
                    AddBookManually();
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
                            LibrarySearch.SearchBookByISBN();
                            break;
                        case "Search by Title":
                            LibrarySearch.SearchBookByTitle();
                            break;
                        case "Search by Author":
                            LibrarySearch.SearchBookByAuthor();
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

        public static void AddBookManually()
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
                var book = new Book
                {
                    Title = title,
                    Author = author,
                    PublicationDate = int.Parse(publicationYear),
                    PageCount = pageCount
                };
                LibraryStorage.AddBook(book);
                Console.WriteLine("Book added successfully!");
            }
            else
            {
                Console.WriteLine("Book not added.");
            }
        }

        public static string GetRequiredInput(string fieldName)
        {
            return Prompt.Input<string>($"{fieldName}: ", validators: new[] { Validators.Required() });
        }

        public static string GetValidYearInput(string fieldName)
        {
            string input = Prompt.Input<string>($"{fieldName}: ", validators: new[] { Validators.Required(), Validators.RegularExpression(@"^\d{4}$", "Please enter a valid year in yyyy format") });

            if (int.TryParse(input, out int year) && year >= 1 && year <= DateTime.Now.Year)
            {
                return input;
            }

            Console.WriteLine("Invalid year. Please enter a year between 1 and " + DateTime.Now.Year + ".");
            return GetValidYearInput(fieldName);
        }

        public static void EditRemoveBook()
        {
            var bookList = LibraryStorage.LoadBookList();

            if (bookList.Count == 0)
            {
                Console.WriteLine("Your library is empty.");
                return;
            }

            Console.WriteLine("Book list:");
            var selectedBook = Prompt.Select("Select a book to edit/remove", bookList.Select(b => b.Title + " by " + b.Author).ToArray());
            var selection = Prompt.Select("Choose action", new[] { "Edit", "Remove", "Return" });

            switch (selection)
            {
                case "Edit":
                    EditBook(selectedBook, bookList);
                    break;
                case "Remove":
                    RemoveBook(selectedBook, bookList);
                    break;
                case "Return":
                    return;
            }
        }

        public static void EditBook(string selectedBook, List<Book> bookList)
        {
            var book = bookList.FirstOrDefault(b => (b.Title + " by " + b.Author) == selectedBook);
            if (book == null) return;

            Console.WriteLine("Editing book: " + selectedBook);
            string newTitle = Prompt.Input<string>("New Title (leave blank to keep current)", defaultValue: book.Title);
            string newAuthor = Prompt.Input<string>("New Author (leave blank to keep current)", defaultValue: book.Author);
            string newPublicationDate = Prompt.Input<string>("New Publication Date (yyyy-MM-dd, leave blank to keep current)", defaultValue: book.PublicationDate.ToString("yyyy-MM-dd"));
            string newPageCountStr = Prompt.Input<string>("New Page Count (leave blank to keep current)", defaultValue: book.PageCount.ToString());

            if (!string.IsNullOrWhiteSpace(newTitle)) book.Title = newTitle;
            if (!string.IsNullOrWhiteSpace(newAuthor)) book.Author = newAuthor;
            if (!string.IsNullOrWhiteSpace(newPublicationDate) && DateTime.TryParse(newPublicationDate, out DateTime pubDate)) book.PublicationDate = pubDate.Year;
            if (!string.IsNullOrWhiteSpace(newPageCountStr) && int.TryParse(newPageCountStr, out int pageCount)) book.PageCount = pageCount;

            LibraryStorage.UpdateBooks(bookList);
            Console.WriteLine("Book updated successfully!");
        }

        public static void RemoveBook(string selectedBook, List<Book> bookList)
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
            LibraryStorage.UpdateBooks(bookList);
            Console.WriteLine("Book removed successfully!");
        }

        public static void SearchFilterBooks()
        {
            throw new NotImplementedException("Search/filter books functionality is not implemented yet.");
        }

        public static void ExportLibrary()
        {
            throw new NotImplementedException("Export library functionality is not implemented yet.");
        }

        public static void WishlistMenu()
        {
            throw new NotImplementedException("Wishlist menu is not implemented yet.");
        }

        public static void GoalsStatisticsMenu()
        {
            throw new NotImplementedException("Goals & statistics menu is not implemented yet.");
        }
    }
}
