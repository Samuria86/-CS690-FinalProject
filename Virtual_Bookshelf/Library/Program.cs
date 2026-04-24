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
                        HandleWeeklySummary(books);
                        break;
                    case "Set daily page goal":
                        HandleDailyPageGoal(books);
                        break;
                    case "Set weekly page goal":
                        HandleWeeklyPageGoal(books);
                        break;
                    case "Set monthly book goal":
                        HandleMonthlyBookGoal(books);
                        break;
                    case "Set yearly book goal":
                        HandleYearlyBookGoal(books);
                        break;
                    case "View active goals":
                        GoalsService.DisplayActiveGoals(books);
                        break;
                    case "Edit active goals":
                        HandleEditActiveGoals(books);
                        break;
                    case "Clear all goals":
                        HandleClearAllGoals();
                        break;
                    case "Return":
                        return;
                }
            } while (true);
        }

        static void HandleDailyPageGoal(List<Book> books)
        {
            int pageGoal = Prompt.Input<int>("Enter daily page goal (pages)");
            if (pageGoal <= 0)
            {
                Console.WriteLine("Goal must be greater than 0.");
                return;
            }
            GoalsService.SetDailyPageGoal(books, pageGoal);
            // Save goal to persistent storage
            var activeGoal = GoalsStorage.GetActiveGoal();
            if (activeGoal != null)
            {
                activeGoal.DailyPageGoal = pageGoal;
                GoalsStorage.UpdateGoal(activeGoal);
            }
            else
            {
                GoalsStorage.AddGoal(new ReadingGoal { DailyPageGoal = pageGoal });
            }
            Console.WriteLine("Daily page goal saved successfully!");
        }

        static void HandleWeeklyPageGoal(List<Book> books)
        {
            int pageGoal = Prompt.Input<int>("Enter weekly page goal (pages)");
            if (pageGoal <= 0)
            {
                Console.WriteLine("Goal must be greater than 0.");
                return;
            }
            GoalsService.SetWeeklyPageGoal(books, pageGoal);
            // Save goal to persistent storage
            var activeGoal = GoalsStorage.GetActiveGoal();
            if (activeGoal != null)
            {
                activeGoal.WeeklyPageGoal = pageGoal;
                GoalsStorage.UpdateGoal(activeGoal);
            }
            else
            {
                GoalsStorage.AddGoal(new ReadingGoal { WeeklyPageGoal = pageGoal });
            }
            Console.WriteLine("Weekly page goal saved successfully!");
        }

        static void HandleMonthlyBookGoal(List<Book> books)
        {
            int bookGoal = Prompt.Input<int>("Enter monthly book goal (books)");
            if (bookGoal <= 0)
            {
                Console.WriteLine("Goal must be greater than 0.");
                return;
            }
            GoalsService.SetMonthlyReadingGoal(books, bookGoal);
            // Save goal to persistent storage
            var activeGoal = GoalsStorage.GetActiveGoal();
            if (activeGoal != null)
            {
                activeGoal.MonthlyBookGoal = bookGoal;
                GoalsStorage.UpdateGoal(activeGoal);
            }
            else
            {
                GoalsStorage.AddGoal(new ReadingGoal { MonthlyBookGoal = bookGoal });
            }
            Console.WriteLine("Monthly book goal saved successfully!");
        }

        static void HandleYearlyBookGoal(List<Book> books)
        {
            int bookGoal = Prompt.Input<int>("Enter yearly book goal (books)");
            if (bookGoal <= 0)
            {
                Console.WriteLine("Goal must be greater than 0.");
                return;
            }
            GoalsService.SetYearlyReadingGoal(books, bookGoal);
            // Save goal to persistent storage
            var activeGoal = GoalsStorage.GetActiveGoal();
            if (activeGoal != null)
            {
                activeGoal.YearlyBookGoal = bookGoal;
                GoalsStorage.UpdateGoal(activeGoal);
            }
            else
            {
                GoalsStorage.AddGoal(new ReadingGoal { YearlyBookGoal = bookGoal });
            }
            Console.WriteLine("Yearly book goal saved successfully!");
        }

        static void HandleWeeklySummary(List<Book> books)
        {
            var activeGoal = GoalsStorage.GetActiveGoal();
            int? weeklyPageGoal = activeGoal?.WeeklyPageGoal;
            int? monthlyBookGoal = activeGoal?.MonthlyBookGoal;

            GoalsService.DisplayWeeklySummary(books, weeklyPageGoal, monthlyBookGoal);
        }

        static void HandleEditActiveGoals(List<Book> books)
        {
            var activeGoal = GoalsStorage.GetActiveGoal();
            if (activeGoal == null)
            {
                Console.WriteLine("No active reading goals found. Set a goal first.");
                return;
            }

            Console.WriteLine("Editing active reading goals. Leave blank to keep current values.");

            var dailyGoal = Prompt.Input<string>($"Daily page goal ({activeGoal.DailyPageGoal?.ToString() ?? "none"}):");
            if (!string.IsNullOrWhiteSpace(dailyGoal) && int.TryParse(dailyGoal, out var dailyValue) && dailyValue > 0)
            {
                activeGoal.DailyPageGoal = dailyValue;
            }

            var weeklyGoal = Prompt.Input<string>($"Weekly page goal ({activeGoal.WeeklyPageGoal?.ToString() ?? "none"}):");
            if (!string.IsNullOrWhiteSpace(weeklyGoal) && int.TryParse(weeklyGoal, out var weeklyValue) && weeklyValue > 0)
            {
                activeGoal.WeeklyPageGoal = weeklyValue;
            }

            var monthlyGoal = Prompt.Input<string>($"Monthly book goal ({activeGoal.MonthlyBookGoal?.ToString() ?? "none"}):");
            if (!string.IsNullOrWhiteSpace(monthlyGoal) && int.TryParse(monthlyGoal, out var monthlyValue) && monthlyValue > 0)
            {
                activeGoal.MonthlyBookGoal = monthlyValue;
            }

            var yearlyGoal = Prompt.Input<string>($"Yearly book goal ({activeGoal.YearlyBookGoal?.ToString() ?? "none"}):");
            if (!string.IsNullOrWhiteSpace(yearlyGoal) && int.TryParse(yearlyGoal, out var yearlyValue) && yearlyValue > 0)
            {
                activeGoal.YearlyBookGoal = yearlyValue;
            }

            activeGoal.LastModifiedDate = DateTime.Now;
            GoalsStorage.UpdateGoal(activeGoal);
            GoalsService.DisplayActiveGoals(books);
            Console.WriteLine("Active reading goals updated successfully.");
        }

        static void HandleClearAllGoals()
        {
            var confirm = Prompt.Confirm("Are you sure you want to clear all saved reading goals?", defaultValue: false);
            if (!confirm)
            {
                Console.WriteLine("Clear goals canceled.");
                return;
            }

            GoalsStorage.ClearAllGoals();
            Console.WriteLine("All reading goals have been cleared.");
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
                    "View statistics",
                    "Manage custom status labels",
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
                    case "View statistics":
                        LibraryManager.ViewStatistics(FileName);
                        break;
                    case "Manage custom status labels":
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
