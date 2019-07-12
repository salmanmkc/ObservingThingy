using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ObservingThingy.Data;

namespace ObservingThingy.DataAccess
{
    public class TagsRepository
    {
        private readonly Func<ApplicationDbContext> _factory;
        private readonly ILogger<TagsRepository> _logger;

        public TagsRepository(ILoggerFactory loggerfactory, Func<ApplicationDbContext> factory)
        {
            _factory = factory;
            _logger = loggerfactory.CreateLogger<TagsRepository>();
        }

        internal async Task<List<Tag>> GetAll()
        {
            using (var context = _factory())
                return await context.Tags
                    .ToListAsync();
        }

        internal async Task<List<Tag>> GetAllVisible()
        {
            using (var context = _factory())
                return await context.Tags
                    .Where(x => x.IsValid == true)
                    .Where(x => x.IsVisible == true)
                    .ToListAsync();
        }

        internal async Task<Tag> Get(int id)
        {
            using (var context = _factory())
                return await context.Tags
                    .SingleAsync(x => x.Id == id);
        }
        internal async Task<Tag> Get(string name)
        {
            using (var context = _factory())
                return await context.Tags
                    .SingleAsync(x => x.Name == name);
        }


        internal async Task Create(Tag tag)
        {
            using (var context = _factory())
            {
                await context.Tags.AddAsync(tag);
                await context.SaveChangesAsync();
            }
        }

        internal async Task Update(Tag tag)
        {
            using (var context = _factory())
            {
                context.Tags.Update(tag);
                await context.SaveChangesAsync();
            }
        }

        internal async Task Delete(Tag tag)
        {
            using (var context = _factory())
            {
                context.Tags.Remove(tag);
                await context.SaveChangesAsync();
            }
        }

        internal async Task<List<Tag>> GetTagsForHost(int hostid)
        {
            using (var context = _factory())
                return await context.Set<TagToHost>()
                    .Include(x => x.Tag)
                    .Where(x => x.HostId == hostid)
                    .Select(x => x.Tag)
                    .ToListAsync();

        }

        internal async Task AddTagToHost(Tag tag, Host host)
        {
            using (var context = _factory())
            {
                await context.Set<TagToHost>()
                    .AddAsync(new TagToHost { TagId = tag.Id, HostId = host.Id });
                await context.SaveChangesAsync();
            }
        }

        internal async Task AddTagToHost(string tagname, Host host)
        {
            await AddTagToHost(await Get(tagname), host);
        }

        internal async Task RemoveTagFromHost(Tag tag, Host host)
        {
            using (var context = _factory())
            {
                context.Set<TagToHost>()
                    .Remove(new TagToHost { TagId = tag.Id, HostId = host.Id });
                await context.SaveChangesAsync();
            }
        }

        internal async Task RemoveTagFromHost(string tagname, Host host)
        {
            await RemoveTagFromHost(await Get(tagname), host);
        }
    }
}
