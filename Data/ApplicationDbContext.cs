using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecipeManagement.Models;

namespace RecipeManagement.Data
{
  public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Recipe> Recipes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      // Configure relationships
      builder.Entity<Recipe>()
          .HasOne(r => r.User)
          .WithMany(u => u.Recipes)
          .HasForeignKey(r => r.UserId)
          .OnDelete(DeleteBehavior.Cascade);

      // Remove the seed data - comment out or delete this section
      // builder.Entity<Recipe>().HasData(...)
    }
  }
}