using Microsoft.AspNetCore.Identity;
using RecipeManagement.Models;

namespace RecipeManagement.Data
{
  public static class DbInitializer
  {
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
      using var scope = serviceProvider.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
      var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

      // Ensure database is created
      await context.Database.EnsureCreatedAsync();

      // Create roles if they don't exist
      string[] roleNames = { "Admin", "User" };
      foreach (var roleName in roleNames)
      {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
          await roleManager.CreateAsync(new IdentityRole(roleName));
        }
      }

      // Create admin user if it doesn't exist
      var adminEmail = "admin@recipeapp.com";
      var adminUser = await userManager.FindByEmailAsync(adminEmail);
      if (adminUser == null)
      {
        adminUser = new ApplicationUser
        {
          UserName = adminEmail,
          Email = adminEmail,
          FirstName = "Admin",
          LastName = "User",
          CreatedAt = DateTime.UtcNow
        };
        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
          await userManager.AddToRoleAsync(adminUser, "Admin");
          Console.WriteLine("✅ Admin user created successfully!");
        }
        else
        {
          Console.WriteLine("❌ Failed to create admin user:");
          foreach (var error in result.Errors)
          {
            Console.WriteLine($"   - {error.Description}");
          }
        }
      }

      // Create sample regular user if it doesn't exist
      var userEmail = "user@example.com";
      var regularUser = await userManager.FindByEmailAsync(userEmail);
      if (regularUser == null)
      {
        regularUser = new ApplicationUser
        {
          UserName = userEmail,
          Email = userEmail,
          FirstName = "Sample",
          LastName = "User",
          CreatedAt = DateTime.UtcNow
        };
        var result = await userManager.CreateAsync(regularUser, "User@123");
        if (result.Succeeded)
        {
          await userManager.AddToRoleAsync(regularUser, "User");
          Console.WriteLine("✅ Sample user created successfully!");
        }
      }

      // Add sample recipes if none exist
      if (!context.Recipes.Any())
      {
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin != null)
        {
          var sampleRecipes = new[]
          {
                        new Recipe
                        {
                            Title = "Classic Spaghetti Carbonara",
                            Description = "A creamy Italian pasta dish made with eggs, cheese, pancetta, and pepper.",
                            Ingredients = "200g spaghetti\n100g pancetta\n2 large eggs\n50g Pecorino Romano\nBlack pepper\nSalt",
                            Instructions = "1. Cook pasta\n2. Fry pancetta\n3. Mix eggs and cheese\n4. Combine all\n5. Serve with pepper",
                            PrepTimeMinutes = 10,
                            CookTimeMinutes = 15,
                            Difficulty = "Medium",
                            Servings = 4,
                            UserId = admin.Id,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Recipe
                        {
                            Title = "Chicken Tikka Masala",
                            Description = "A delicious Indian curry with grilled chicken in a spiced cream sauce.",
                            Ingredients = "500g chicken breast\n1 cup yogurt\n2 tbsp tikka masala spice\n1 onion\n2 cloves garlic\n1 can tomato sauce\n1 cup heavy cream",
                            Instructions = "1. Marinate chicken in yogurt and spices\n2. Grill chicken until cooked\n3. Sauté onions and garlic\n4. Add tomato sauce and cream\n5. Add chicken and simmer",
                            PrepTimeMinutes = 20,
                            CookTimeMinutes = 30,
                            Difficulty = "Medium",
                            Servings = 4,
                            UserId = admin.Id,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Recipe
                        {
                            Title = "Vegetable Stir Fry",
                            Description = "Quick and healthy vegetable stir fry with tofu.",
                            Ingredients = "200g tofu\n1 bell pepper\n1 broccoli\n2 carrots\n3 tbsp soy sauce\n1 tbsp ginger\n2 cloves garlic",
                            Instructions = "1. Press and cube tofu\n2. Chop vegetables\n3. Stir fry tofu until golden\n4. Add vegetables\n5. Add sauce and cook until tender",
                            PrepTimeMinutes = 15,
                            CookTimeMinutes = 10,
                            Difficulty = "Easy",
                            Servings = 2,
                            UserId = admin.Id,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

          await context.Recipes.AddRangeAsync(sampleRecipes);
          await context.SaveChangesAsync();
          Console.WriteLine("✅ Sample recipes added successfully!");
        }
      }

      // Add default categories if none exist
      if (!context.Categories.Any())
      {
        var categories = new[]
        {
                    new Category { Name = "Breakfast", Description = "Start your day right", Icon = "fa-sun", Color = "#f39c12", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Lunch", Description = "Midday meals", Icon = "fa-utensils", Color = "#2ecc71", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Dinner", Description = "Evening feasts", Icon = "fa-moon", Color = "#3498db", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Dessert", Description = "Sweet treats", Icon = "fa-cake-candles", Color = "#e74c3c", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Appetizer", Description = "Starters and snacks", Icon = "fa-bread-slice", Color = "#e67e22", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Vegetarian", Description = "Meat-free delights", Icon = "fa-leaf", Color = "#27ae60", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Vegan", Description = "Plant-based", Icon = "fa-seedling", Color = "#2ecc71", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Gluten Free", Description = "No gluten", Icon = "fa-wheat-awn-slash", Color = "#f1c40f", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Quick & Easy", Description = "Under 30 minutes", Icon = "fa-bolt", Color = "#e67e22", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Healthy", Description = "Nutritious choices", Icon = "fa-heartbeat", Color = "#1abc9c", CreatedAt = DateTime.UtcNow }
                };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
        Console.WriteLine("✅ Default categories added successfully!");
      }
    }
  }
}