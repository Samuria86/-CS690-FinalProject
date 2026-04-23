using Xunit;
using Library.Services;
using Library.Models;
using System.IO;


namespace Library.Tests
{
    public class LibraryManagerTests
    {
        private static string GetTestFilePath() => Path.Combine(Path.GetTempPath(), $"library_manager_test_{Guid.NewGuid()}.dat");


        [Fact]
        public void TestEmptyLibraryMessage()
        {
            var testFile = GetTestFilePath();
            var originalOut = Console.Out;
            try
            {
                using var output = new StringWriter();
                Console.SetOut(output);

                LibraryManager.ViewBooks(testFile, "library");

                var result = output.ToString();
                Assert.Contains("Your library is empty.", result);
            }
            finally
            {
                Console.SetOut(originalOut);
                if (File.Exists(testFile))
                {
                    File.Delete(testFile);
                }
            }
        }

        [Fact]
        public void TestViewBookDetails()
        {
            var book = new Book
            {
                Title = "Test Title",
                Author = "Test Author",
                PublicationDate = 2026,
                PageCount = 100,
                Status = "Reading",
                DateAdded = new DateTime(2024, 1, 1),
                DateFinished = null
            };

            var originalOut = Console.Out;
            try
            {
                using var output = new StringWriter();
                Console.SetOut(output);

                LibraryManager.ViewBookDetails("Test Title by Test Author", new List<Book> { book });

                var result = output.ToString();
                Console.WriteLine("Book Details Output:");
                Console.WriteLine(result);
                Assert.Contains("Title: Test Title", result);
                Assert.Contains("Author: Test Author", result);
                Assert.Contains("Publication Date: 2026", result);
                Assert.Contains("Page Count: 100", result);
                Assert.Contains("Status: Reading", result);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

    }
}
