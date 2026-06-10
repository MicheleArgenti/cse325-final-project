using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecipeManagement.Data;
using RecipeManagement.Models;

namespace RecipeManagement.Controllers
{
  [Authorize]
  public class RecipesController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public RecipesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
      _context = context;
      _userManager = userManager;
    }

    // ==================== HELPER METHODS ====================

    // This method gets all categories from the database and marks which ones are selected
    private async Task<List<SelectListItem>> GetCategoriesSelectListAsync(int[]? selectedCategoryIds = null)
    {
      var categories = await _context.Categories
          .OrderBy(c => c.Name)
          .Select(c => new SelectListItem
          {
            Value = c.Id.ToString(),
            Text = c.Name,
            Selected = selectedCategoryIds != null && selectedCategoryIds.Contains(c.Id)
          })
          .ToListAsync();

      return categories;
    }

    private bool RecipeExists(int id)
    {
      return _context.Recipes.Any(e => e.Id == id);
    }

    // ==================== LIST RECIPES ====================

    // GET: Recipes
    [AllowAnonymous]
    public async Task<IActionResult> Index(int? categoryId = null)
    {
      // Start with all recipes
      var query = _context.Recipes
          .Include(r => r.User)
          .Include(r => r.RecipeCategories)  // Include the categories
              .ThenInclude(rc => rc.Category)  // Include the category details
          .AsQueryable();

      // If a category is selected, filter by that category
      if (categoryId.HasValue)
      {
        query = query.Where(r => r.RecipeCategories.Any(rc => rc.CategoryId == categoryId.Value));
      }

      // Order by newest first
      var recipes = await query
          .OrderByDescending(r => r.CreatedAt)
          .ToListAsync();

      // Get all categories for the filter bar
      ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
      ViewBag.SelectedCategory = categoryId;

      return View(recipes);
    }

    // ==================== RECIPE DETAILS ====================

    // GET: Recipes/Details/5
    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var recipe = await _context.Recipes
          .Include(r => r.User)
          .Include(r => r.RecipeCategories)
              .ThenInclude(rc => rc.Category)
          .FirstOrDefaultAsync(m => m.Id == id);

      if (recipe == null)
      {
        return NotFound();
      }

      return View(recipe);
    }

    // ==================== CREATE RECIPE ====================

    // GET: Recipes/Create
    public async Task<IActionResult> Create()
    {
      // Get all categories with their colors and icons
      var categories = await _context.Categories
          .OrderBy(c => c.Name)
          .Select(c => new SelectListItem
          {
            Value = c.Id.ToString(),
            Text = c.Name,
            Selected = false
          })
          .ToListAsync();

      ViewBag.Categories = categories;

      // Also pass the categories with their colors and icons separately
      ViewBag.CategoriesWithDetails = await _context.Categories
          .OrderBy(c => c.Name)
          .ToListAsync();

      return View();
    }

    // POST: Recipes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,Ingredients,Instructions,PrepTimeMinutes,CookTimeMinutes,Difficulty,Servings,ImageUrl")] Recipe recipe, int[] selectedCategories)
    {
      if (ModelState.IsValid)
      {
        // Get the current user
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
          recipe.UserId = user.Id;
        }
        recipe.CreatedAt = DateTime.UtcNow;

        // Save the recipe
        _context.Add(recipe);
        await _context.SaveChangesAsync();

        // Add the selected categories to the recipe
        if (selectedCategories != null && selectedCategories.Any())
        {
          foreach (var categoryId in selectedCategories)
          {
            _context.RecipeCategories.Add(new RecipeCategory
            {
              RecipeId = recipe.Id,
              CategoryId = categoryId
            });
          }
          await _context.SaveChangesAsync();
        }

        TempData["Success"] = "Recipe created successfully!";
        return RedirectToAction(nameof(Index));
      }

      // If something went wrong, reload the categories
      ViewBag.Categories = await GetCategoriesSelectListAsync(selectedCategories);
      return View(recipe);
    }

    // ==================== EDIT RECIPE ====================

    // GET: Recipes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      // Get the recipe with its categories
      var recipe = await _context.Recipes
          .Include(r => r.RecipeCategories)
          .FirstOrDefaultAsync(r => r.Id == id);

      if (recipe == null)
      {
        return NotFound();
      }

      // Check permission
      var user = await _userManager.GetUserAsync(User);
      if (user == null || (recipe.UserId != user.Id && !User.IsInRole("Admin")))
      {
        return Forbid();
      }

      // Get all categories
      var allCategories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

      // Get selected category IDs
      var selectedCategoryIds = recipe.RecipeCategories?.Select(rc => rc.CategoryId).ToArray() ?? new int[0];

      // Create SelectListItems with Selected property set
      var categories = allCategories.Select(c => new SelectListItem
      {
        Value = c.Id.ToString(),
        Text = c.Name,
        Selected = selectedCategoryIds.Contains(c.Id)
      }).ToList();

      ViewBag.Categories = categories;
      ViewBag.CategoriesWithDetails = allCategories;
      ViewBag.SelectedCategories = selectedCategoryIds;

      return View(recipe);
    }

    // POST: Recipes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Ingredients,Instructions,PrepTimeMinutes,CookTimeMinutes,Difficulty,Servings,ImageUrl")] Recipe recipe, int[] selectedCategories)
    {
      if (id != recipe.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          // Get the existing recipe from the database
          var existingRecipe = await _context.Recipes
              .Include(r => r.RecipeCategories)
              .FirstOrDefaultAsync(r => r.Id == id);

          if (existingRecipe == null)
          {
            return NotFound();
          }

          // Check permission
          var user = await _userManager.GetUserAsync(User);
          if (user == null || (existingRecipe.UserId != user.Id && !User.IsInRole("Admin")))
          {
            return Forbid();
          }

          // Update the recipe properties
          existingRecipe.Title = recipe.Title;
          existingRecipe.Description = recipe.Description;
          existingRecipe.Ingredients = recipe.Ingredients;
          existingRecipe.Instructions = recipe.Instructions;
          existingRecipe.PrepTimeMinutes = recipe.PrepTimeMinutes;
          existingRecipe.CookTimeMinutes = recipe.CookTimeMinutes;
          existingRecipe.Difficulty = recipe.Difficulty;
          existingRecipe.Servings = recipe.Servings;
          existingRecipe.ImageUrl = recipe.ImageUrl;
          existingRecipe.UpdatedAt = DateTime.UtcNow;

          // Update categories - remove all existing categories first
          if (existingRecipe.RecipeCategories != null)
          {
            existingRecipe.RecipeCategories.Clear();
          }

          // Add the newly selected categories
          if (selectedCategories != null && selectedCategories.Any())
          {
            foreach (var categoryId in selectedCategories)
            {
              existingRecipe.RecipeCategories?.Add(new RecipeCategory
              {
                RecipeId = recipe.Id,
                CategoryId = categoryId
              });
            }
          }

          await _context.SaveChangesAsync();
          TempData["Success"] = "Recipe updated successfully!";
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!RecipeExists(recipe.Id))
          {
            return NotFound();
          }
          else
          {
            throw;
          }
        }
        return RedirectToAction(nameof(Index));
      }

      // If something went wrong, reload the categories
      ViewBag.Categories = await GetCategoriesSelectListAsync(selectedCategories);
      return View(recipe);
    }

    // ==================== DELETE RECIPE ====================

    // GET: Recipes/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var recipe = await _context.Recipes
          .Include(r => r.User)
          .Include(r => r.RecipeCategories)
              .ThenInclude(rc => rc.Category)
          .FirstOrDefaultAsync(m => m.Id == id);

      if (recipe == null)
      {
        return NotFound();
      }

      var user = await _userManager.GetUserAsync(User);
      if (user == null || (recipe.UserId != user.Id && !User.IsInRole("Admin")))
      {
        return Forbid();
      }

      return View(recipe);
    }

    // POST: Recipes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var recipe = await _context.Recipes.FindAsync(id);
      if (recipe != null)
      {
        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync();
      }

      return RedirectToAction(nameof(Index));
    }

    // ==================== MY RECIPES ====================

    // GET: Recipes/MyRecipes
    public async Task<IActionResult> MyRecipes()
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null)
      {
        return Challenge();
      }

      var recipes = await _context.Recipes
          .Include(r => r.RecipeCategories)
              .ThenInclude(rc => rc.Category)
          .Where(r => r.UserId == user.Id)
          .OrderByDescending(r => r.CreatedAt)
          .ToListAsync();

      return View(recipes);
    }

    // ==================== FAVORITES ====================

    // POST: Recipes/ToggleFavorite/5
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null)
      {
        return Unauthorized();
      }

      var recipe = await _context.Recipes.FindAsync(id);
      if (recipe == null)
      {
        return NotFound();
      }

      // Check if already favorited
      var existingFavorite = await _context.Favorites
          .FirstOrDefaultAsync(f => f.UserId == user.Id && f.RecipeId == id);

      if (existingFavorite != null)
      {
        // Remove from favorites
        _context.Favorites.Remove(existingFavorite);
        await _context.SaveChangesAsync();
        TempData["Message"] = "Recipe removed from favorites";
      }
      else
      {
        // Add to favorites
        var favorite = new Favorite
        {
          UserId = user.Id,
          RecipeId = id,
          CreatedAt = DateTime.UtcNow
        };
        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync();
        TempData["Message"] = "Recipe added to favorites";
      }

      // Return to previous page
      return Redirect(Request.Headers["Referer"].ToString());
    }

    // GET: Recipes/Favorites
    [Authorize]
    public async Task<IActionResult> Favorites()
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null)
      {
        return Challenge();
      }

      var favoriteRecipes = await _context.Favorites
          .Include(f => f.Recipe)
              .ThenInclude(r => r.User)
          .Include(f => f.Recipe)
              .ThenInclude(r => r.RecipeCategories)
                  .ThenInclude(rc => rc.Category)
          .Where(f => f.UserId == user.Id)
          .OrderByDescending(f => f.CreatedAt)
          .Select(f => f.Recipe)
          .ToListAsync();

      return View(favoriteRecipes);
    }

    // GET: Recipes/CheckFavorite/5
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> CheckFavorite(int id)
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null)
      {
        return Json(new { isFavorite = false });
      }

      var isFavorite = await _context.Favorites
          .AnyAsync(f => f.UserId == user.Id && f.RecipeId == id);

      return Json(new { isFavorite = isFavorite });
    }

    // GET: Recipes/FilterByMultipleCategories
    [AllowAnonymous]
    public async Task<IActionResult> FilterByMultipleCategories(int[] selectedCategories, string? searchTerm, string? difficulty, int? minTime, int? maxTime, bool matchAllCategories = false)
    {
      var viewModel = new CategoryFilterViewModel
      {
        SelectedCategoryIds = selectedCategories ?? new int[0],
        SearchTerm = searchTerm,
        Difficulty = difficulty,
        MinTime = minTime,
        MaxTime = maxTime,
        MatchAllCategories = matchAllCategories
      };

      // Get all categories for the filter UI
      viewModel.AllCategories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

      // Start with all recipes
      var query = _context.Recipes
          .Include(r => r.User)
          .Include(r => r.RecipeCategories)
              .ThenInclude(rc => rc.Category)
          .AsQueryable();

      // Filter by multiple categories
      if (selectedCategories != null && selectedCategories.Any())
      {
        if (matchAllCategories)
        {
          // AND logic - recipe must have ALL selected categories
          foreach (var categoryId in selectedCategories)
          {
            query = query.Where(r => r.RecipeCategories.Any(rc => rc.CategoryId == categoryId));
          }
        }
        else
        {
          // OR logic - recipe can have ANY selected category
          query = query.Where(r => r.RecipeCategories.Any(rc => selectedCategories.Contains(rc.CategoryId)));
        }
      }

      // Filter by search term
      if (!string.IsNullOrWhiteSpace(searchTerm))
      {
        searchTerm = searchTerm.ToLower();
        query = query.Where(r =>
            r.Title.ToLower().Contains(searchTerm) ||
            r.Description.ToLower().Contains(searchTerm) ||
            r.Ingredients.ToLower().Contains(searchTerm) ||
            r.Instructions.ToLower().Contains(searchTerm));
      }

      // Filter by difficulty
      if (!string.IsNullOrWhiteSpace(difficulty))
      {
        query = query.Where(r => r.Difficulty == difficulty);
      }

      // Filter by total time
      if (minTime.HasValue)
      {
        query = query.Where(r => (r.PrepTimeMinutes + r.CookTimeMinutes) >= minTime.Value);
      }
      if (maxTime.HasValue)
      {
        query = query.Where(r => (r.PrepTimeMinutes + r.CookTimeMinutes) <= maxTime.Value);
      }

      viewModel.Recipes = await query
          .OrderByDescending(r => r.CreatedAt)
          .ToListAsync();

      return View(viewModel);
    }

    // Helper method to get category color by ID
    private string GetCategoryColor(int categoryId)
    {
      var category = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
      return category?.Color ?? "#6c757d";
    }

    // Helper method to get category icon by ID
    private string GetCategoryIcon(int categoryId)
    {
      var category = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
      return category?.Icon ?? "fa-tag";
    }

    // GET: Recipes/GetReviews/5
    [AllowAnonymous]
    public async Task<IActionResult> GetReviews(int recipeId)
    {
      var reviews = await _context.Reviews
          .Include(r => r.User)
          .Where(r => r.RecipeId == recipeId)
          .OrderByDescending(r => r.CreatedAt)
          .ToListAsync();

      // Calculate average rating
      var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

      return Json(new { reviews, averageRating, totalReviews = reviews.Count });
    }

    // POST: Recipes/AddReview
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(int recipeId, int rating, string? comment)
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null)
      {
        return Unauthorized();
      }

      // Check if user already reviewed this recipe
      var existingReview = await _context.Reviews
          .FirstOrDefaultAsync(r => r.UserId == user.Id && r.RecipeId == recipeId);

      if (existingReview != null)
      {
        // Update existing review
        existingReview.Rating = rating;
        existingReview.Comment = comment;
        existingReview.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Your review has been updated!";
      }
      else
      {
        // Add new review
        var review = new Review
        {
          UserId = user.Id,
          RecipeId = recipeId,
          Rating = rating,
          Comment = comment,
          CreatedAt = DateTime.UtcNow
        };
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Thank you for your review!";
      }

      return RedirectToAction(nameof(Details), new { id = recipeId });
    }

    // GET: Recipes/GetUserReview/5
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetUserReview(int recipeId)
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null)
      {
        return Json(new { hasReview = false });
      }

      var review = await _context.Reviews
          .FirstOrDefaultAsync(r => r.UserId == user.Id && r.RecipeId == recipeId);

      if (review != null)
      {
        return Json(new { hasReview = true, rating = review.Rating, comment = review.Comment });
      }

      return Json(new { hasReview = false });
    }

  }
}