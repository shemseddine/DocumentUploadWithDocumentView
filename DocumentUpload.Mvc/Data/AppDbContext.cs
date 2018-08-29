using Microsoft.EntityFrameworkCore;

namespace DocumentUpload.Mvc.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            //Database.Migrate();
        }
        public DbSet<DocumentEntity> Documents { get; set; }
    }
}
