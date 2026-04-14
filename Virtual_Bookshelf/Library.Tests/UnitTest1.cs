using Xunit;
using Virtual_Bookshelf.Library;
using Virtual_Bookshelf.Library.Services;
using Virtual_Bookshelf.Library.Models;
using System.Diagnostics;
using System.IO;


namespace Library.UnitTests.Services
{
    public class BookSearchTest
    {
        [Fact]
        public async Task TestISBNSearch()
        {
            var apiKey = ApiKeyManager.GetApiKey();
            string isbn = "0071807993";
            string parameter = "isbn";
            var bookService = new BookService(apiKey);
            var result = await bookService.SearchBooks(parameter, isbn);
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.Items.Count > 0);
        }
        [Fact]
        public async Task TestTitleSearch()
        {
            var apiKey = ApiKeyManager.GetApiKey();
            string title = "The Great Gatsby";
            string parameter = "intitle";
            var bookService = new BookService(apiKey);
            var result = await bookService.SearchBooks(parameter, title);
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.Items.Count > 0);
        }

        [Fact]
        public async Task TestAuthorSearch()
        {
            var apiKey = ApiKeyManager.GetApiKey();
            string author = "F. Scott Fitzgerald";
            string parameter = "inauthor";
            var bookService = new BookService(apiKey);
            var result = await bookService.SearchBooks(parameter, author);
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.Items.Count > 0);
        }
    }


    public class BookStoragePersistenceTest
    {
        private string GetTestFilePath() => Path.Combine(Path.GetTempPath(), $"library_test_{Guid.NewGuid()}.dat");

        [Fact]
        public void SaveAndLoadBooks_Success()
        {
            var testFile = GetTestFilePath();
            var books = new List<Book>
            {
                new Book { Title = "Book 1", Author = "Author 1", PublicationDate = 2020, PageCount = 300 },
                new Book { Title = "Book 2", Author = "Author 2", PublicationDate = 2021, PageCount = 250 }
            };
            LibraryStorage.SaveBookList(books, testFile);
            var loadedBooks = LibraryStorage.LoadBookList(testFile);
            Assert.NotNull(loadedBooks);
            Assert.Equal(2, loadedBooks.Count);
            Assert.Equal("Book 1", loadedBooks[0].Title);
            Assert.Equal("Book 2", loadedBooks[1].Title);
            if (File.Exists(testFile)) File.Delete(testFile);
        }

        [Fact]
        public void LoadEmptyFile_ReturnsEmptyList()
        {

            var testFile = GetTestFilePath();
            File.WriteAllText(testFile, "");
            var books = LibraryStorage.LoadBookList(testFile);
            Assert.NotNull(books);
            Assert.Empty(books);
            if (File.Exists(testFile)) File.Delete(testFile);
        }

        [Fact]
        public void LoadNonexistentFile_ReturnsEmptyList()
        {
            var testFile = GetTestFilePath();
            var books = LibraryStorage.LoadBookList(testFile);
            Assert.NotNull(books);
            Assert.Empty(books);
        }

        [Fact]
        public void SaveMultipleBooks()
        {

            var testFile = GetTestFilePath();
            var books = new List<Book>();
            for (int i = 0; i < 5; i++)
            {
                books.Add(new Book
                {
                    Title = $"Book {i + 1}",
                    Author = $"Author {i + 1}",
                    PublicationDate = 2020 + i,
                    PageCount = 200 + (i * 50)
                });
            }
            LibraryStorage.SaveBookList(books, testFile);
            var loadedBooks = LibraryStorage.LoadBookList(testFile);
            Assert.NotNull(loadedBooks);
            Assert.Equal(5, loadedBooks.Count);
            Assert.Equal("Book 5", loadedBooks[4].Title);
            Assert.Equal(400, loadedBooks[4].PageCount);
            if (File.Exists(testFile)) File.Delete(testFile);
        }

        [Fact]
        public void SaveBook_AppendsBookToMemoryMappedFile()
        {

            var testFile = GetTestFilePath();
            var initialBooks = new List<Book>
            {
                new Book { Title = "Initial Book", Author = "Initial Author", PublicationDate = 2022, PageCount = 200 }
            };
            LibraryStorage.SaveBookList(initialBooks, testFile);

            var newBook = new Book { Title = "New Book", Author = "New Author", PublicationDate = 2023, PageCount = 300 };
            LibraryStorage.SaveBook(newBook, testFile);
            var loadedBooks = LibraryStorage.LoadBookList(testFile);
            Assert.NotNull(loadedBooks);
            Assert.Equal(2, loadedBooks.Count);
            Assert.Equal("Initial Book", loadedBooks[0].Title);
            Assert.Equal("New Book", loadedBooks[1].Title);
            if (File.Exists(testFile)) File.Delete(testFile);
        }
    }


    public class LibraryManagerTests
    {
        private static string GetTestFilePath() => Path.Combine(Path.GetTempPath(), $"library_manager_test_{Guid.NewGuid()}.dat");


        [Fact]
        public void TestEmptyLibraryMessage()
        {
            var testFile = GetTestFilePath();
            var originalOut = Console.Out;
            try
            {
                using var output = new StringWriter();
                Console.SetOut(output);

                LibraryManager.ViewBooks(testFile, "library");

                var result = output.ToString();
                Assert.Contains("Your library is empty.", result);
            }
            finally
            {
                Console.SetOut(originalOut);
                if (File.Exists(testFile))
                {
                    File.Delete(testFile);
                }
            }
        }

        [Fact]
        public void TestViewBookDetails()
        {
            var book = new Book
            {
                Title = "Test Title",
                Author = "Test Author",
                PublicationDate = 2026,
                PageCount = 100,
                Status = "Reading",
                DateAdded = new DateTime(2024, 1, 1),
                DateFinished = null
            };

            var originalOut = Console.Out;
            try
            {
                using var output = new StringWriter();
                Console.SetOut(output);

                LibraryManager.ViewBookDetails("Test Title by Test Author", new List<Book> { book });

                var result = output.ToString();
                Console.WriteLine("Book Details Output:");
                Console.WriteLine(result);
                Assert.Contains("Title: Test Title", result);
                Assert.Contains("Author: Test Author", result);
                Assert.Contains("Publication Date: 2026", result);
                Assert.Contains("Page Count: 100", result);
                Assert.Contains("Status: Reading", result);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

    }
}
