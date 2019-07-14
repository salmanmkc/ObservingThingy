using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ObservingThingy.Data;

namespace ObservingThingy
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    using (var context = services.GetRequiredService<ApplicationDbContext>())
                    {
                        await context.Database.EnsureCreatedAsync();

                        await SeedDatabase(context);
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while initializing the database.");
                }
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseUrls("http://*:5000");
                });

        private static async Task SeedDatabase(ApplicationDbContext context)
        {
            if (context.Tags.Count() > 0)
                return;

            await context.Tags.AddRangeAsync(new List<Tag>(){
                new Tag { Name = "online",    Color = TagColor.green },
                new Tag { Name = "offline",   Color = TagColor.red },

                new Tag { Name = "step",      Color = TagColor.grey, IsVisible = true , Icon = "step forward"},
                new Tag { Name = "prepare",   Color = TagColor.yellow },
                new Tag { Name = "restart",   Color = TagColor.orange },
                new Tag { Name = "complete",  Color = TagColor.teal },

                // new Tag { Name = "online-1",  Color = TagColor.green },
                // new Tag { Name = "online-2",  Color = TagColor.green },
                // new Tag { Name = "online-3",  Color = TagColor.green },
                // new Tag { Name = "online-4",  Color = TagColor.green },
                // new Tag { Name = "online-5",  Color = TagColor.green },
                // new Tag { Name = "offline-1", Color = TagColor.red },
                // new Tag { Name = "offline-2", Color = TagColor.red },
                // new Tag { Name = "offline-3", Color = TagColor.red },
                // new Tag { Name = "offline-4", Color = TagColor.red },
                // new Tag { Name = "offline-5", Color = TagColor.red },
            });

            await context.SaveChangesAsync();
        }
    }
}
