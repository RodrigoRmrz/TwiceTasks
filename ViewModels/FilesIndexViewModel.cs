using TwiceTasks.Models;

namespace TwiceTasks.ViewModels
{
    public class FilesIndexViewModel
    {
        public List<FileResource> Files { get; set; } = new();
        public List<Collection> Collections { get; set; } = new();
        public int? SelectedCollectionId { get; set; }
    }
}
