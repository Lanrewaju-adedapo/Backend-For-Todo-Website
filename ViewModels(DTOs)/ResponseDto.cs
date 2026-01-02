namespace TestProject.ViewModels_DTOs_
{
    public class ResponseDto
    {
        public int TaskId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public byte? Priority { get; set; } // 0: Low, 1: Medium, 2: High        
        public DateTime? CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime? LastModified { get; set; }
        public string? ColorCode { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
