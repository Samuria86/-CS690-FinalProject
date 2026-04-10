using Google.Apis.Books.v1;
using Google.Apis.Services;

namespace Virtual_Bookshelf.Library.Services
{
    public class BookService
    {
        public readonly BooksService _booksService;
        public BookService(string ApiKey)
        {
            _booksService = new BooksService(
                new BaseClientService.Initializer()
                {
                    ApiKey = ApiKey,
                    ApplicationName = this.GetType().ToString(),
                });
        }

        public async Task<Google.Apis.Books.v1.Data.Volumes> SearchBooks(string Parameter, string Input)
        {
            Console.WriteLine("Executing a book search request...");
            var result = await _booksService.Volumes.List(Parameter + ":" + Input).ExecuteAsync();
            return result;
        }

        public async Task<Google.Apis.Books.v1.Data.Volume> SearchBook(string Parameter, string Input)
        {
            var result = await SearchBooks(Parameter, Input);
            if (result != null && result.Items != null && result.Items.Count > 0)
            {
                return result.Items[0];
            }
            throw new InvalidOperationException("Book not found.");
        }
    }
}