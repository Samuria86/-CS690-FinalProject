using Xunit;
using Books.Services;
using System.Diagnostics;

namespace Library.UnitTests.Services
{

    public class BookSearchTest
    {
        [Fact]  
        public async Task TestISBNSearch()
        {
            string isbn = "0071807993";
            string parameter = "isbn";
            var result = await BookService.SearchBook(parameter, isbn);
            Assert.NotNull(result);
            Console.WriteLine("\nBook Name: " + result.VolumeInfo.Title);
            Console.WriteLine("Authors: " + string.Join(", ", result.VolumeInfo.Authors));
            Console.WriteLine("Publisher: " + result.VolumeInfo.Publisher);
        }
        [Fact]
        public async Task TestTitleSearch()
        {
            string title = "The Great Gatsby";
            string parameter = "intitle";
            var result = await BookService.SearchBook(parameter, title);
            Assert.NotNull(result);
            Console.WriteLine("\nBook Name: " + result.VolumeInfo.Title);
            Console.WriteLine("Authors: " + string.Join(", ", result.VolumeInfo.Authors));
            Console.WriteLine("Publisher: " + result.VolumeInfo.Publisher);
        }
    }
    
}
