using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Virtual_Bookshelf.Library.Models;

namespace Virtual_Bookshelf.Library
{
    public static class LibraryStorage
    {

        public static List<Book> LoadBookList(string FileName)
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

        public static void SaveBookList(List<Book> bookList, string FileName)
        {
            string jsonData = JsonSerializer.Serialize(bookList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileName, jsonData);
        }

        public static void SaveBook(Book book, string FileName)
        {
            var books = LoadBookList(FileName);
            books.Add(book);
            SaveBookList(books, FileName);
        }

        public static void UpdateBooks(List<Book> bookList, string FileName)
        {
            SaveBookList(bookList, FileName);
        }
    }
}
