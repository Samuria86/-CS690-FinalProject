using Library.Services;
using Library.Models;
using Sharprompt;


namespace Library
{
    partial class Program
    {
        public static BookService? BookService { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Virtual Bookshelf...");
            string LibraryFileName = "library.dat";
            string WishlistFileName = "wishlist.dat";

            // Display active goals on startup if they exist
            var books = LibraryStorage.LoadBookList(LibraryFileName);
            GoalsService.DisplayActiveGoals(books);

            do
            {
                Console.WriteLine("Displaying main menu...");
                string mainMenu = Prompt.Select("Main Menu", new[]
                {
                    "Main Library",
                    "Wishlist",
                    "Statistics & Reading Goals",
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
                    case "Statistics & Reading Goals":
                        StatisticsAndGoalsMenu(LibraryFileName);
                        break;
                    case "Exit":
                        Console.WriteLine("Exiting the application...");
                        return;
                }
            } while (true);
        }

        static void StatisticsAndGoalsMenu(string FileName)
        {
            do
            {
                var books = LibraryStorage.LoadBookList(FileName);

                string goalsMenu = Prompt.Select("Statistics & Reading Goals", new[]
                {
                    "View statistics",
                    "View weekly summary",
                    "Set daily page goal",
                    "Set weekly page goal",
                    "Set monthly book goal",
                    // "Set yearly book goal",
                    "View active goals",
                    "Edit active goals",
                    "Clear all goals",
                    "Return"
                });

                switch (goalsMenu)
                {
                    case "View statistics":
                        LibraryManager.ViewStatistics(FileName);
                        break;
                    case "View weekly summary":
                        GoalsService.HandleWeeklySummary(books);
                        break;
                    case "Set daily page goal":
                        GoalsService.HandleDailyPageGoal(books);
                        break;
                    case "Set weekly page goal":
                        GoalsService.HandleWeeklyPageGoal(books);
                        break;
                    case "Set monthly book goal":
                        GoalsService.HandleMonthlyBookGoal(books);
                        break;
                    case "Set yearly book goal":
                        GoalsService.HandleYearlyBookGoal(books);
                        break;
                    case "View active goals":
                        GoalsService.DisplayActiveGoals(books);
                        break;
                    case "Edit active goals":
                        GoalsService.HandleEditActiveGoals(books);
                        break;
                    case "Clear all goals":
                        GoalsService.HandleClearAllGoals();
                        break;
                    case "Return":
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
                    "Manage status labels",
                    "Import library",
                    "Export library",
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
                    case "Manage status labels":
                        LibraryManager.ManageStatusLabels(FileName);
                        break;
                    case "Export library":
                        LibraryManager.ExportLibrary(FileName, Mode);
                        break;
                    case "Import library":
                        LibraryManager.ImportLibrary(FileName, Mode);
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
                    "Import wishlist",
                    "Export wishlist",
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
                    case "Export wishlist":
                        LibraryManager.ExportLibrary(FileName, Mode);
                        break;
                    case "Return":
                        return;
                }
            } while (true);
        }
    }
}
