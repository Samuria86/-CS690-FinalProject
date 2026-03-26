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
        public static BookService? BookService { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Virtual Bookshelf...");
            do
            {
                Console.WriteLine("Displaying main menu...");
                string mainMenu = Prompt.Select("Main Menu", new[]
                {
                    "Main Library",
                    // TODO: "Wishlist",
                    // TODO: Goals & statistics
                    "Exit"
                });
                Console.WriteLine($"Selected: {mainMenu}");
                switch (mainMenu)
                {
                    case "Main Library":
                        MainLibraryMenu();
                        break;
                    case "Wishlist":
                        LibraryManager.WishlistMenu();
                        break;
                    case "Goals & statistics":
                        LibraryManager.GoalsStatisticsMenu();
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
                    "View library",
                    "Add book",
                    "Edit/Remove book",
                    // TODO: "Search/filter books",
                    // TODO: "Export library",
                    "Return"
                });
                switch (libraryMenu)
                {
                    case "View library":
                        LibraryManager.ViewLibrary();
                        break;
                    case "Add book":
                        LibraryManager.AddBook();
                        break;
                    case "Edit/Remove book":
                        LibraryManager.EditRemoveBook();
                        break;
                    case "Search/filter books":
                        LibraryManager.SearchFilterBooks();
                        break;
                    case "Export library":
                        LibraryManager.ExportLibrary();
                        break;
                    case "Return":
                        return;
                }
            } while (true);
        }
    }
}
