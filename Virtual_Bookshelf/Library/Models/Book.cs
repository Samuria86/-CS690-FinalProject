namespace Virtual_Bookshelf.Library.Models
{
    public class Book
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; } = DateTime.MinValue;
        public int PageCount { get; set; } = 0;
        public string Status { get; set; } = "Not started";
    }
}