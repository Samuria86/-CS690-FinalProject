using Xunit;
using Virtual_Bookshelf.Library.Services;
using System.Diagnostics;

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
    }

}
