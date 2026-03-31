using Spectre.Console;
using Sharprompt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Virtual_Bookshelf.Library.Models;
using Virtual_Bookshelf.Library.Services;
using System.Linq.Expressions;

namespace Virtual_Bookshelf.Library
{
    public static class LibrarySearch
    {
        public static void SearchBookByISBN()
        {
            Console.Write("Enter ISBN: ");
            string isbn = Prompt.Input<string>("ISBN", validators: new[] { Validators.Required(), Validators.RegularExpression(@"^\d{10}(\d{3})?$", "Please enter a valid 10 or 13 digit ISBN") });
            string parameter = "isbn";
            var bookService = Program.BookService ?? throw new InvalidOperationException("BookService is not initialized.");
            var result = AnsiConsole.Status().Start("Searching for book...", ctx =>
            {
                return bookService.SearchBook(parameter, isbn).GetAwaiter().GetResult();
            });

            if (result == null)
            {
                Console.WriteLine("No results found for the given ISBN.");
                return;
            }

            var confirm = Prompt.Confirm("Confirm add book " + result.VolumeInfo.Title + " by " + string.Join(", ", result.VolumeInfo.Authors ?? new List<string>()) + " to library?");
            if (!confirm)
            {
                Console.WriteLine("Book not added.");
                return;
            }

            var book = new Book
            {
                Title = result.VolumeInfo.Title,
                Author = string.Join(", ", result.VolumeInfo.Authors ?? new List<string>()),
                PublicationDate = DateTime.TryParse(result.VolumeInfo.PublishedDate, out DateTime pubDate) ? pubDate.Year : DateTime.MinValue.Year,
                PageCount = result.VolumeInfo.PageCount ?? 0
            };
            LibraryStorage.SaveBook(book);
        }

        public static void SearchBookByTitle()
        {
            Console.Write("Enter Title: ");
            string title = Prompt.Input<string>("Title", validators: new[] { Validators.Required() });
            string parameter = "intitle";
            var bookService = Program.BookService ?? throw new InvalidOperationException("BookService is not initialized.");
            var result = bookService.SearchBooks(parameter, title).GetAwaiter().GetResult();

            if (result == null || result.Items == null || result.Items.Count == 0)
            {
                Console.WriteLine("No results found for the given Title.");
                return;
            }
            try
            {

                var selections = Prompt.MultiSelect("Select books to add to library", result.Items.Select(b => b.VolumeInfo.Title + " by " + string.Join(", ", b.VolumeInfo.Authors ?? new List<string>()) + " (" + (b.VolumeInfo.PublishedDate ?? "Unknown") + ")").ToArray());
                Console.WriteLine("Selected books:");
                foreach (var selected in selections)
                {
                    Console.WriteLine(selected);
                }

                var confirm = Prompt.Confirm("Confirm add selected books to library?");
                if (!confirm)
                {
                    Console.WriteLine("Books not added.");
                    return;
                }

                var bookList = LibraryStorage.LoadBookList();
                foreach (var item in result.Items)
                {
                    string bookTitle = item.VolumeInfo.Title + " by " + string.Join(", ", item.VolumeInfo.Authors ?? new List<string>());
                    if (selections.Contains(bookTitle))
                    {
                        bookList.Add(new Book
                        {
                            Title = item.VolumeInfo.Title,
                            Author = string.Join(", ", item.VolumeInfo.Authors ?? new List<string>()),
                            PublicationDate = DateTime.TryParse(item.VolumeInfo.PublishedDate, out DateTime pubDate) ? pubDate.Year : DateTime.MinValue.Year,
                            PageCount = item.VolumeInfo.PageCount ?? 0
                        });
                    }
                }
                LibraryStorage.UpdateBooks(bookList);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Book selection cancelled. No books added.");
            }
        }

        public static void SearchBookByAuthor()
        {
            Console.Write("Enter Author: ");
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
            var bookService = Program.BookService ?? throw new InvalidOperationException("BookService is not initialized.");
            var result = bookService.SearchBooks(parameter, author).GetAwaiter().GetResult();

            if (result == null || result.Items == null || result.Items.Count == 0)
            {
                Console.WriteLine("No results found for the given Author.");
                return;
            }

            try
            {
                var selections = Prompt.MultiSelect("Select books to add to library", result.Items.Select(b => b.VolumeInfo.Title + " by " + string.Join(", ", b.VolumeInfo.Authors ?? new List<string>()) + " (" + (b.VolumeInfo.PublishedDate ?? "Unknown") + ")").ToArray());

                Console.WriteLine("Selected books:");
                foreach (var selected in selections)
                {
                    Console.WriteLine(selected);
                }

                var confirm = Prompt.Confirm("Confirm add selected books to library?");
                if (!confirm)
                {
                    Console.WriteLine("Books not added.");
                    return;
                }
                var bookList = LibraryStorage.LoadBookList();
                foreach (var item in result.Items)
                {
                    string bookTitle = item.VolumeInfo.Title + " by " + string.Join(", ", item.VolumeInfo.Authors ?? new List<string>());
                    if (selections.Contains(bookTitle))
                    {
                        bookList.Add(new Book
                        {
                            Title = item.VolumeInfo.Title,
                            Author = string.Join(", ", item.VolumeInfo.Authors ?? new List<string>()),
                            PublicationDate = DateTime.TryParse(item.VolumeInfo.PublishedDate, out DateTime pubDate) ? pubDate.Year : DateTime.MinValue.Year,
                            PageCount = item.VolumeInfo.PageCount ?? 0
                        });
                    }
                }
                LibraryStorage.UpdateBooks(bookList);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Book selection cancelled. No books added.");
            }

        }
    }
}
