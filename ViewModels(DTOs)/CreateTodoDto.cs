namespace TestProject.ViewModels_DTOs_
{
    public class CreateTodoDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public byte Priority { get; set; }
        public int CategoryId { get; set; }
    }
}
