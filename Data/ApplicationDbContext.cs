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
            : base(options) { }

        public DbSet<Host> Hosts { get; set; }
        public DbSet<HostList> HostLists { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ApplicationEvent> ApplicationEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Host>()
                .HasAlternateKey(x => x.Hostname);
            builder.Entity<HostList>()
                .HasAlternateKey(x => x.Name);

            builder.Entity<Tag>()
                .HasAlternateKey(x => x.Name);

            builder.Entity<HostListToHost>()
                .HasKey(x => new { x.HostListId, x.HostId });

            builder.Entity<TagToHost>()
                .HasKey(x => new { x.TagId, x.HostId });

            builder.Entity<TagAddedEvent>();
            builder.Entity<TagRemovedEvent>();
            builder.Entity<HostOnlineEvent>();
            builder.Entity<HostOfflineEvent>();
        }
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
