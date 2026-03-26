using Spectre.Console;
using Sharprompt;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Text.Json;
using Virtual_Bookshelf.Library.Models;
using Virtual_Bookshelf.Library.Services;

namespace Virtual_Bookshelf.Library
{
    public partial class Program
    {
        static void ViewLibrary()
        {
            Console.WriteLine("Viewing library...");
            string fileName = "library.json";
            List<Book> bookList = LoadBookList(fileName);

            if (bookList.Count == 0)
            {
                Console.WriteLine("Your library is empty.");
            }
            else
            {
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
        }

        static void AddBook()
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
                    BookService = new BookService(apiKey);
                    var method = Prompt.Select("Choose search method", new[]
                    {
                        "Search by ISBN", "Search by Title", "Search by Author", "new API key", "Return"
                    });
                    switch (method)
                    {
                        case "Search by ISBN":
                            SearchBookByISBN();
                            break;
                        case "Search by Title":
                            SearchBookByTitle();
                            break;
                        case "Search by Author":
                            SearchBookByAuthor();
                            break;
                        case "new API key":
                            string newApiKey = Prompt.Input<string>("Enter new API key", validators: new[]
                            {
                                Validators.Required()
                            });
                            ApiKeyManager.SaveApiKey(newApiKey);
                            BookService = new BookService(newApiKey);
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

        static void AddBookManually()
        {
            Console.WriteLine("Adding book manually, enter 'exit' to stop...");

            string title = GetRequiredInput("Title");
            if (title.ToLower() == "exit") return;

            string author = GetRequiredInput("Author");
            if (author.ToLower() == "exit") return;

            string publicationYear = GetValidYearInput("Publication Year (yyyy)");
            if (publicationYear.ToLower() == "exit") return;

            string pageCountStr = GetRequiredInput("Page Count");
            if (pageCountStr.ToLower() == "exit") return;

            Console.WriteLine($"Add book: {title} by {author}, publication year {publicationYear}, Page Count: {pageCountStr}?");
            var confirm = Prompt.Confirm("Confirm add book?");

            if (confirm)
            {
                SaveBookToLibrary(title, author, int.Parse(publicationYear), pageCountStr);
                Console.WriteLine("Book added successfully!");
            }
            else
            {
                Console.WriteLine("Book not added.");
            }
        }

        static string GetRequiredInput(string fieldName)
        {
            string input = Prompt.Input<string>($"{fieldName}: ", validators: new[] { Validators.Required() });
            return input;
        }

        static string GetValidYearInput(string fieldName)
        {
            string input = Prompt.Input<string>($"{fieldName}: ", validators: new[] { Validators.Required(), Validators.RegularExpression(@"^\d{4}$", "Please enter a valid year in yyyy format") });

            if (int.TryParse(input, out int year) && year >= 1 && year <= DateTime.Now.Year)
            {
                return input;
            }

            Console.WriteLine("Invalid year. Please enter a year between 1 and " + DateTime.Now.Year + ".");
            return GetValidYearInput(fieldName);
        }

        static void SaveBookToLibrary(string title, string author, int publicationYear, string pageCountStr)
        {
            string fileName = "library.json";
            List<Book> bookList = LoadBookList(fileName);

            var book = new Book
            {
                Title = title,
                Author = author,
                PublicationDate = publicationYear,
                PageCount = int.TryParse(pageCountStr, out int pageCount) ? pageCount : 0
            };

            bookList.Add(book);
            string jsonData = JsonSerializer.Serialize(bookList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(fileName, jsonData);
        }

        static List<Book> LoadBookList(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return new List<Book>();
            }

            string data = File.ReadAllText(fileName);
            if (string.IsNullOrWhiteSpace(data))
            {
                return new List<Book>();
            }

            return JsonSerializer.Deserialize<List<Book>>(data) ?? new List<Book>();
        }

        static void SearchBookByISBN()
        {
            Console.Write("Enter ISBN: ");
            string isbn = Prompt.Input<string>("ISBN", validators: new[] { Validators.Required(), Validators.RegularExpression(@"^\d{10}(\d{3})?$", "Please enter a valid 10 or 13 digit ISBN") });
            string parameter = "isbn";
            var result = AnsiConsole.Status().Start("Searching for book...", ctx =>
            {
                return BookService.SearchBook(parameter, isbn).GetAwaiter().GetResult();
            });
            if (result != null)
            {
                var confirm = Prompt.Confirm("Confirm add book " + result.VolumeInfo.Title + " by " + string.Join(", ", result.VolumeInfo.Authors ?? new List<string>()) + " to library?");
                if (confirm)
                {
                    string fileName = "library.json";
                    List<Book> bookList;
                    if (File.Exists(fileName))
                    {
                        string data = File.ReadAllText(fileName);
                        if (string.IsNullOrWhiteSpace(data))
                        {
                            bookList = new List<Book>();
                        }
                        else
                        {
                            bookList = JsonSerializer.Deserialize<List<Book>>(data) ?? new List<Book>();
                        }
                    }
                    else
                    {
                        bookList = new List<Book>();
                    }
                    var bookData = new Book
                    {
                        Title = result.VolumeInfo.Title,
                        Author = string.Join(", ", result.VolumeInfo.Authors ?? new List<string>()),
                        PublicationDate = DateTime.TryParse(result.VolumeInfo.PublishedDate, out DateTime pubDate) ? pubDate.Year : DateTime.MinValue.Year,
                        PageCount = result.VolumeInfo.PageCount ?? 0
                    };
                    bookList.Add(bookData);
                    string jsonData = JsonSerializer.Serialize(bookList, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(fileName, jsonData);
                }
                else
                {
                    Console.WriteLine("Book not added.");
                }
            }
            else
            {
                Console.WriteLine("No results found for the given ISBN.");
            }
        }

        static void SearchBookByTitle()
        {
            Console.Write("Enter Title: ");
            string title = Prompt.Input<string>("Title", validators: new[] { Validators.Required() });
            string parameter = "intitle";
            var result = BookService.SearchBooks(parameter, title).GetAwaiter().GetResult();
            if (result != null && result.Items != null && result.Items.Count > 0)
            {
                var books = Prompt.MultiSelect("Select books to add to library", result.Items.Select(b => b.VolumeInfo.Title + " by " + string.Join(", ", b.VolumeInfo.Authors ?? new List<string>())).ToArray());
                Console.WriteLine("Selected books:");
                foreach (var selected in books)
                {
                    Console.WriteLine(selected);
                }
                var confirm = Prompt.Confirm("Confirm add selected books to library?");
                if (confirm)
                {
                    string fileName = "library.json";
                    List<Book> bookList;
                    if (File.Exists(fileName))
                    {
                        string data = File.ReadAllText(fileName);
                        if (string.IsNullOrWhiteSpace(data))
                        {
                            bookList = new List<Book>();
                        }
                        else
                        {
                            bookList = JsonSerializer.Deserialize<List<Book>>(data) ?? new List<Book>();
                        }
                    }
                    else
                    {
                        bookList = new List<Book>();
                    }
                    foreach (var book in result.Items)
                    {
                        string bookTitle = book.VolumeInfo.Title + " by " + string.Join(", ", book.VolumeInfo.Authors ?? new List<string>());
                        if (books.Contains(bookTitle))
                        {
                            var bookData = new Book
                            {
                                Title = book.VolumeInfo.Title,
                                Author = string.Join(", ", book.VolumeInfo.Authors ?? new List<string>()),
                                PublicationDate = DateTime.TryParse(book.VolumeInfo.PublishedDate, out DateTime pubDate) ? pubDate.Year : DateTime.MinValue.Year,
                                PageCount = book.VolumeInfo.PageCount ?? 0
                            };
                            bookList.Add(bookData);
                        }
                    }
                    string jsonData = JsonSerializer.Serialize(bookList, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(fileName, jsonData);
                }
                else
                {
                    Console.WriteLine("Books not added.");
                }
            }
            else
            {
                Console.WriteLine("No results found for the given Title.");
            }
        }

        static void SearchBookByAuthor()
        {
            Console.Write("Enter Author");
            string author;
            do
            {
                author = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(author))
                {
                    Console.WriteLine("Author is required. Please enter a value.");
                }
            } while (string.IsNullOrWhiteSpace(author));
            string parameter = "inauthor";
            var result = BookService.SearchBooks(parameter, author).GetAwaiter().GetResult();
            if (result != null && result.Items != null && result.Items.Count > 0)
            {
                var books = Prompt.MultiSelect("Select books to add to library", result.Items.Select(b => b.VolumeInfo.Title + " by " + string.Join(", ", b.VolumeInfo.Authors ?? new List<string>())).ToArray());
                Console.WriteLine("Selected books:");
                foreach (var selected in books)
                {
                    Console.WriteLine(selected);
                }
                var confirm = Prompt.Confirm("Confirm add selected books to library?");
                if (confirm)
                {
                    string fileName = "library.json";
                    List<Book> bookList;
                    if (File.Exists(fileName))
                    {
                        string data = File.ReadAllText(fileName);
                        if (string.IsNullOrWhiteSpace(data))
                        {
                            bookList = new List<Book>();
                        }
                        else
                        {
                            bookList = JsonSerializer.Deserialize<List<Book>>(data) ?? new List<Book>();
                        }
                    }
                    else
                    {
                        bookList = new List<Book>();
                    }
                    foreach (var book in result.Items)
                    {
                        string bookTitle = book.VolumeInfo.Title + " by " + string.Join(", ", book.VolumeInfo.Authors ?? new List<string>());
                        if (books.Contains(bookTitle))
                        {
                            var bookData = new Book
                            {
                                Title = book.VolumeInfo.Title,
                                Author = string.Join(", ", book.VolumeInfo.Authors ?? new List<string>()),
                                PublicationDate = DateTime.TryParse(book.VolumeInfo.PublishedDate, out DateTime pubDate) ? pubDate.Year : DateTime.MinValue.Year,
                                PageCount = book.VolumeInfo.PageCount ?? 0
                            };
                            bookList.Add(bookData);
                        }
                    }
                    string jsonData = JsonSerializer.Serialize(bookList, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(fileName, jsonData);
                }
                else
                {
                    Console.WriteLine("Books not added.");
                }
            }
            else
            {
                Console.WriteLine("No results found for the given Author.");
            }
        }

        static void EditRemoveBook()
        {
            string fileName = "library.json";
            List<Book> bookList = LoadBookList(fileName);

            if (bookList.Count == 0)
            {
                Console.WriteLine("Your library is empty.");
            }
            else
            {
                Console.WriteLine("Book list:");
                var selectedBook = Prompt.Select("Select a book to edit/remove", bookList.Select(b => b.Title + " by " + b.Author).ToArray());
                var selection = Prompt.Select("Choose action", new[] { "Edit", "Remove", "Return" });
                switch (selection)
                {
                    case "Edit":
                        EditBook(selectedBook, bookList, fileName);
                        break;
                    case "Remove":
                        RemoveBook(selectedBook, bookList, fileName);
                        break;
                    case "Return":
                        return;
                }
            }
        }

        static void EditBook(string selectedBook, List<Book> bookList, string fileName)
        {
            var book = bookList.FirstOrDefault(b => (b.Title + " by " + b.Author) == selectedBook);
            if (book != null)
            {
                Console.WriteLine("Editing book: " + selectedBook);
                string newTitle = Prompt.Input<string>("New Title (leave blank to keep current)", defaultValue: book.Title);
                string newAuthor = Prompt.Input<string>("New Author (leave blank to keep current)", defaultValue: book.Author);
                string newPublicationDate = Prompt.Input<string>("New Publication Date (yyyy-MM-dd, leave blank to keep current)", defaultValue: book.PublicationDate.ToString("yyyy-MM-dd"));
                string newPageCountStr = Prompt.Input<string>("New Page Count (leave blank to keep current)", defaultValue: book.PageCount.ToString());

                if (!string.IsNullOrWhiteSpace(newTitle)) book.Title = newTitle;
                if (!string.IsNullOrWhiteSpace(newAuthor)) book.Author = newAuthor;
                if (!string.IsNullOrWhiteSpace(newPublicationDate) && DateTime.TryParse(newPublicationDate, out DateTime pubDate)) book.PublicationDate = pubDate.Year;
                if (!string.IsNullOrWhiteSpace(newPageCountStr) && int.TryParse(newPageCountStr, out int pageCount)) book.PageCount = pageCount;

                string jsonData = JsonSerializer.Serialize(bookList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(fileName, jsonData);
                Console.WriteLine("Book updated successfully!");
            }
        }

        static void RemoveBook(string selectedBook, List<Book> bookList, string fileName)
        {
            var book = bookList.FirstOrDefault(b => (b.Title + " by " + b.Author) == selectedBook);
            if (book != null)
            {
                var confirm = Prompt.Confirm("Are you sure you want to remove this book?");
                if (confirm)
                {
                    bookList.Remove(book);
                    string jsonData = JsonSerializer.Serialize(bookList, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(fileName, jsonData);
                    Console.WriteLine("Book removed successfully!");
                }
                else
                {
                    Console.WriteLine("Book not removed.");
                }
            }
        }

        static void SearchFilterBooks()
        {
            // TODO: Implement search/filter books functionality
            throw new NotImplementedException("Search/filter books functionality is not implemented yet.");
        }

        static void ExportLibrary()
        {
            // TODO: Implement export library functionality
            throw new NotImplementedException("Export library functionality is not implemented yet.");
        }

        static void WishlistMenu()
        {
            // TODO: Implement wishlist functionality
            throw new NotImplementedException("Wishlist menu is not implemented yet.");
        }

        static void GoalsStatisticsMenu()
        {
            // TODO: Implement goals and statistics functionality
            throw new NotImplementedException("Goals & statistics menu is not implemented yet.");
        }
    }
}
