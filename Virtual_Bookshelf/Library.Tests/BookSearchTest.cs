using Xunit;
using Library.Services;
using System.Diagnostics;


namespace Library.Tests
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
}
