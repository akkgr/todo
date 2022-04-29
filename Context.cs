using Microsoft.EntityFrameworkCore;

namespace TodoApi;

class ApiDbContext : DbContext
{
    public DbSet<Item> Items => Set<Item>();

    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }
}
