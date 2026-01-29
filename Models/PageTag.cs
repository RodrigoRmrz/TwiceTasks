namespace TwiceTasks.Models
{
    public class PageTag
    {
        public int PageId { get; set; }
        public Page Page { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
