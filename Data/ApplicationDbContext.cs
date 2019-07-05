using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ObservingThingy.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Host> Hosts { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ApplicationEvent> Events { get; set; }
        public DbSet<Rule> Rules { get; set; }
        public DbSet<AppAction> Actions { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }

    public class AppAction : DataModel
    {
        public string Description { get; set; }
    }

    public interface IDataModel
    {
        int Id { get; set; }
        string Name { get; set; }
        bool IsValid { get; set; }
    }

    public class DataModel : IDataModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsValid { get; set; } = true;
    }
}
