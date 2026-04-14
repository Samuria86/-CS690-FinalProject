using Xunit;
using Virtual_Bookshelf.Library;
using Virtual_Bookshelf.Library.Services;
using Virtual_Bookshelf.Library.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Library.UnitTests.Services
{
    public class BookSearchTest
    {
        const string apiKey = "AIzaSyAQMecOCUAHAqtKR_n-iZcgUaUgn8G6GPw";
        [Fact]
        public async Task TestISBNSearch()
        {
            string isbn = "0071807993";
            string parameter = "isbn";
            var bookService = new BookService(apiKey);
            var result = await bookService.SearchBooks(parameter, isbn);
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.Items.Count > 0);
            var book = result.Items[0];
            Console.WriteLine("\nBook Name: " + book.VolumeInfo.Title);
            Console.WriteLine("Authors: " + string.Join(", ", book.VolumeInfo.Authors ?? new List<string>()));
            Console.WriteLine("Publisher: " + book.VolumeInfo.Publisher);
        }
        [Fact]
        public async Task TestTitleSearch()
        {
            string title = "The Great Gatsby";
            string parameter = "intitle";
            var bookService = new BookService(apiKey);
            var result = await bookService.SearchBooks(parameter, title);
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.Items.Count > 0);
            var book = result.Items[0];
            Console.WriteLine("\nBook Name: " + book.VolumeInfo.Title);
            Console.WriteLine("Authors: " + string.Join(", ", book.VolumeInfo.Authors ?? new List<string>()));
            Console.WriteLine("Publisher: " + book.VolumeInfo.Publisher);
        }

        [Fact]
        public async Task TestAuthorSearch()
        {
            string author = "F. Scott Fitzgerald";
            string parameter = "inauthor";
            var bookService = new BookService(apiKey);
            var result = await bookService.SearchBooks(parameter, author);
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.True(result.Items.Count > 0);
        }
    }

    public class BookModelTest
    {
        [Fact]
        public void BookModel_CreateBook_WithAllProperties_Success()
        {
            // Arrange
            var title = "Test Book";
            var author = "Test Author";
            int publicationDate = 2024;
            int pageCount = 300;
            var status = "Reading";

            // Act
            var book = new Book
            {
                Title = title,
                Author = author,
                PublicationDate = publicationDate,
                PageCount = pageCount,
                Status = status
            };

            // Assert
            Assert.Equal(title, book.Title);
            Assert.Equal(author, book.Author);
            Assert.Equal(publicationDate, book.PublicationDate);
            Assert.Equal(pageCount, book.PageCount);
            Assert.Equal(status, book.Status);
        }

        [Fact]
        public void BookModel_CreateBook_WithDefaults_Success()
        {
            // Arrange & Act
            var book = new Book
            {
                Title = "Book Title",
                Author = "Author Name"
            };

            // Assert
            Assert.Equal("Book Title", book.Title);
            Assert.Equal("Author Name", book.Author);
            Assert.Equal("Not started", book.Status);
            Assert.Equal(0, book.PageCount);
        }

        [Fact]
        public void BookModel_JsonSerialization_Success()
        {
            // Arrange
            var book = new Book
            {
                Title = "JSON Test",
                Author = "Test Author",
                PublicationDate = 2025,
                PageCount = 250,
                Status = "Completed"
            };

            // Act
            var json = JsonSerializer.Serialize(book);
            var deserializedBook = JsonSerializer.Deserialize<Book>(json);

            // Assert
            Assert.NotNull(deserializedBook);
            Assert.Equal(book.Title, deserializedBook.Title);
            Assert.Equal(book.Author, deserializedBook.Author);
            Assert.Equal(book.PublicationDate, deserializedBook.PublicationDate);
        }

        [Theory]
        [InlineData("Novel", "Author One", 2020, 400)]
        [InlineData("Mystery", "Author Two", 2021, 350)]
        [InlineData("Science Fiction", "Author Three", 2022, 500)]
        public void BookModel_MultipleBooks_AllPropertiesSet_Success(string title, string author, int year, int pages)
        {
            // Act
            var book = new Book
            {
                Title = title,
                Author = author,
                PublicationDate = year,
                PageCount = pages
            };

            // Assert
            Assert.Equal(title, book.Title);
            Assert.Equal(author, book.Author);
            Assert.Equal(year, book.PublicationDate);
            Assert.Equal(pages, book.PageCount);
        }
    }

    public class BookStoragePersistenceTest
    {
        private string GetTestFilePath() => Path.Combine(Path.GetTempPath(), $"library_test_{Guid.NewGuid()}.dat");

        [Fact]
        public void BookStorage_SaveAndLoadBooks_Success()
        {
            // Arrange
            var testFile = GetTestFilePath();
            var books = new List<Book>
            {
                new Book { Title = "Book 1", Author = "Author 1", PublicationDate = 2020, PageCount = 300 },
                new Book { Title = "Book 2", Author = "Author 2", PublicationDate = 2021, PageCount = 250 }
            };

            // Act
            LibraryStorage.SaveBookList(books, testFile);
            var loadedBooks = LibraryStorage.LoadBookList(testFile);

            // Assert
            Assert.NotNull(loadedBooks);
            Assert.Equal(2, loadedBooks.Count);
            Assert.Equal("Book 1", loadedBooks[0].Title);
            Assert.Equal("Book 2", loadedBooks[1].Title);

            // Cleanup
            if (File.Exists(testFile)) File.Delete(testFile);
        }

        [Fact]
        public void BookStorage_LoadEmptyFile_ReturnsEmptyList()
        {
            // Arrange
            var testFile = GetTestFilePath();
            File.WriteAllText(testFile, "");

            // Act
            var books = LibraryStorage.LoadBookList(testFile);

            // Assert
            Assert.NotNull(books);
            Assert.Empty(books);

            // Cleanup
            if (File.Exists(testFile)) File.Delete(testFile);
        }

        [Fact]
        public void BookStorage_LoadNonexistentFile_ReturnsEmptyList()
        {
            // Arrange
            var testFile = GetTestFilePath();

            // Act
            var books = LibraryStorage.LoadBookList(testFile);

            // Assert
            Assert.NotNull(books);
            Assert.Empty(books);
        }

        [Fact]
        public void BookStorage_SaveMultipleBooks_Readable()
        {
            // Arrange
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

            // Act
            LibraryStorage.SaveBookList(books, testFile);
            var loadedBooks = LibraryStorage.LoadBookList(testFile);

            // Assert
            Assert.NotNull(loadedBooks);
            Assert.Equal(5, loadedBooks.Count);
            Assert.Equal("Book 5", loadedBooks[4].Title);
            Assert.Equal(400, loadedBooks[4].PageCount);

            // Cleanup
            if (File.Exists(testFile)) File.Delete(testFile);
        }

        [Fact]
        public void BookStorage_SaveBook_AppendsBookToExistingMemoryMappedFile()
        {
            // Arrange
            var testFile = GetTestFilePath();
            var initialBooks = new List<Book>
            {
                new Book { Title = "Initial Book", Author = "Initial Author", PublicationDate = 2022, PageCount = 200 }
            };
            LibraryStorage.SaveBookList(initialBooks, testFile);

            var newBook = new Book { Title = "New Book", Author = "New Author", PublicationDate = 2023, PageCount = 300 };

            // Act
            LibraryStorage.SaveBook(newBook, testFile);
            var loadedBooks = LibraryStorage.LoadBookList(testFile);

            // Assert
            Assert.NotNull(loadedBooks);
            Assert.Equal(2, loadedBooks.Count);
            Assert.Equal("Initial Book", loadedBooks[0].Title);
            Assert.Equal("New Book", loadedBooks[1].Title);

            // Cleanup
            if (File.Exists(testFile)) File.Delete(testFile);
        }
    }

}
