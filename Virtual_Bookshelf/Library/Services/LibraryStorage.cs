using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.Json;
using Library.Models;

namespace Library.Services
{
    public static class LibraryStorage
    {
        private const string StorageFileMagic = "VBKT";
        private const int StorageFileVersion = 1;

        public static List<Book> LoadBookList(string FileName)
        {
            if (!File.Exists(FileName))
            {
                return new List<Book>();
            }

            var data = ReadFromMemoryMappedFile(FileName);
            if (data.Length == 0)
            {
                return new List<Book>();
            }

            return DeserializeBookList(data);
        }

        public static void SaveBookList(List<Book> bookList, string FileName)
        {
            var bytes = SerializeBookList(bookList);
            WriteToMemoryMappedFile(FileName, bytes);
        }

        public static void SaveBook(Book book, string FileName)
        {
            var books = LoadBookList(FileName);

            if (books.Any(b => b.Title == book.Title && b.Author == book.Author && b.PublicationDate == book.PublicationDate && b.PageCount == book.PageCount))
            {
                throw new InvalidOperationException("Book already exists in the library.");
            }
            books.Add(book);
            SaveBookList(books, FileName);
        }

        public static void ExportToJson(List<Book> bookList, string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(bookList, options);
            File.WriteAllText(filePath, json);
        }

        public static List<Book> ImportFromJson(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("JSON file not found.", filePath);
            }

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Book>>(json) ?? new List<Book>();
        }

        private static byte[] ReadFromMemoryMappedFile(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            if (!fileInfo.Exists || fileInfo.Length == 0)
            {
                return [];
            }

            using var mmf = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open, null, fileInfo.Length, MemoryMappedFileAccess.Read);
            using var accessor = mmf.CreateViewAccessor(0, fileInfo.Length, MemoryMappedFileAccess.Read);
            var buffer = new byte[fileInfo.Length];
            accessor.ReadArray(0, buffer, 0, buffer.Length);
            return buffer;
        }

        private static void WriteToMemoryMappedFile(string fileName, byte[] bytes)
        {
            using var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            fs.SetLength(bytes.Length);

            if (bytes.Length == 0)
            {
                return;
            }

            using var mmf = MemoryMappedFile.CreateFromFile(fs, null, bytes.Length, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
            using var accessor = mmf.CreateViewAccessor(0, bytes.Length, MemoryMappedFileAccess.Write);
            accessor.WriteArray(0, bytes, 0, bytes.Length);
            accessor.Flush();
        }

        private static byte[] SerializeBookList(List<Book> bookList)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, leaveOpen: true);

            writer.Write(Encoding.ASCII.GetBytes(StorageFileMagic));
            writer.Write(StorageFileVersion);
            writer.Write(bookList.Count);

            foreach (var book in bookList)
            {
                writer.Write(book.Title ?? string.Empty);
                writer.Write(book.Author ?? string.Empty);
                writer.Write(book.Genre ?? "Unknown");
                writer.Write(book.PublicationDate);
                writer.Write(book.PageCount);
                writer.Write(book.PagesRead);
                writer.Write(book.Status ?? string.Empty);
                writer.Write(book.DateAdded.Ticks);
                writer.Write(book.DateFinished.HasValue);
                if (book.DateFinished.HasValue)
                {
                    writer.Write(book.DateFinished.Value.Ticks);
                }
                writer.Write(book.DateModified.Ticks);

                // Serialize Bookmarks
                writer.Write(book.Bookmarks?.Count ?? 0);
                if (book.Bookmarks != null)
                {
                    foreach (var bookmark in book.Bookmarks)
                    {
                        writer.Write(bookmark.PageNumber);
                        writer.Write(bookmark.Color ?? "red");
                        writer.Write(bookmark.Notes ?? string.Empty);
                        writer.Write(bookmark.DateCreated.Ticks);
                        writer.Write(bookmark.DateModified.Ticks);
                    }
                }

                // Serialize StatusChanges
                writer.Write(book.StatusChanges?.Count ?? 0);
                if (book.StatusChanges != null)
                {
                    foreach (var statusChange in book.StatusChanges)
                    {
                        writer.Write(statusChange.PreviousStatus ?? string.Empty);
                        writer.Write(statusChange.NewStatus ?? string.Empty);
                        writer.Write(statusChange.ChangeDate.Ticks);
                        writer.Write(statusChange.PagesReadAtChange);
                    }
                }
            }

            writer.Flush();
            return memoryStream.ToArray();
        }

        private static List<Book> DeserializeBookList(byte[] bytes)
        {
            using var memoryStream = new MemoryStream(bytes);
            using var reader = new BinaryReader(memoryStream, Encoding.UTF8, leaveOpen: true);

            var magicBytes = reader.ReadBytes(4);
            var magic = Encoding.ASCII.GetString(magicBytes);
            if (magic != StorageFileMagic)
            {
                return new List<Book>();
            }

            var version = reader.ReadInt32();
            if (version != StorageFileVersion)
            {
                return new List<Book>();
            }

            var count = reader.ReadInt32();
            var books = new List<Book>(count);
            for (int i = 0; i < count; i++)
            {
                var title = reader.ReadString();
                var author = reader.ReadString();
                var genre = reader.ReadString();
                var publicationDate = reader.ReadInt32();
                var pageCount = reader.ReadInt32();
                var pagesRead = reader.ReadInt32();
                var status = reader.ReadString();
                var dateAdded = new DateTime(reader.ReadInt64());
                var hasDateFinished = reader.ReadBoolean();
                DateTime? dateFinished = hasDateFinished ? new DateTime(reader.ReadInt64()) : null;
                var dateModified = new DateTime(reader.ReadInt64());

                var book = new Book
                {
                    Title = title,
                    Author = author,
                    Genre = genre,
                    PublicationDate = publicationDate,
                    PageCount = pageCount,
                    PagesRead = pagesRead,
                    Status = status,
                    DateAdded = dateAdded,
                    DateFinished = dateFinished,
                    DateModified = dateModified
                };

                // Deserialize Bookmarks
                var bookmarkCount = reader.ReadInt32();
                for (int j = 0; j < bookmarkCount; j++)
                {
                    var pageNumber = reader.ReadInt32();
                    var color = reader.ReadString();
                    var notes = reader.ReadString();
                    var dateCreated = new DateTime(reader.ReadInt64());
                    var dateModifiedBookmark = new DateTime(reader.ReadInt64());

                    book.Bookmarks.Add(new Bookmark
                    {
                        PageNumber = pageNumber,
                        Color = color,
                        Notes = notes,
                        DateCreated = dateCreated,
                        DateModified = dateModifiedBookmark
                    });
                }

                // Deserialize StatusChanges
                var statusChangeCount = reader.ReadInt32();
                for (int j = 0; j < statusChangeCount; j++)
                {
                    var previousStatus = reader.ReadString();
                    var newStatus = reader.ReadString();
                    var changeDate = new DateTime(reader.ReadInt64());
                    var pagesReadAtChange = reader.ReadInt32();

                    book.StatusChanges.Add(new StatusChangeRecord
                    {
                        PreviousStatus = previousStatus,
                        NewStatus = newStatus,
                        ChangeDate = changeDate,
                        PagesReadAtChange = pagesReadAtChange
                    });
                }

                books.Add(book);
            }

            return books;
        }
    }
}
