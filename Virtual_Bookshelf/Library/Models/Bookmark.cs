namespace Virtual_Bookshelf.Library.Models
{
    public class Bookmark
    {
        public int PageNumber { get; set; } = 0;
        public string Color { get; set; } = "red"; // red, green, blue, yellow, purple, orange
        public string Notes { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; } = DateTime.Now;

        public static List<string> AvailableColors = new List<string>
        {
            "red",
            "green",
            "blue",
            "yellow",
            "purple",
            "orange"
        };

        public string GetColorCode()
        {
            return Color switch
            {
                "red" => "🔴",
                "green" => "🟢",
                "blue" => "🔵",
                "yellow" => "🟡",
                "purple" => "🟣",
                "orange" => "🟠",
                _ => "⚪"
            };
        }
    }
}
