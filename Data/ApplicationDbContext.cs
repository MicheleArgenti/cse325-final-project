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
    public DbSet<Favorite> Favorites { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      // Configure relationships
      builder.Entity<Recipe>()
          .HasOne(r => r.User)
          .WithMany(u => u.Recipes)
          .HasForeignKey(r => r.UserId)
          .OnDelete(DeleteBehavior.Cascade);

      // Configure Favorite relationships
      builder.Entity<Favorite>()
          .HasOne(f => f.User)
          .WithMany()
          .HasForeignKey(f => f.UserId)
          .OnDelete(DeleteBehavior.Cascade);

      builder.Entity<Favorite>()
          .HasOne(f => f.Recipe)
          .WithMany()
          .HasForeignKey(f => f.RecipeId)
          .OnDelete(DeleteBehavior.Cascade);

      // Ensure a user can only favorite a recipe once
      builder.Entity<Favorite>()
          .HasIndex(f => new { f.UserId, f.RecipeId })
          .IsUnique();
    }
  }
}