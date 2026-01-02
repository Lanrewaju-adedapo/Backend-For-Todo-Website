using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TestProject.Models.Authentication;
using TestProject.Models.Entities;

namespace TestProject.Data
{
    public class TodoAppContext : IdentityDbContext<Users, IdentityRole<int>, int>
    {
        public TodoAppContext(DbContextOptions<TodoAppContext> options) : base(options)
        {
        }

        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Users> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("users");

                entity.Property(u => u.PasswordHash).HasColumnName("password_hash");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.RefreshTokenExpiry)
                    .IsRequired(false); // Ensure it's nullable
            });

            modelBuilder.Entity<Tasks>(entity =>
            {
                // Configure the relationship
                entity.HasOne(t => t.User)
                      .WithMany(u => u.Tasks)
                      .HasForeignKey(t => t.User_id)
                      .OnDelete(DeleteBehavior.Cascade);

                // Map the property to the database column
                entity.Property(t => t.User_id)
                      .HasColumnName("user_id");
            });

            modelBuilder.Entity<Categories>(entity =>
            {
                // Configure the relationship
                entity.HasOne(t => t.User)
                      .WithMany(u => u.Categories)
                      .HasForeignKey(t => t.User_id)
                      .OnDelete(DeleteBehavior.Cascade);

                // Map the property to the database column
                entity.Property(t => t.User_id)
                      .HasColumnName("user_id");
            });

            //var adminUser = new Users
            //{
            //    Id = 1, 
            //    UserName = "adefaludapo@gmail.com",
            //    Email = "adefaludapo@gmail.com",
            //    FirstName = "Admin",
            //    LastName = "Admin",
            //    EmailVerified = true,
            //    RefreshToken = null, 
            //    RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1), 
            //    SecurityStamp = Guid.NewGuid().ToString(),
            //    ConcurrencyStamp = Guid.NewGuid().ToString(),
            //    LockoutEnabled = true,
            //    AccessFailedCount = 0,
            //    TwoFactorEnabled = false,
            //    PhoneNumberConfirmed = false
            //};

            //modelBuilder.Entity<Users>().HasData(adminUser);

            //modelBuilder.Entity<Categories>().HasData(
            //    new Categories
            //    {
            //        CategoryId = 2,
            //        CategoryName = "Personal",
            //        ColorCode = "#4CAF50",
            //        User_id = adminUser.Id
            //    }
            //);
        }
    }
}
