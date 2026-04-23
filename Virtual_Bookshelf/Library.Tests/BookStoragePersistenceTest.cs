using Library.Services;
using Library.Models;


namespace Library.Tests
{
    public class BookStoragePersistenceTest
    {
        private string GetTestFilePath() => Path.Combine(Path.GetTempPath(), $"library_test_{Guid.NewGuid()}.dat");

        [Fact]
        public void SaveAndLoadBooks_Success()
        {
            var testFile = GetTestFilePath();
            var books = new List<Book>
            {
                new() { Title = "Book 1", Author = "Author 1", PublicationDate = 2020, PageCount = 300 },
                new() { Title = "Book 2", Author = "Author 2", PublicationDate = 2021, PageCount = 250 }
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
                new () { Title = "Initial Book", Author = "Initial Author", PublicationDate = 2022, PageCount = 200 }
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
}
