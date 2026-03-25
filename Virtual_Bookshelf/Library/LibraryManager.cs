using Spectre.Console;
using Sharprompt;
using System.Globalization;
using System.Text.Json;
using Virtual_Bookshelf.Library.Models;
using Virtual_Bookshelf.Library.Services;

namespace Virtual_Bookshelf.Library
{
    public partial class Program
    {
        static string apiKey = "AIzaSyAQMecOCUAHAqtKR_n-iZcgUaUgn8G6GPw";
        static BookService bookService = new BookService(apiKey);

        static void ViewLibrary()
        {
            Console.WriteLine("Viewing library...");
            string fileName = "library.json";
            List<Book> bookList;

            if (File.Exists(fileName))
            {
                string data = File.ReadAllText(fileName);
                if (string.IsNullOrWhiteSpace(data))
                {
                    Console.WriteLine("Your library is empty.");
                    return;
                }
                bookList = JsonSerializer.Deserialize<List<Book>>(data) ?? new List<Book>();
            }
            else
            {
                bookList = new List<Book>();
            }

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
                    table.AddRow(book.Title, book.Author, book.PublicationDate.ToString("MMM yyyy", CultureInfo.InvariantCulture), book.PageCount.ToString(), book.Status);
                }
                AnsiConsole.Write(table);
            }
        }

        static void AddBook()
        {
            var entryMethod = Prompt.Select("Add book by", new[]
            {
                "Manual entry", "Google Books search (requires API key)", "Return"
            });
            switch (entryMethod)
            {
                case "Manual entry":
                    Console.WriteLine("Adding book manually, enter 'exit' to stop...");
                    while (true)
                    {
                        Console.Write("Title: ");
                        string title = Console.ReadLine();
                        if (title.ToLower() == "exit") break;
                        Console.Write("Author: ");
                        string author = Console.ReadLine();
                        if (author.ToLower() == "exit") break;
                        Console.Write("Publication Date (yyyy-MM-dd): ");
                        string publicationDate = Console.ReadLine();
                        if (publicationDate.ToLower() == "exit") break;
                        Console.Write("Page Count: ");
                        string PageCount = Console.ReadLine();
                        if (PageCount.ToLower() == "exit") break;
                        Console.WriteLine($"Add book: {title} by {author}, published on {publicationDate}, Page Count: {PageCount}?");
                        var confirm = Prompt.Confirm("Confirm add book?");
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
                                Title = title,
                                Author = author,
                                PublicationDate = DateTime.Parse(publicationDate),
                                PageCount = int.TryParse(PageCount, out int pageCount) ? pageCount : 0
                            };
                            bookList.Add(bookData);
                            string jsonData = JsonSerializer.Serialize(bookList, new JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText(fileName, jsonData);

                            break;
                        }
                        else
                        {
                            Console.WriteLine("Book not added.");
                            break;
                        }
                    }
                    break;
                case "Google Books search (requires API key)":
                    var method = Prompt.Select("Choose search method", new[]
                    {
                        "Search by ISBN", "Search by Title", "Search by Author", "Return"
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
                        case "Return":
                            return;
                    }
                    break;
                case "Return":
                    return;
            }
        }

        static void SearchBookByISBN()
        {
            Console.Write("Enter ISBN: ");
            string isbn = Console.ReadLine();
            string parameter = "isbn";
            var result = bookService.SearchBook(parameter, isbn).GetAwaiter().GetResult();
            if (result != null)
            {
                Console.WriteLine("\nBook Name: " + result.VolumeInfo.Title);
                Console.WriteLine("Authors: " + string.Join(", ", result.VolumeInfo.Authors));
                Console.WriteLine("Publisher: " + result.VolumeInfo.Publisher);
            }
            else
            {
                Console.WriteLine("No results found for the given ISBN.");
            }
        }

        static void SearchBookByTitle()
        {
            Console.Write("Enter Title: ");
            string title = Console.ReadLine();
            string parameter = "intitle";
            var result = bookService.SearchBooks(parameter, title).GetAwaiter().GetResult();
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
                                PublicationDate = DateTime.TryParse(book.VolumeInfo.PublishedDate, out DateTime pubDate) ? pubDate : DateTime.MinValue,
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
            Console.Write("Enter Author: ");
            string author = Console.ReadLine();
            string parameter = "inauthor";
            var result = bookService.SearchBooks(parameter, author).GetAwaiter().GetResult();
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
                                PublicationDate = DateTime.TryParse(book.VolumeInfo.PublishedDate, out DateTime pubDate) ? pubDate : DateTime.MinValue,
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

        static void SearchFilterBooks()
        {
            // TODO: Implement search/filter books functionality
            throw new NotImplementedException("Search/filter books functionality is not implemented yet.");
        }

        static void ExportLibrary()
        {
            // TODO: Implement export library functionality
            Console.WriteLine("Exporting library...");
        }

        static void WishlistMenu()
        {
            // TODO: Implement wishlist functionality
            throw new NotImplementedException("Wishlist menu is not implemented yet.");
        }
    }
}