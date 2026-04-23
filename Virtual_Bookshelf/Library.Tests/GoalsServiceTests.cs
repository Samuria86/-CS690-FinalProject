using Xunit;
using Library.Services;
using Library.Models;


namespace Library.Tests
{
    public class GoalsServiceTests
    {
        private static List<Book> CreateTestBooks()
        {
            var today = DateTime.Today;
            return
            [
                new Book
                {
                    Title = "Book 1",
                    Author = "Author 1",
                    Status = "Completed",
                    PageCount = 300,
                    PagesRead = 300,
                    StatusChanges = new List<StatusChangeRecord>
                    {
                        new() {
                            PreviousStatus = "Not started",
                            NewStatus = "Completed",
                            ChangeDate = today,
                            PagesReadAtChange = 300
                        }
                    }
                },
                new Book
                {
                    Title = "Book 2",
                    Author = "Author 2",
                    Status = "Reading",
                    PageCount = 400,
                    PagesRead = 150,
                    StatusChanges = new List<StatusChangeRecord>
                    {
                        new() {
                            PreviousStatus = "Not started",
                            NewStatus = "Reading",
                            ChangeDate = today.AddDays(-1),
                            PagesReadAtChange = 50
                        },
                        new() {
                            PreviousStatus = "Reading",
                            NewStatus = "Reading",
                            ChangeDate = today,
                            PagesReadAtChange = 100
                        }
                    }
                }
            ];
        }

        [Fact]
        public void SetDailyPageGoal_WithNegativeGoal_ThrowsException()
        {
            var books = CreateTestBooks();
            Assert.Throws<ArgumentException>(() => GoalsService.SetDailyPageGoal(books, -5));
        }

        [Fact]
        public void SetWeeklyPageGoal_WithNegativeGoal_ThrowsException()
        {
            var books = CreateTestBooks();
            Assert.Throws<ArgumentException>(() => GoalsService.SetWeeklyPageGoal(books, -10));
        }

        [Fact]
        public void SetMonthlyReadingGoal_WithNegativeGoal_ThrowsException()
        {
            var books = CreateTestBooks();
            Assert.Throws<ArgumentException>(() => GoalsService.SetMonthlyReadingGoal(books, -1));
        }

        [Fact]
        public void SetYearlyReadingGoal_WithNegativeGoal_ThrowsException()
        {
            var books = CreateTestBooks();
            Assert.Throws<ArgumentException>(() => GoalsService.SetYearlyReadingGoal(books, -12));
        }
    }
}
