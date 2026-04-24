namespace Library.Models
{
    public class Book
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int PublicationDate { get; set; } = DateTime.MinValue.Year;
        public int PageCount { get; set; } = 0;
        public int PagesRead { get; set; } = 0;
        public string Status { get; set; } = "Not started";
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? DateFinished { get; set; } = null;
        public DateTime DateModified { get; set; } = DateTime.Now;
        public List<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public List<StatusChangeRecord> StatusChanges { get; set; } = new List<StatusChangeRecord>();
    }
}