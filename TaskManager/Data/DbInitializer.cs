using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Models;


namespace TaskManager.Data
{
    public static class DbInitializer
    {
        public static async System.Threading.Tasks.Task Initialize(IServiceProvider provider)
        {

           
            

            using(var context = new TaskDbContext(provider.GetRequiredService<DbContextOptions<TaskDbContext>>()))
            {

                var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
                var logger = provider.GetRequiredService<ILogger<Program>>();
                var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
                await context.Database.MigrateAsync();

            if (context.Entities.Any())
            {
                return;
            }


            var user = new IdentityUser { UserName = "kirill", Email = "ahoj@ahoj.cz" };

            await userManager.CreateAsync(user, "#Ahoj234");

            var newUser = await userManager.FindByNameAsync(user.UserName);


            var tasks = new Models.Task[]
            {
                new Models.Task {Title = "Some title", Note = "Some note", CreationTime = DateTime.Now, LastEditedTime = DateTime.Now, HasToBeDoneTime = DateTime.Now.AddDays(5), UserId = newUser.Id},
                new Models.Task {Title = "Some good title", Note = "Some good note", CreationTime = DateTime.Now, LastEditedTime = DateTime.Now, HasToBeDoneTime = DateTime.Now.AddDays(7),UserId = newUser.Id},
            };

            await context.Tasks.AddRangeAsync(tasks);
            

            var tags = new Tag[]
            {
                new Tag{Title = "Some tag title", Color = "#ffffff", CreationTime = DateTime.Now, LastEditedTime = DateTime.Now,UserId = newUser.Id}
            };

            await context.Tags.AddRangeAsync(tags);

            var TasksTags = new TaskTag[]
            {
                new TaskTag{Tag = tags[0], Task = tasks[0]},
                new TaskTag{Tag = tags[0], Task = tasks[1]},
            };

            await context.TaskTags.AddRangeAsync(TasksTags);


            

            await context.SaveChangesAsync();
           }


        }
    }
}
