using Microsoft.EntityFrameworkCore;
using WEB_API.Models;

namespace WEB_API.Data
{
    public class WEB_APIContext : DbContext
    {
        public WEB_APIContext (DbContextOptions<WEB_APIContext> options)
            : base(options)
        {
        }

        public DbSet<ClaimModel> ClaimModel { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
