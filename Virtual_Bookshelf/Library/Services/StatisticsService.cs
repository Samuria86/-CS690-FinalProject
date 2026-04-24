using Library.Models;

namespace Library.Services
{
    public static class StatisticsService
    {
        /// Get total number of books marked as completed.
        public static int GetTotalBooksRead(List<Book> books)
        {
            return books.Count(b =>
                b.StatusChanges.Any(sc =>
                    sc.NewStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                    sc.NewStatus.Equals("Finished", StringComparison.OrdinalIgnoreCase)
                )
            );
        }

        /// Get count of books currently being read (in progress).
        public static int GetCurrentlyReadingCount(List<Book> books)
        {
            return books.Count(b =>
                b.Status.Equals("In progress", StringComparison.OrdinalIgnoreCase) ||
                b.Status.Equals("Reading", StringComparison.OrdinalIgnoreCase) ||
                b.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase)
            );
        }

        /// Get total pages read across all books.
        public static int GetTotalPagesRead(List<Book> books)
        {
            return books.Sum(b => b.PagesRead);
        }

        /// Get books read per month (current month).
        public static int GetBooksThisMonth(List<Book> books)
        {
            var now = DateTime.Now;
            var completionsThisMonth = books
                .SelectMany(b => b.StatusChanges)
                .Where(sc =>
                    (sc.NewStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                     sc.NewStatus.Equals("Finished", StringComparison.OrdinalIgnoreCase)) &&
                    sc.ChangeDate.Year == now.Year &&
                    sc.ChangeDate.Month == now.Month
                )
                .ToList();

            int booksThisMonth = 0;
            foreach (var book in books)
            {
                var completion = book.StatusChanges.FirstOrDefault(sc =>
                    (sc.NewStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                     sc.NewStatus.Equals("Finished", StringComparison.OrdinalIgnoreCase)) &&
                    sc.ChangeDate.Year == now.Year &&
                    sc.ChangeDate.Month == now.Month
                );

                if (completion != null)
                {
                    booksThisMonth++;
                }
            }

            return booksThisMonth;
        }

        /// Get pages read for the last 6 months (dictionary with month/year as key).
        public static Dictionary<string, int> GetPagesPerLastSixMonths(List<Book> books)
        {
            var result = new Dictionary<string, int>();
            var now = DateTime.Now;

            // Initialize dictionary with last 6 months
            for (int i = 5; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                string key = $"{month.Year}-{month.Month:D2}";
                result[key] = 0;
            }

            // Count pages for each month
            foreach (var book in books)
            {
                foreach (var completion in book.StatusChanges.Where(sc =>
                    sc.NewStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                    sc.NewStatus.Equals("Finished", StringComparison.OrdinalIgnoreCase)
                ))
                {
                    var month = completion.ChangeDate;
                    string key = $"{month.Year}-{month.Month:D2}";

                    // Only count if within last 6 months
                    if (result.ContainsKey(key))
                    {
                        result[key] += book.PageCount;
                    }
                }
            }

            return result;
        }
        /// <summary>
        /// Get books completed per month (dictionary with month/year as key).
        /// </summary>
        public static Dictionary<string, int> GetBooksPerMonth(List<Book> books)
        {
            var result = new Dictionary<string, int>();

            var completions = books
                .SelectMany(b => b.StatusChanges.Where(sc =>
                    sc.NewStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                    sc.NewStatus.Equals("Finished", StringComparison.OrdinalIgnoreCase)
                ))
                .GroupBy(sc => new { sc.ChangeDate.Year, sc.ChangeDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .ToList();

            foreach (var group in completions)
            {
                string key = $"{group.Key.Year}-{group.Key.Month:D2}";
                result[key] = group.Count();
            }

            return result;
        }

        /// <summary>
        /// Get books completed per year (dictionary with year as key).
        /// </summary>
        public static Dictionary<int, int> GetBooksPerYear(List<Book> books)
        {
            var result = new Dictionary<int, int>();

            var completions = books
                .SelectMany(b => b.StatusChanges.Where(sc =>
                    sc.NewStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                ))
                .GroupBy(sc => sc.ChangeDate.Year)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var group in completions)
            {
                result[group.Key] = group.Count();
            }

            return result;
        }

        /// <summary>
        /// Get current reading streak (consecutive days with reading activity).
        /// A reading activity is defined as any status change that indicates reading.
        /// </summary>
        public static int GetReadingStreak(List<Book> books)
        {
            if (books.Count == 0) return 0;

            var allStatusChanges = books
                .SelectMany(b => b.StatusChanges)
                .OrderByDescending(sc => sc.ChangeDate)
                .ToList();

            if (allStatusChanges.Count == 0) return 0;

            var datesWithActivity = allStatusChanges
                .Select(sc => sc.ChangeDate.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            int streak = 0;
            DateTime? expectedDate = null;

            foreach (var date in datesWithActivity)
            {
                if (expectedDate == null)
                {
                    expectedDate = date;
                    streak = 1;
                }
                else if (date == expectedDate.Value.AddDays(-1))
                {
                    streak++;
                    expectedDate = date;
                }
                else if (date < expectedDate.Value.AddDays(-1))
                {
                    break;
                }
            }

            return streak;
        }

        /// <summary>
        /// Get statistics summary as formatted string.
        /// </summary>
        public static string GetStatisticsSummary(List<Book> books)
        {
            var summary = new System.Text.StringBuilder();

            summary.AppendLine("\n========== READING STATISTICS ==========");
            summary.AppendLine($"Total Books Read: {GetTotalBooksRead(books)}");
            summary.AppendLine($"Currently Reading: {GetCurrentlyReadingCount(books)}");
            summary.AppendLine($"Total Pages Read: {GetTotalPagesRead(books)}");
            summary.AppendLine($"Books Completed This Month: {GetBooksThisMonth(books)}");
            summary.AppendLine($"Reading Streak (days): {GetReadingStreak(books)}");

            summary.AppendLine("========================================\n");
            return summary.ToString();
        }
    }
}
