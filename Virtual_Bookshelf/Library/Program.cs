using DotNetEnv;
using Google.Apis.Books.v1;
using Google.Apis.Books.v1.Data;
using Google.Apis.Services;
using Sharprompt;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Globalization;
using Spectre.Console;
using Virtual_Bookshelf.Library.Services;


namespace Virtual_Bookshelf.Library
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Virtual Bookshelf...");
            do
            {
                Console.WriteLine("Displaying main menu...");
                string mainMenu = Prompt.Select("Main Menu", new[]
                {
                    "Main Library", "Wishlist", "Exit"
                });
                Console.WriteLine($"Selected: {mainMenu}");
                switch (mainMenu)
                {
                    case "Main Library":
                        MainLibraryMenu();
                        break;
                    case "Wishlist":
                        WishlistMenu();
                        break;
                    case "Exit":
                        Console.WriteLine("Exiting the application...");
                        return;
                }
            } while (true);
        }

        static void MainLibraryMenu()
        {
            do
            {
                string libraryMenu = Prompt.Select("Main Library", new[]
                {
                    "View library", "Add book", "Search/filter books", "Export library", "Return"
                });
                switch (libraryMenu)
                {
                    case "View library":
                        ViewLibrary();
                        break;
                    case "Add book":
                        AddBook();
                        break;
                    case "Search/filter books":
                        SearchFilterBooks();
                        break;
                    case "Export library":
                        ExportLibrary();
                        break;
                    case "Return":
                        return;
                }
            } while (true);
        }
    }
}
