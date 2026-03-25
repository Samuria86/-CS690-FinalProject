using DotNetEnv;
using Google.Apis.Books.v1;
using Google.Apis.Books.v1.Data;
using Google.Apis.Services;
using System.Linq;

Env.Load(); // Loads the .env file from the current directory

namespace Books.Services
{
    public static class BookService
    {
        public string apiKey = Environment.GetEnvironmentVariable("GOOGLE_BOOKS_API_KEY");
       
        public static BooksService service = new BooksService(
            new BaseClientService.Initializer
        {
                ApplicationName = "ISBNBookSearch",
                ApiKey = apiKey,
        });

        public static async Task<Volume> SearchBook(string parameter, string input)
        {
            Console.WriteLine("Executing a book search request...");
            var result = await service.Volumes.List(parameter + ":" + input).ExecuteAsync();
            if (result != null && result.Items != null)
            {
                var item = result.Items.FirstOrDefault();
                return item;
            }
            return null;
        }
    }
}



