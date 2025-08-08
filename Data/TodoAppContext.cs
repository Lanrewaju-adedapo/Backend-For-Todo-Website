using Microsoft.EntityFrameworkCore;
using TestProject.Models.Entities;

namespace TestProject.Data
{
    public class TodoAppContext : DbContext
    {
        public TodoAppContext(DbContextOptions<TodoAppContext> options) : base(options)
        {
        }

        public DbSet<Tasks> Tasks { get; set; } 
        public DbSet<Categories> Categories { get; set; }
    }
}
