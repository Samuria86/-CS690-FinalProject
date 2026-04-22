using System;
using System.Collections.Generic;
using System.Linq;
using Virtual_Bookshelf.Library.Models;

namespace Virtual_Bookshelf.Library.Services
{
    public static class BookmarkManager
    {
        private const int MaxBookmarksPerBook = 3;


        /// Add a bookmark to a book. Maximum 3 bookmarks per book.

        public static void AddBookmark(Book book, int pageNumber, string color)
        {
            AddBookmark(book, pageNumber, color, "");
        }


        /// Add a bookmark to a book with notes. Maximum 3 bookmarks per book.

        public static void AddBookmark(Book book, int pageNumber, string color, string notes)
        {
            ArgumentNullException.ThrowIfNull(book);

            if (pageNumber < 1 || pageNumber > book.PageCount)
            {
                throw new ArgumentException($"Page number must be between 1 and {book.PageCount}.");
            }

            if (!Bookmark.AvailableColors.Contains(color.ToLower()))
            {
                throw new ArgumentException($"Invalid color. Available colors: {string.Join(", ", Bookmark.AvailableColors)}");
            }

            if (book.Bookmarks.Count >= MaxBookmarksPerBook)
            {
                throw new InvalidOperationException($"Maximum {MaxBookmarksPerBook} bookmarks per book allowed.");
            }

            // Check if bookmark already exists on this page
            if (book.Bookmarks.Any(b => b.PageNumber == pageNumber))
            {
                throw new InvalidOperationException($"A bookmark already exists on page {pageNumber}.");
            }

            var bookmark = new Bookmark
            {
                PageNumber = pageNumber,
                Color = color.ToLower(),
                Notes = notes ?? ""
            };

            book.Bookmarks.Add(bookmark);
            book.DateModified = DateTime.Now;
        }

        /// Remove a bookmark from a book by page number.
        public static void RemoveBookmark(Book book, int pageNumber)
        {
            ArgumentNullException.ThrowIfNull(book);

            var bookmark = book.Bookmarks.FirstOrDefault(b => b.PageNumber == pageNumber);
            if (bookmark == null)
            {
                throw new InvalidOperationException($"No bookmark found on page {pageNumber}.");
            }

            book.Bookmarks.Remove(bookmark);
            book.DateModified = DateTime.Now;
        }

        /// Update bookmark color and/or notes.
        public static void UpdateBookmark(Book book, int pageNumber, string newColor, string newNotes)
        {
            ArgumentNullException.ThrowIfNull(book);

            var bookmark = book.Bookmarks.FirstOrDefault(b => b.PageNumber == pageNumber) ?? throw new InvalidOperationException($"No bookmark found on page {pageNumber}.");
            if (!string.IsNullOrEmpty(newColor) && !Bookmark.AvailableColors.Contains(newColor.ToLower()))
            {
                throw new ArgumentException($"Invalid color. Available colors: {string.Join(", ", Bookmark.AvailableColors)}");
            }

            if (!string.IsNullOrEmpty(newColor))
            {
                bookmark.Color = newColor.ToLower();
            }

            if (newNotes != null)
            {
                bookmark.Notes = newNotes;
            }

            bookmark.DateModified = DateTime.Now;
            book.DateModified = DateTime.Now;
        }

        /// Get all bookmarks for a book, sorted by page number.
        public static List<Bookmark> GetBookmarks(Book book)
        {
            ArgumentNullException.ThrowIfNull(book);

            return book.Bookmarks.OrderBy(b => b.PageNumber).ToList();
        }

        /// Get bookmarks count.
        public static int GetBookmarkCount(Book book)
        {
            ArgumentNullException.ThrowIfNull(book);

            return book.Bookmarks.Count;
        }

        /// Check if a bookmark exists on a specific page.
        public static bool BookmarkExists(Book book, int pageNumber)
        {
            ArgumentNullException.ThrowIfNull(book);

            return book.Bookmarks.Any(b => b.PageNumber == pageNumber);
        }

        /// Get a bookmark by page number.
        public static Bookmark? GetBookmark(Book book, int pageNumber)
        {
            ArgumentNullException.ThrowIfNull(book);

            return book.Bookmarks.FirstOrDefault(b => b.PageNumber == pageNumber);
        }

        /// Clear all bookmarks from a book.
        public static void ClearAllBookmarks(Book book)
        {
            ArgumentNullException.ThrowIfNull(book);

            book.Bookmarks.Clear();
            book.DateModified = DateTime.Now;
        }

        /// Get the remaining bookmark slots.
        public static int GetRemainingBookmarkSlots(Book book)
        {
            ArgumentNullException.ThrowIfNull(book);

            return Math.Max(0, MaxBookmarksPerBook - book.Bookmarks.Count);
        }

        /// Display bookmarks in a formatted way.
        public static string FormatBookmarksDisplay(Book book)
        {
            ArgumentNullException.ThrowIfNull(book);

            if (book.Bookmarks.Count == 0)
            {
                return "No bookmarks";
            }

            var sortedBookmarks = GetBookmarks(book);
            var bookmarkStrings = sortedBookmarks.Select(b =>
                $"{b.GetColorCode()} Page {b.PageNumber}" +
                (string.IsNullOrEmpty(b.Notes) ? "" : $" - {b.Notes}")
            );

            return string.Join(" | ", bookmarkStrings);
        }
    }
}
