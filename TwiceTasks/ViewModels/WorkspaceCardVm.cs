namespace TwiceTasks.ViewModels
{
    public class WorkspaceCardVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int PageCount { get; set; }
        public int FileCount { get; set; }
    }
}
