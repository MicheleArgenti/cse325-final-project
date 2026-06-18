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
        public DbSet<Category> Categories { get; set; }
        public DbSet<RecipeCategory> RecipeCategories { get; set; }
        public DbSet<Review> Reviews { get; set; } // Add this line

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Recipe - User relationship
            builder.Entity<Recipe>()
                .HasOne(r => r.User)
                .WithMany(u => u.Recipes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure many-to-many relationship between Recipe and Category
            builder.Entity<RecipeCategory>()
                .HasKey(rc => new { rc.RecipeId, rc.CategoryId });

            builder.Entity<RecipeCategory>()
                .HasOne(rc => rc.Recipe)
                .WithMany(r => r.RecipeCategories)
                .HasForeignKey(rc => rc.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RecipeCategory>()
                .HasOne(rc => rc.Category)
                .WithMany(c => c.RecipeCategories)
                .HasForeignKey(rc => rc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Favorite relationships
            builder.Entity<Favorite>(entity =>
            {
                entity.ToTable("Favorites");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .UseIdentityColumn();

                // Map properties to columns
                entity.Property(e => e.UserId)
                    .HasColumnName("UserId")
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.RecipeId)
                    .HasColumnName("RecipeId")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CreatedAt")
                    .IsRequired();

                // Configure relationships WITHOUT creating shadow properties
                entity.HasOne(f => f.User)
                    .WithMany()  // No inverse navigation
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.Recipe)
                    .WithMany()  // No inverse navigation
                    .HasForeignKey(f => f.RecipeId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint
                entity.HasIndex(f => new { f.UserId, f.RecipeId })
                    .IsUnique()
                    .HasDatabaseName("IX_Favorites_UserId_RecipeId");
            });

            // Configure Review relationships
            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Review>()
                .HasOne(r => r.Recipe)
                .WithMany()
                .HasForeignKey(r => r.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure a user can only review a recipe once
            builder.Entity<Review>()
                .HasIndex(r => new { r.UserId, r.RecipeId })
                .IsUnique();

            // Seed default categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Breakfast", Description = "Start your day right", Icon = "fa-sun", Color = "#f39c12" },
                new Category { Id = 2, Name = "Lunch", Description = "Midday meals", Icon = "fa-utensils", Color = "#2ecc71" },
                new Category { Id = 3, Name = "Dinner", Description = "Evening feasts", Icon = "fa-moon", Color = "#3498db" },
                new Category { Id = 4, Name = "Dessert", Description = "Sweet treats", Icon = "fa-cake-candles", Color = "#e74c3c" },
                new Category { Id = 5, Name = "Appetizer", Description = "Starters and snacks", Icon = "fa-bread-slice", Color = "#e67e22" },
                new Category { Id = 6, Name = "Vegetarian", Description = "Meat-free delights", Icon = "fa-leaf", Color = "#27ae60" },
                new Category { Id = 7, Name = "Vegan", Description = "Plant-based", Icon = "fa-seedling", Color = "#2ecc71" },
                new Category { Id = 8, Name = "Gluten Free", Description = "No gluten", Icon = "fa-wheat-awn-slash", Color = "#f1c40f" },
                new Category { Id = 9, Name = "Quick & Easy", Description = "Under 30 minutes", Icon = "fa-bolt", Color = "#e67e22" },
                new Category { Id = 10, Name = "Healthy", Description = "Nutritious choices", Icon = "fa-heartbeat", Color = "#1abc9c" }
            );
        }
    }
}