using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TaskManager.Models;
using Task = TaskManager.Models.Task;

namespace TaskManager.Data
{
    public class TaskDbContext : IdentityDbContext<IdentityUser>
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
        {

        }

        public DbSet<Task> Tasks { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Entity> Entities { get; set; }
        public DbSet<TaskTag> TaskTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskTag>().HasKey(c => new { c.TaskId, c.TagId });
            
            base.OnModelCreating(modelBuilder);
        }
        
    }
}
