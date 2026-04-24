using Library.Models;
using Library.Services;
using Sharprompt;
using Spectre.Console;

namespace Library.Services
{
    public static class GoalsService
    {

        /// Display active reading goals if they exist

        public static void DisplayActiveGoals(List<Book> books)
        {
            var activeGoal = GoalsStorage.GetActiveGoal();
            if (activeGoal == null)
            {
                return;
            }

            Console.WriteLine("\n========== Active Reading Goals ==========");

            if (activeGoal.DailyPageGoal.HasValue)
            {
                SetDailyPageGoal(books, activeGoal.DailyPageGoal.Value);
            }

            if (activeGoal.WeeklyPageGoal.HasValue)
            {
                SetWeeklyPageGoal(books, activeGoal.WeeklyPageGoal.Value);
            }

            if (activeGoal.MonthlyBookGoal.HasValue)
            {
                SetMonthlyReadingGoal(books, activeGoal.MonthlyBookGoal.Value);
            }

            if (activeGoal.YearlyBookGoal.HasValue)
            {
                SetYearlyReadingGoal(books, activeGoal.YearlyBookGoal.Value);
            }

            Console.WriteLine("==========================================\n");
        }


        /// Set and display daily page reading goal

        public static void SetDailyPageGoal(List<Book> books, int pageGoal)
        {
            if (pageGoal < 0)
            {
                throw new ArgumentException("Page goal must be a non-negative integer.");
            }

            var today = DateTime.Today;
            int pagesReadToday = GetPagesReadOnDate(books, today);
            int progressPercent = (int)Math.Min(100, ((double)pagesReadToday / pageGoal) * 100);

            DisplayProgressBar($"Daily Page Goal ({today:ddd, MMM dd})", pagesReadToday, pageGoal, progressPercent, "pages");
        }


        /// Set and display weekly page reading goal

        public static void SetWeeklyPageGoal(List<Book> books, int pageGoal)
        {
            if (pageGoal < 0)
            {
                throw new ArgumentException("Page goal must be a non-negative integer.");
            }

            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);

            int pagesReadThisWeek = 0;
            for (var date = startOfWeek; date <= endOfWeek; date = date.AddDays(1))
            {
                pagesReadThisWeek += GetPagesReadOnDate(books, date);
            }

            int progressPercent = (int)Math.Min(100, ((double)pagesReadThisWeek / pageGoal) * 100);
            DisplayProgressBar($"Weekly Page Goal ({startOfWeek:MMM dd} - {endOfWeek:MMM dd})", pagesReadThisWeek, pageGoal, progressPercent, "pages");
        }


        /// Set and display monthly book reading goal

        public static void SetMonthlyReadingGoal(List<Book> books, int goal)
        {
            if (goal < 0)
            {
                throw new ArgumentException("Goal must be a non-negative integer.");
            }

            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;

            int booksCompletedThisMonth = CountBooksCompletedInPeriod(books, currentYear, currentMonth);
            int progressPercent = (int)Math.Min(100, ((double)booksCompletedThisMonth / goal) * 100);

            DisplayProgressBar($"Monthly Book Goal ({DateTime.Now:MMMM yyyy})", booksCompletedThisMonth, goal, progressPercent, "books");
        }


        /// Set and display yearly book reading goal

        public static void SetYearlyReadingGoal(List<Book> books, int goal)
        {
            if (goal < 0)
            {
                throw new ArgumentException("Goal must be a non-negative integer.");
            }

            var currentYear = DateTime.Now.Year;
            int booksCompletedThisYear = 0;

            for (int month = 1; month <= 12; month++)
            {
                booksCompletedThisYear += CountBooksCompletedInPeriod(books, currentYear, month);
            }

            int progressPercent = (int)Math.Min(100, ((double)booksCompletedThisYear / goal) * 100);
            DisplayProgressBar($"Yearly Book Goal ({currentYear})", booksCompletedThisYear, goal, progressPercent, "books");
        }


        /// Display a visual weekly summary of goal achievement

        public static void DisplayWeeklySummary(List<Book> books, int? dailyPageGoal = null, int? monthlyBookGoal = null)
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);

            var table = new Table
            {
                Title = new TableTitle($"Weekly Summary ({startOfWeek:MMM dd} - {startOfWeek.AddDays(6):MMM dd})")
            };
            table.AddColumn("Day");
            table.AddColumn("Pages Read");
            table.AddColumn("Books Finished");
            table.AddColumn("Progress");

            var dayNames = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

            for (int i = 0; i < 7; i++)
            {
                var date = startOfWeek.AddDays(i);
                int pagesRead = GetPagesReadOnDate(books, date);
                int booksFinished = GetBooksFinishedOnDate(books, date);

                string progressBar = dailyPageGoal.HasValue
                    ? $"{pagesRead}/{dailyPageGoal.Value} pages"
                    : "N/A";

                table.AddRow(
                    dayNames[i],
                    pagesRead.ToString(),
                    booksFinished.ToString(),
                    progressBar
                );
            }

            AnsiConsole.Write(table);

            // Summary statistics
            int totalPagesThisWeek = 0;
            int totalBooksThisWeek = 0;

            for (int i = 0; i < 7; i++)
            {
                var date = startOfWeek.AddDays(i);
                totalPagesThisWeek += GetPagesReadOnDate(books, date);
                totalBooksThisWeek += GetBooksFinishedOnDate(books, date);
            }

            Console.WriteLine($"\nWeekly Totals: {totalPagesThisWeek} pages read, {totalBooksThisWeek} books finished");

            if (monthlyBookGoal.HasValue)
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                int monthlyBooksCompleted = CountBooksCompletedInPeriod(books, currentYear, currentMonth);
                int monthProgressPercent = (int)Math.Min(100, ((double)monthlyBooksCompleted / monthlyBookGoal.Value) * 100);
                Console.WriteLine($"Monthly Progress: {monthlyBooksCompleted}/{monthlyBookGoal.Value} books ({monthProgressPercent}%)");
            }
        }


        /// Get total pages read on a specific date

        private static int GetPagesReadOnDate(List<Book> books, DateTime date)
        {
            return books.Sum(b =>
                b.StatusChanges.Where(sc => sc.ChangeDate.Date == date.Date)
                               .Sum(sc => sc.PagesReadAtChange)
            );
        }


        /// Get number of books finished on a specific date

        private static int GetBooksFinishedOnDate(List<Book> books, DateTime date)
        {
            return books.Count(b =>
                b.StatusChanges.Any(sc =>
                    (sc.NewStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                     sc.NewStatus.Equals("Finished", StringComparison.OrdinalIgnoreCase)) &&
                    sc.ChangeDate.Date == date.Date
                )
            );
        }


        /// Count books completed in a specific year/month

        private static int CountBooksCompletedInPeriod(List<Book> books, int year, int month)
        {
            return books.Count(b =>
                b.StatusChanges.Any(sc =>
                    (sc.NewStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                     sc.NewStatus.Equals("Finished", StringComparison.OrdinalIgnoreCase)) &&
                    sc.ChangeDate.Year == year &&
                    sc.ChangeDate.Month == month
                )
            );
        }


        /// Display a formatted progress bar with goal achievement

        private static void DisplayProgressBar(string title, int current, int goal, int progressPercent, string unit)
        {
            if (goal == 0)
            {
                Console.WriteLine($"\n{title}");
                Console.WriteLine($"Progress: {current}/{goal} {unit} (0%)");
                return;
            }

            Console.WriteLine($"\n{title}");
            Console.WriteLine($"Progress: {current}/{goal} {unit} ({progressPercent}%)");

            AnsiConsole.Progress()
                .Start(ctx =>
                {
                    var task = ctx.AddTask($"{title}", new ProgressTaskSettings { MaxValue = goal });
                    task.Value = current;
                });

            if (progressPercent >= 100)
            {
                Console.WriteLine("✓ Goal reached!");
            }
            else
            {
                int remaining = goal - current;
                Console.WriteLine($"{remaining} {unit} remaining");
            }
        }
        public static void HandleDailyPageGoal(List<Book> books)
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

        public static void HandleWeeklyPageGoal(List<Book> books)
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

        public static void HandleMonthlyBookGoal(List<Book> books)
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

        public static void HandleYearlyBookGoal(List<Book> books)
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

        public static void HandleWeeklySummary(List<Book> books)
        {
            var activeGoal = GoalsStorage.GetActiveGoal();
            int? dailyPageGoal = activeGoal?.DailyPageGoal;
            int? monthlyBookGoal = activeGoal?.MonthlyBookGoal;

            DisplayWeeklySummary(books, dailyPageGoal, monthlyBookGoal);
        }

        public static void HandleEditActiveGoals(List<Book> books)
        {
            var activeGoal = GoalsStorage.GetActiveGoal();
            if (activeGoal == null)
            {
                Console.WriteLine("No active reading goals found. Set a goal first.");
                return;
            }

            Console.WriteLine("Editing active reading goals. Leave blank to keep current values.");

            var dailyGoal = Prompt.Input<string>($"Daily page goal ({activeGoal.DailyPageGoal?.ToString() ?? "none"})");
            if (!string.IsNullOrWhiteSpace(dailyGoal) && int.TryParse(dailyGoal, out var dailyValue) && dailyValue > 0)
            {
                activeGoal.DailyPageGoal = dailyValue;
            }

            var weeklyGoal = Prompt.Input<string>($"Weekly page goal ({activeGoal.WeeklyPageGoal?.ToString() ?? "none"})");
            if (!string.IsNullOrWhiteSpace(weeklyGoal) && int.TryParse(weeklyGoal, out var weeklyValue) && weeklyValue > 0)
            {
                activeGoal.WeeklyPageGoal = weeklyValue;
            }

            var monthlyGoal = Prompt.Input<string>($"Monthly book goal ({activeGoal.MonthlyBookGoal?.ToString() ?? "none"})");
            if (!string.IsNullOrWhiteSpace(monthlyGoal) && int.TryParse(monthlyGoal, out var monthlyValue) && monthlyValue > 0)
            {
                activeGoal.MonthlyBookGoal = monthlyValue;
            }

            var yearlyGoal = Prompt.Input<string>($"Yearly book goal ({activeGoal.YearlyBookGoal?.ToString() ?? "none"})");
            if (!string.IsNullOrWhiteSpace(yearlyGoal) && int.TryParse(yearlyGoal, out var yearlyValue) && yearlyValue > 0)
            {
                activeGoal.YearlyBookGoal = yearlyValue;
            }

            activeGoal.LastModifiedDate = DateTime.Now;
            GoalsStorage.UpdateGoal(activeGoal);
            GoalsService.DisplayActiveGoals(books);
            Console.WriteLine("Active reading goals updated successfully.");
        }

        public static void HandleClearAllGoals()
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

    }
}


