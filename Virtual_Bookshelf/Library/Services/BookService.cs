using Google.Apis.Books.v1;
using Google.Apis.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Virtual_Bookshelf.Library.Services
{
    public class BookService
    {
        public readonly BooksService _booksService;
        public BookService(string apiKey)
        {
            _booksService = new BooksService(
                new BaseClientService.Initializer()
                {
                    ApiKey = apiKey,
                    ApplicationName = this.GetType().ToString(),
                });
        }

        public async Task<Google.Apis.Books.v1.Data.Volumes> SearchBooks(string parameter, string input)
        {
            Console.WriteLine("Executing a book search request...");
            var result = await _booksService.Volumes.List(parameter + ":" + input).ExecuteAsync();
            return result;
        }

        public async Task<Google.Apis.Books.v1.Data.Volume> SearchBook(string parameter, string input)
        {
            var result = await SearchBooks(parameter, input);
            if (result != null && result.Items != null && result.Items.Count > 0)
            {
                return result.Items[0];
            }
            throw new InvalidOperationException("Book not found.");
        }
    }
}