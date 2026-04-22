namespace Virtual_Bookshelf.Library.Models
{
    public class StatusChangeRecord
    {
        public string PreviousStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime ChangeDate { get; set; } = DateTime.Now;
        public int PagesReadAtChange { get; set; } = 0;
    }
}
