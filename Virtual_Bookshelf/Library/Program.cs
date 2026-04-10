using Sharprompt;
using Virtual_Bookshelf.Library.Services;


namespace Virtual_Bookshelf.Library
{
    partial class Program
    {
        public static BookService? BookService { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Virtual Bookshelf...");
            string LibraryFileName = "library.json";
            string WishlistFileName = "wishlist.json";
            do
            {
                Console.WriteLine("Displaying main menu...");
                string mainMenu = Prompt.Select("Main Menu", new[]
                {
                    "Main Library",
                    "Wishlist",
                    // TODO: Goals & statistics
                    "Exit"
                });
                Console.WriteLine($"Selected: {mainMenu}");
                switch (mainMenu)
                {
                    case "Main Library":
                        MainLibraryMenu(LibraryFileName);
                        break;
                    case "Wishlist":
                        WishlistMenu(WishlistFileName, LibraryFileName);
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

        static void MainLibraryMenu(string FileName)
        {
            do
            {
                string Mode = "library";
                string libraryMenu = Prompt.Select("Main Library", new[]
                {
                    "View library",
                    "Add book",
                    "Edit/Remove book",
                    "Search/filter books",
                    // TODO: "Export library",
                    "Return"
                });
                switch (libraryMenu)
                {
                    case "View library":
                        LibraryManager.ViewBooks(FileName, Mode);
                        break;
                    case "Add book":
                        LibraryManager.AddBook(FileName, Mode);
                        break;
                    case "Edit/Remove book":
                        LibraryManager.EditOrRemoveBook(FileName, Mode);
                        break;
                    case "Search/filter books":
                        LibraryManager.SearchFilterBooks(FileName, Mode);
                        break;
                    case "Export library":
                        LibraryManager.ExportLibrary();
                        break;
                    case "Return":
                        return;
                }
            } while (true);
        }

        static void WishlistMenu(string FileName, string LibraryFileName)
        {
            do
            {
                string Mode = "wishlist";
                string wishlistMenu = Prompt.Select("Wishlist", new[]
                {
                    "View wishlist",
                    "Add book to wishlist",
                    "Edit/Remove book from wishlist",
                    "Add book to main library",
                    "Search/filter wishlist",
                    // TODO: "Export wishlist",
                    "Return"
                });
                switch (wishlistMenu)
                {
                    case "View wishlist":
                        LibraryManager.ViewBooks(FileName, Mode);
                        break;
                    case "Add book to wishlist":
                        LibraryManager.AddBook(FileName, Mode);
                        break;
                    case "Edit/Remove book from wishlist":
                        LibraryManager.EditOrRemoveBook(FileName, Mode);
                        break;
                    case "Add book to main library":
                        LibraryManager.WishlistToLibrary(FileName, LibraryFileName);
                        break;
                    case "Search/filter wishlist":
                        LibraryManager.SearchFilterBooks(FileName, Mode);
                        break;
                    case "Return":
                        return;
                }
            } while (true);
        }
    }
}
