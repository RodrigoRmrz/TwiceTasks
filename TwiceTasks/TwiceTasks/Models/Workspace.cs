namespace TwiceTasks.Models
{
    public class Workspace
    {
        // Id
        public int WorkspaceId { get; set; }
        // UserId
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        // Name
        public string Name { get; set; } = string.Empty;
        // Description
        public string Description { get; set; } = string.Empty;
        // CreatedAt
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}