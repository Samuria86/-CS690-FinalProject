using Library.Models;
using Spectre.Console;

namespace Library.Services
{
    public static class GoalsService
    {
        /// <summary>
        /// Display active reading goals if they exist
        /// </summary>
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

        /// <summary>
        /// Set and display daily page reading goal
        /// </summary>
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

        /// <summary>
        /// Set and display weekly page reading goal
        /// </summary>
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

        /// <summary>
        /// Set and display monthly book reading goal
        /// </summary>
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

        /// <summary>
        /// Set and display yearly book reading goal
        /// </summary>
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

        /// <summary>
        /// Display a visual weekly summary of goal achievement
        /// </summary>
        public static void DisplayWeeklySummary(List<Book> books, int? weeklyPageGoal = null, int? monthlyBookGoal = null)
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);

            var table = new Table();
            table.Title = new TableTitle($"Weekly Summary ({startOfWeek:MMM dd} - {startOfWeek.AddDays(6):MMM dd})");
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
                
                string progressBar = weeklyPageGoal.HasValue 
                    ? $"{pagesRead}/{weeklyPageGoal.Value} pages"
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

        /// <summary>
        /// Get total pages read on a specific date
        /// </summary>
        private static int GetPagesReadOnDate(List<Book> books, DateTime date)
        {
            return books.Sum(b =>
                b.StatusChanges.Where(sc => sc.ChangeDate.Date == date.Date)
                               .Sum(sc => sc.PagesReadAtChange)
            );
        }

        /// <summary>
        /// Get number of books finished on a specific date
        /// </summary>
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

        /// <summary>
        /// Count books completed in a specific year/month
        /// </summary>
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

        /// <summary>
        /// Display a formatted progress bar with goal achievement
        /// </summary>
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
    }
}