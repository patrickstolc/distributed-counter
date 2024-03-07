using SharedModels;

namespace CounterService.Core.Helpers;

using Microsoft.EntityFrameworkCore;

public class LikeDataContext : DbContext {

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseInMemoryDatabase("LikeDb");
    }
    
    public DbSet<Like> Likes { get; set; }
}