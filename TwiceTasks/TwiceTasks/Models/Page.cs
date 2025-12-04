namespace TwiceTasks.Models
{
    public class Page
    {

        public int PageId { get; set; }
        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsArchived { get; set; } = false;
    }
}