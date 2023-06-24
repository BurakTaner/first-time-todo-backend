using Microsoft.EntityFrameworkCore;
using TodoBackend.Models;

namespace TodoBackend.Data;

public class TodosDbContext : DbContext
{
    public TodosDbContext(DbContextOptions<TodosDbContext> options) : base(options)
    {

    }

    public DbSet<Todo> Todos { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurations
        modelBuilder.Entity<User>()
                        .HasMany(a => a.Todos)
                        .WithOne(b => b.User)
                        .HasForeignKey(b => b.UserId);
        //

        modelBuilder.Entity<Todo>().HasData(
            new Todo { Id = 1, Title = "Kiss wife and children", IsFinished = false, Description = "I am never going finish this because of my fault.", UserId = 1 }
            );

        modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "Burak", Password = "I amar prestar aen" }
                );
    }

}
