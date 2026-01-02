using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TestProject.Data;
using TestProject.IRepo;
using TestProject.Models.Entities;
using TestProject.POCO;
using TestProject.Services.Helpers;
using TestProject.ViewModels_DTOs_;

namespace TestProject.Services
{
    public class TodoService : ITodoService
    {
        private readonly TodoAppContext _dBcontext;
        private readonly IUserAccessor _userAccessor;

        public TodoService(TodoAppContext dBcontext, IUserAccessor userAccessor)
        {
            _dBcontext = dBcontext;
            _userAccessor = userAccessor;
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

            var loggedInUser = Convert.ToInt16(_userAccessor.GetCurrentUserId());
            const byte DefaultPriority = 0;

            var todoData = new Tasks
            {
                Title = todoDto.Title,
                Description = todoDto.Description,
                Priority = todoDto.Priority.HasValue ? todoDto.Priority : DefaultPriority,
                CreatedAt = DateTime.UtcNow,
                DueDate = todoDto.DueDate,
                IsCompleted = false,
                CategoryId = todoDto.CategoryId.HasValue ? todoDto.CategoryId.Value : null,
                User_id = loggedInUser
            };

            await _dBcontext.Tasks.AddAsync(todoData);
            await _dBcontext.SaveChangesAsync();

            return new ResponseDto
            {
                TaskId = todoData.TaskId,
                Title = todoData.Title,
                Description = todoData.Description,
                Priority = todoData.Priority,
                DueDate = todoData.DueDate,
                CreatedAt = todoData.CreatedAt
            };
        }

        public async Task<StatusMessage> GetAllTasks()
        {
            try
            {
                var loggedInUser = _userAccessor.GetCurrentUserId();
                var tasks = await (from task in _dBcontext.Tasks
                                   where task.User_id.ToString() == loggedInUser
                                   join category in _dBcontext.Categories on task.CategoryId equals category.CategoryId into taskCategory
                                   from category in taskCategory.DefaultIfEmpty()
                                   select new ResponseDto
                                   {
                                       TaskId = task.TaskId,
                                       Title = task.Title,
                                       Description = task.Description,
                                       Priority = task.Priority,
                                       CreatedAt = task.CreatedAt,
                                       DueDate = task.DueDate,
                                       IsCompleted = task.IsCompleted,
                                       LastModified = task.LastModified,
                                       CategoryId = task.CategoryId,
                                       ColorCode = category.ColorCode,
                                       CategoryName = category.CategoryName,
                                   }).ToListAsync();
                // var tasks = await _dBcontext.Tasks.Where(x => x.User_id.ToString() == loggedInUser).Join(_dBcontext.Categories, task => task.CategoryId, category => category.CategoryId, (task, category) => new ResponseDto
                // {
                //     TaskId = task.TaskId,
                //     Title = task.Title,
                //     Description = task.Description,
                //     Priority = task.Priority,
                //     CreatedAt = task.CreatedAt,
                //     DueDate = task.DueDate,
                //     IsCompleted = task.IsCompleted,
                //     LastModified = task.LastModified,
                //     CategoryId = task.CategoryId,
                //     ColorCode = category.ColorCode,
                //     CategoryName = category.CategoryName,
                // }).ToListAsync();

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

        public async Task<StatusMessage> GetAllTodayTasks()
        {
            try
            {
                var loggedInUser = _userAccessor.GetCurrentUserId();

                var today = DateTime.Today;
                var tasks = await (from task in _dBcontext.Tasks
                                   where task.DueDate.HasValue &&
                                   task.IsCompleted != true &&
                                   task.DueDate.Value.Date == today && task.User_id.ToString() == loggedInUser.ToString()
                                   join
                                   category in _dBcontext.Categories on task.CategoryId equals category.CategoryId into taskCategory
                                   from category in taskCategory.DefaultIfEmpty()
                                   select new ResponseDto
                                   {
                                       TaskId = task.TaskId,
                                       Title = task.Title,
                                       Description = task.Description,
                                       Priority = task.Priority,
                                       CreatedAt = task.CreatedAt,
                                       DueDate = task.DueDate,
                                       IsCompleted = task.IsCompleted,
                                       LastModified = task.LastModified,
                                       CategoryId = task.CategoryId,
                                       ColorCode = category.ColorCode,
                                       CategoryName = category.CategoryName,
                                   }).ToListAsync();
                // var tasks = await _dBcontext.Tasks.Where(x => x.DueDate.HasValue && x.DueDate.Value.Date == today && x.User_id.ToString() == loggedInUser).Join(_dBcontext.Categories, task => task.CategoryId, category => category.CategoryId, (task, category) => new ResponseDto
                // {
                //     TaskId = task.TaskId,
                //     Title = task.Title,
                //     Description = task.Description,
                //     Priority = task.Priority,
                //     CreatedAt = task.CreatedAt,
                //     DueDate = task.DueDate,
                //     IsCompleted = task.IsCompleted,
                //     LastModified = task.LastModified,
                //     CategoryId = task.CategoryId,
                //     ColorCode = category.ColorCode,
                //     CategoryName = category.CategoryName,
                // }).ToListAsync();

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

        public async Task<StatusMessage> GetAllUpcomingTasks()
        {
            try
            {
                var loggedInUser = _userAccessor.GetCurrentUserId();
                var today = DateTime.Today.AddDays(1);
                var tasks = await (from task in _dBcontext.Tasks
                                   where task.DueDate.HasValue &&
                                         task.DueDate.Value.Date >= today &&
                                         task.IsCompleted != true &&
                                         task.User_id.ToString() == loggedInUser.ToString()
                                   join category in _dBcontext.Categories
                                   on task.CategoryId equals category.CategoryId into taskCategory
                                   from category in taskCategory.DefaultIfEmpty()
                                   select new ResponseDto
                                   {
                                       TaskId = task.TaskId,
                                       Title = task.Title,
                                       Description = task.Description,
                                       Priority = task.Priority,
                                       CreatedAt = task.CreatedAt,
                                       DueDate = task.DueDate,
                                       IsCompleted = task.IsCompleted,
                                       LastModified = task.LastModified,
                                       CategoryId = task.CategoryId,
                                       ColorCode = category.ColorCode,
                                       CategoryName = category.CategoryName,
                                   }).ToListAsync();

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

        public async Task<StatusMessage> GetAllDueTasks()
        {
            try
            {
                var loggedInUser = _userAccessor.GetCurrentUserId();
                var today = DateTime.UtcNow;
                var tasks = await (from task in _dBcontext.Tasks
                                   where task.DueDate.HasValue && task.DueDate.Value <= today && task.IsCompleted != true && task.User_id.ToString() == loggedInUser
                                   join category in _dBcontext.Categories on task.CategoryId equals category.CategoryId into taskCategory
                                   from category in taskCategory.DefaultIfEmpty()
                                   select new ResponseDto
                                   {
                                       TaskId = task.TaskId,
                                       Title = task.Title,
                                       Description = task.Description,
                                       Priority = task.Priority,
                                       CreatedAt = task.CreatedAt,
                                       DueDate = task.DueDate,
                                       IsCompleted = task.IsCompleted,
                                       LastModified = task.LastModified,
                                       CategoryId = task.CategoryId,
                                       ColorCode = category.ColorCode,
                                       CategoryName = category.CategoryName,
                                   }).ToListAsync();
                // var tasks = await _dBcontext.Tasks.Where(x => x.DueDate.HasValue && x.DueDate.Value <= today && x.IsCompleted != true && x.User_id.ToString() == loggedInUser).Join(_dBcontext.Categories, task => task.CategoryId, category => category.CategoryId, (task, category) => new ResponseDto
                // {
                //     TaskId = task.TaskId,
                //     Title = task.Title,
                //     Description = task.Description,
                //     Priority = task.Priority,
                //     CreatedAt = task.CreatedAt,
                //     DueDate = task.DueDate,
                //     IsCompleted = task.IsCompleted,
                //     LastModified = task.LastModified,
                //     CategoryId = task.CategoryId,
                //     ColorCode = category.ColorCode,
                //     CategoryName = category.CategoryName,
                // }).ToListAsync();

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
                var loggedInUser = _userAccessor.GetCurrentUserId();
                var taskDetails = await _dBcontext.Tasks.Where(x => x.TaskId == taskId && x.User_id.ToString() == loggedInUser).Join(_dBcontext.Categories, task => task.CategoryId, category => category.CategoryId, (task, category) => new ResponseDto
                {
                    TaskId = task.TaskId,
                    Title = task.Title,
                    Description = task.Description,
                    Priority = task.Priority,
                    CreatedAt = task.CreatedAt,
                    DueDate = task.DueDate,
                    IsCompleted = task.IsCompleted,
                    LastModified = task.LastModified,
                    CategoryId = task.CategoryId,
                    ColorCode = category.ColorCode,
                    CategoryName = category.CategoryName,
                }).FirstOrDefaultAsync(); ;

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
            var loggedInUser = Convert.ToInt16(_userAccessor.GetCurrentUserId());

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

            existingTask.Title = string.IsNullOrEmpty(updateTask.Title) ? existingTask.Title : updateTask.Title;
            existingTask.Description = string.IsNullOrEmpty(updateTask.Description) ? existingTask.Description : updateTask.Description;
            existingTask.Priority = updateTask.Priority.HasValue ? updateTask.Priority : existingTask.Priority;
            existingTask.DueDate = updateTask.DueDate.HasValue ? updateTask.DueDate : existingTask.DueDate;
            existingTask.LastModified = DateTime.UtcNow;
            existingTask.User_id = loggedInUser;

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
                var loggedInUser = _userAccessor.GetCurrentUserId();

                var existingTask = await _dBcontext.Tasks.Where(x => x.TaskId == taskId && x.User_id.ToString() == loggedInUser).FirstOrDefaultAsync();

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
                var loggedInUser = _userAccessor.GetCurrentUserId();
                var categories = await _dBcontext.Categories.Where(x => x.User_id.ToString() == loggedInUser).ToListAsync();

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
                var loggedInUser = _userAccessor.GetCurrentUserId();
                var existingTask = await _dBcontext.Tasks
                    .FirstOrDefaultAsync(x => x.TaskId == taskDto.TaskId && x.User_id.ToString() == loggedInUser);

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
                        Message = taskDto.IsCompleted == true ? "Task is already completed." : "Task is already uncompleted."
                    };
                }

                existingTask.IsCompleted = taskDto.IsCompleted;
                await _dBcontext.SaveChangesAsync();

                return new StatusMessage()
                {
                    Status = "Success",
                    Message = taskDto.IsCompleted == true ? "Task completed successfully." : "Task uncompleted successfully."
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
                var loggedInUser = _userAccessor.GetCurrentUserId();
                var category = await _dBcontext.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.User_id.ToString() == loggedInUser);

                if (category == null)
                {
                    return new StatusMessage
                    {
                        Status = "Failed",
                        Message = $"Category with ID {categoryId} not found.",
                        data = new List<Tasks>()
                    };
                }
                var tasks = await _dBcontext.Tasks.Where(t => t.CategoryId == categoryId && t.User_id.ToString() == loggedInUser).ToListAsync();

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

        public async Task<StatusMessage> GetAllCompletedTasks()
        {
            try
            {
                var loggedInUser = _userAccessor.GetCurrentUserId();
                var tasks = await (from task in _dBcontext.Tasks
                                   where task.IsCompleted == true && task.User_id.ToString() == loggedInUser.ToString()
                                   join category in _dBcontext.Categories on task.CategoryId equals category.CategoryId into taskCategory
                                   from category in taskCategory.DefaultIfEmpty()
                                   select new ResponseDto
                                   {
                                       TaskId = task.TaskId,
                                       Title = task.Title,
                                       Description = task.Description,
                                       Priority = task.Priority,
                                       CreatedAt = task.CreatedAt,
                                       DueDate = task.DueDate,
                                       IsCompleted = task.IsCompleted,
                                       LastModified = task.LastModified,
                                       CategoryId = task.CategoryId,
                                       ColorCode = category.ColorCode,
                                       CategoryName = category.CategoryName,
                                   }).ToListAsync();
                // var tasks = await _dBcontext.Tasks.Where(x => x.IsCompleted == true && x.User_id.ToString() == loggedInUser).Join(_dBcontext.Categories, task => task.CategoryId, category => category.CategoryId, (task, category) => new ResponseDto
                // {
                //     TaskId = task.TaskId,
                //     Title = task.Title,
                //     Description = task.Description,
                //     Priority = task.Priority,
                //     CreatedAt = task.CreatedAt,
                //     DueDate = task.DueDate,
                //     IsCompleted = task.IsCompleted,
                //     LastModified = task.LastModified,
                //     CategoryId = task.CategoryId,
                //     ColorCode = category.ColorCode,
                //     CategoryName = category.CategoryName
                // }).ToListAsync();

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

        public async Task<StatusMessage> AddNewCategory(CreateCategoryDto category)
        {
            if (string.IsNullOrEmpty(category.CategoryName))
            {
                return new StatusMessage
                {
                    Status = "Failed",
                    Message = "Category name required"
                };
            }

            var loggedInUser = Convert.ToInt16(_userAccessor.GetCurrentUserId());
            bool exists = await _dBcontext.Categories.AnyAsync(c => c.CategoryName == category.CategoryName && loggedInUser == c.User_id);
            if (exists)
            {
                return new StatusMessage
                {
                    Status = "Failed",
                    Message = "Category already exists"
                };
            }

            var CategoryObj = new Categories
            {
                CategoryName = category.CategoryName,
                ColorCode = category.ColorCode,
                User_id = loggedInUser
            };

            await _dBcontext.Categories.AddAsync(CategoryObj);
            await _dBcontext.SaveChangesAsync();


            return new StatusMessage
            {
                Status = "Success",
                Message = "Category added successfully",
                data = CategoryObj
            };
        }

        public async Task<StatusMessage> DeleteCategory(int CategoryId)
        {
            if (CategoryId <= 0)
            {
                return new StatusMessage()
                {
                    Status = "Failed",
                    Message = "Invalid Category ID."
                };
            }

            try
            {
                var loggedInUser = _userAccessor.GetCurrentUserId();
                var existingCategory = await _dBcontext.Categories.Where(c => c.User_id.ToString() == loggedInUser && c.CategoryId == CategoryId).FirstOrDefaultAsync();

                if (existingCategory == null)
                {
                    return new StatusMessage()
                    {
                        Status = "Failed",
                        Message = $"Category with ID {CategoryId} not found."
                    };
                }


                var tasksInCategory = await _dBcontext.Tasks.Where(t => t.CategoryId == CategoryId).ToListAsync();

                if (tasksInCategory.Any())
                {
                    foreach (var task in tasksInCategory)
                    {
                        _dBcontext.Remove(task);
                    }
                    await _dBcontext.SaveChangesAsync();
                }

                _dBcontext.Categories.Remove(existingCategory);
                int affectedRows = await _dBcontext.SaveChangesAsync();

                if (affectedRows == 1)
                {
                    return new StatusMessage()
                    {
                        Status = "Success",
                        Message = $"{existingCategory.CategoryName} category deleted successfully."
                    };
                }
                else
                {
                    return new StatusMessage()
                    {
                        Status = "Failed",
                        Message = $"Failed to delete Category with ID {CategoryId}."
                    };
                }
            }
            catch (Exception ex)
            {
                return new StatusMessage()
                {
                    Status = "Failed",
                    Message = $"An error occurred while deleting the Category: {ex.Message}"
                };
            }
        }
    }
}