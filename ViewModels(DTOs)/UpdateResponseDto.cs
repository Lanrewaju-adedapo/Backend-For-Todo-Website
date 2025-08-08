namespace TestProject.ViewModels_DTOs_
{
    public class UpdateResponseDto
    {
        public int TaskId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public byte Priority { get; set; } // 0: Low, 1: Medium, 2: High
        public DateTime? LastModified { get; set; }
    }
}
