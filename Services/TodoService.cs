using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TestProject.Data;
using TestProject.IRepo;
using TestProject.Models.Entities;
using TestProject.POCO;
using TestProject.ViewModels_DTOs_;

namespace TestProject.Services
{
    public class TodoService : ITodoService
    {
        private readonly TodoAppContext _dBcontext;

        public TodoService(TodoAppContext dBcontext)
        {
            _dBcontext = dBcontext;
        }

        public async Task<ResponseDto> CreateTodo(CreateTodoDto todoDto)
        {
            if (todoDto == null)
            {
                throw new ArgumentNullException(nameof(todoDto), "Todo data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(todoDto.Title))
            {
                throw new ArgumentException("Title is required.");
            }
            
            if (todoDto.CategoryId <= 0 &&
                !await _dBcontext.Categories.AnyAsync(c => c.CategoryId == todoDto.CategoryId))
            {
                throw new ArgumentException("Invalid CategoryId.");
            }

            var todoData = new Tasks
            {
                Title = todoDto.Title,
                Description = todoDto.Description,
                Priority = todoDto.Priority,
                CreatedAt = DateTime.UtcNow,
                CategoryId = todoDto.CategoryId
            };

            await _dBcontext.Tasks.AddAsync(todoData);
            await _dBcontext.SaveChangesAsync();

            return new ResponseDto
            {
                TaskId = todoData.TaskId,
                Title = todoData.Title,
                Description = todoData.Description,
                Priority = todoData.Priority,
                CreatedAt = todoData.CreatedAt
            };
        }

        public async Task<StatusMessage> GetAllTasks()
        {
            try
            {
                var tasks = await _dBcontext.Tasks.ToListAsync();
                return new StatusMessage
                {
                    Status = "Success",
                    Message = tasks.Any() ? "Tasks retrieved successfully." : "No tasks found.",
                    data = tasks
                };
            }
            catch (Exception ex)
            {
                return new StatusMessage
                {
                    Status = "Failed",
                    Message = "An error occurred while fetching tasks.",
                    data = new List<Tasks>() 
                };
            }
        }

        public async Task<StatusMessage> GetTaskById(int taskId)
        {
            try
            {
                var taskDetails = await _dBcontext.Tasks.FirstOrDefaultAsync(x => x.TaskId == taskId);

                if (taskDetails == null)
                {
                    return new StatusMessage
                    {
                        Status = "Failed",
                        Message = $"Task {taskId} not found."
                    };
                }
                return new StatusMessage
                {
                    Status = "Success",
                    Message = "Task retrieved successfully.",
                    data = taskDetails
                };
            }
            catch (Exception ex)
            {
                return new StatusMessage
                {
                    Status = "Failed",
                    Message = "An error occurred while fetching the task."
                };
            }
        }

        public async Task<StatusMessage> UpdateTask(UpdateTaskDto updateTask)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(updateTask.Title))
            {
                errors.Add("Please enter a title for the task");
            }

            if (errors.Count > 0)
            {
                return new StatusMessage()
                {
                    Status = "Failed",
                    Message = "Field validation failed.",
                    ErrorMessages = errors
                };
            }

            var existingTask = await _dBcontext.Tasks.FirstOrDefaultAsync(x => x.TaskId == updateTask.TaskId);

            if (existingTask == null)
            {
                return new StatusMessage()
                {
                    Status = "Failed",
                    Message = "Task not found",
                    ErrorMessages = new List<string> { "Task not found" }
                };
            }
            existingTask.Title = updateTask.Title;
            existingTask.Description = updateTask.Description ?? existingTask.Description;
            existingTask.Priority = updateTask.Priority;
            existingTask.LastModified = DateTime.UtcNow;

            if (updateTask.CategoryId.HasValue)
            {
                existingTask.CategoryId = updateTask.CategoryId.Value;
            }
            if (updateTask.CategoryId <= 0)
            {
                return new StatusMessage()
                {
                    Status = "Failed",
                    Message = "Invalid CategoryId"
                };
            }

            try
            {
                await _dBcontext.SaveChangesAsync();
                return new StatusMessage()
                {
                    Status = "Success",
                    Message = "Task updated successfully"
                };
            }
            catch (Exception ex)
            {
                return new StatusMessage()
                {
                    Status = "Failed",
                    Message = "Error updating task",
                    ErrorMessages = new List<string> { ex.Message }
                };
            }
        }

        public async Task<StatusMessage> DeleteTask(int taskId)
        {
            if (taskId <= 0)
            {
                return new StatusMessage()
                {
                    Status = "Failed",
                    Message = "Invalid Task ID."
                };
            }
            try
            {
                var existingTask = await _dBcontext.Tasks.FindAsync(taskId);

                if (existingTask == null)
                {
                    return new StatusMessage()
                    {
                        Status = "Failed",
                        Message = $"Task with ID {taskId} not found."
                    };
                }
                _dBcontext.Tasks.Remove(existingTask);
                int affectedRows = await _dBcontext.SaveChangesAsync();

                if (affectedRows == 1)
                {
                    return new StatusMessage()
                    {
                        Status = "Success",
                        Message = $"Task with ID {taskId} deleted successfully."
                    };
                }
                else
                {
                    return new StatusMessage()
                    {
                        Status = "Failed",
                        Message = $"Failed to delete task with ID {taskId}."
                    };
                }
            }
            catch (Exception ex)
            {
                return new StatusMessage()
                {
                    Status = "Failed",
                    Message = "An error occurred while deleting the task."
                };
            }
        }

        public async Task<StatusMessage> GetAllCategories()
        {
            try
            {
                var categories = await _dBcontext.Categories.ToListAsync();

                if (categories == null || !categories.Any())
                {
                    return new StatusMessage()
                    {
                        Status = "Success",
                        Message = "No categories found.",
                        data = new List<Categories>()
                    };
                }
                return new StatusMessage()
                {
                    Status = "Success",
                    Message = "Categories retrieved successfully.",
                    data = categories
                };
            }
            catch (Exception ex)
            {
                return new StatusMessage()
                {
                    Status = "Failed",
                    Message = "An error occurred while fetching categories.",
                    data = new List<Categories>() 
                };
            }
        }

        public async Task<StatusMessage> CompleteTask(CompleteTaskDto taskDto)
        {
            if (taskDto == null)
            {
                return new StatusMessage()
                {
                    Status = "Failed",
                    Message = "Task data cannot be null."
                };
            }

            if (taskDto.TaskId <= 0)
            {
                return new StatusMessage()
                {
                    Status = "Failed",
                    Message = "Invalid Task ID."
                };
            }
            try
            {
                var existingTask = await _dBcontext.Tasks
                    .FirstOrDefaultAsync(x => x.TaskId == taskDto.TaskId);

                if (existingTask == null)
                {
                    return new StatusMessage()
                    {
                        Status = "Failed",
                        Message = $"Task with ID {taskDto.TaskId} not found."
                    };
                }                
                if (existingTask.IsCompleted == taskDto.IsCompleted)
                {
                    return new StatusMessage()
                    {
                        Status = "Failed",
                        Message = taskDto.IsCompleted ? "Task is already completed." : "Task is already uncompleted."
                    };
                }

                existingTask.IsCompleted = taskDto.IsCompleted;
                await _dBcontext.SaveChangesAsync();

                return new StatusMessage()
                {
                    Status = "Success",
                    Message = taskDto.IsCompleted ? "Task completed successfully." : "Task uncompleted successfully."
                };
            }
            catch (Exception ex)
            {
                return new StatusMessage()
                {
                    Status = "Error",
                    Message = "An error occurred while updating the task status."
                };
            }
        }

        public async Task<StatusMessage> GetTasksByCategory(int categoryId)
        {
            if (categoryId <= 0)
            {
                return new StatusMessage
                {
                    Status = "Failed",
                    Message = "Invalid Category ID.",
                    data = new List<Tasks>()  
                };
            }
            try
            {
                var category = await _dBcontext.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);

                if (category == null)
                {
                    return new StatusMessage
                    {
                        Status = "Failed",
                        Message = $"Category with ID {categoryId} not found.",
                        data = new List<Tasks>()
                    };
                }
                var tasks = await _dBcontext.Tasks.Where(t => t.CategoryId == categoryId).ToListAsync();

                return new StatusMessage
                {
                    Status = "Success",
                    Message = tasks.Any() ? "Tasks retrieved successfully." : "No tasks found for this category.",
                    data = tasks,
                    Metadata = category.CategoryName 
                };
            }
            catch (Exception ex)
            {
                return new StatusMessage
                {
                    Status = "Error",  
                    Message = "An internal error occurred while fetching tasks.",
                    data = new List<Tasks>()
                };
            }
        }
    }
}