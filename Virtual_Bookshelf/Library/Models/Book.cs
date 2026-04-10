namespace Virtual_Bookshelf.Library.Models
{
    public class Book
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int PublicationDate { get; set; } = DateTime.MinValue.Year;
        public int PageCount { get; set; } = 0;
        public string Status { get; set; } = "Not started";
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? DateFinished { get; set; } = null;
        public DateTime DateModified { get; set; } = DateTime.Now;
    }
}