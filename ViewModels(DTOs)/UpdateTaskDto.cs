namespace TestProject.ViewModels_DTOs_
{
    public class UpdateTaskDto
    {
        public int TaskId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public byte? Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public int? CategoryId { get; set; }

    }
}
