using Microsoft.AspNetCore.Mvc;
using TestProject.Models.Entities;
using TestProject.POCO;
using TestProject.ViewModels_DTOs_;

namespace TestProject.IRepo
{
    public interface ITodoService 
    {
        public Task<ResponseDto> CreateTodo(CreateTodoDto todoDto);        
        public Task<StatusMessage> GetTaskById(int TaskId);
        public Task<StatusMessage> GetAllTasks();
        public Task<StatusMessage> GetAllTodayTasks();
        public Task<StatusMessage> GetAllUpcomingTasks();
        public Task<StatusMessage> GetAllDueTasks();
        public Task<StatusMessage> GetAllCompletedTasks();
        public Task<StatusMessage> UpdateTask(UpdateTaskDto updateTask);
        public Task<StatusMessage> DeleteTask(int TaskId);
        public Task<StatusMessage> GetAllCategories(); 
        public Task<StatusMessage> CompleteTask(CompleteTaskDto Task);
        public Task<StatusMessage> GetTasksByCategory (int CategoryId);
        public Task<StatusMessage> AddNewCategory(CreateCategoryDto category);
        public Task<StatusMessage> DeleteCategory(int CategoryId);
    }
}
