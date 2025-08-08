using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestProject.Data;
using TestProject.IRepo;
using TestProject.Models.Entities;
using TestProject.POCO;
using TestProject.ViewModels_DTOs_;

namespace TestProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodosController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody] CreateTodoDto todoDto)
        {
            var result = await _todoService.CreateTodo(todoDto);
            return Ok(result); 
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTaskById(int taskId)
        {
            if (taskId <= 0)
                return BadRequest(new StatusMessage { Status = "Failed", Message = "TaskId must be positive." });

            var result = await _todoService.GetTaskById(taskId);

            return result.Status.ToLower() == "failed"
                ? NotFound(result)
                : Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var result = await _todoService.GetAllTasks();
            return Ok(result); 
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskDto updateTask)
        {
            var result = await _todoService.UpdateTask(updateTask);

            return result.Status.ToLower() == "failed" ? BadRequest(result) : Ok(result);
        }

        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            if (taskId <= 0)
                return BadRequest(new StatusMessage { Status = "Failed", Message = "TaskId must be positive." });

            var result = await _todoService.DeleteTask(taskId);

            return result.Status.ToLower() == "failed"
                ? NotFound(result)
                : Ok(result);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var result = await _todoService.GetAllCategories();
            return Ok(result); 
        }

        [HttpPut("CompleteTask")]
        public async Task<IActionResult> CompleteTask([FromBody] CompleteTaskDto taskDto)
        {
            var result = await _todoService.CompleteTask(taskDto);

            return result.Status.ToLower() == "failed"
                ? BadRequest(result)
                : Ok(result);
        }

        [HttpGet("getcategory/{CategoryId}")]
        public async Task<IActionResult> GetTaskByCategory(int CategoryId)
        {
            var result = await _todoService.GetTasksByCategory(CategoryId);
            return Ok(result);
        }
    }
}