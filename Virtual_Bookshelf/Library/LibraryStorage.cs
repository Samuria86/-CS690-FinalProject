using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Virtual_Bookshelf.Library.Models;

namespace Virtual_Bookshelf.Library
{
    public static class LibraryStorage
    {
        private const string FileName = "library.json";

        public static string GetFilePath() => FileName;

        public static List<Book> LoadBookList()
        {
            if (!File.Exists(FileName))
            {
                return new List<Book>();
            }

            string data = File.ReadAllText(FileName);
            if (string.IsNullOrWhiteSpace(data))
            {
                return new List<Book>();
            }

            return JsonSerializer.Deserialize<List<Book>>(data) ?? new List<Book>();
        }

        public static void SaveBookList(List<Book> bookList)
        {
            string jsonData = JsonSerializer.Serialize(bookList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileName, jsonData);
        }

        public static void AddBook(Book book)
        {
            var books = LoadBookList();
            books.Add(book);
            SaveBookList(books);
        }

        public static void UpdateBooks(List<Book> bookList)
        {
            SaveBookList(bookList);
        }
    }
}
