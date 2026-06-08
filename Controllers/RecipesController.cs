using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

    // GET: Recipes
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
      var recipes = await _context.Recipes
          .Include(r => r.User)
          .OrderByDescending(r => r.CreatedAt)
          .ToListAsync();
      return View(recipes);
    }

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
          .FirstOrDefaultAsync(m => m.Id == id);

      if (recipe == null)
      {
        return NotFound();
      }

      return View(recipe);
    }

    // GET: Recipes/Create
    public IActionResult Create()
    {
      return View();
    }

    // POST: Recipes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,Ingredients,Instructions,PrepTimeMinutes,CookTimeMinutes,Difficulty,Servings,ImageUrl")] Recipe recipe)
    {
      if (ModelState.IsValid)
      {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
          recipe.UserId = user.Id;
        }
        recipe.CreatedAt = DateTime.Now;

        _context.Add(recipe);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }
      return View(recipe);
    }

    // GET: Recipes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var recipe = await _context.Recipes.FindAsync(id);
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

    // POST: Recipes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Ingredients,Instructions,PrepTimeMinutes,CookTimeMinutes,Difficulty,Servings,ImageUrl")] Recipe recipe)
    {
      if (id != recipe.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          var existingRecipe = await _context.Recipes.FindAsync(id);
          if (existingRecipe == null)
          {
            return NotFound();
          }

          var user = await _userManager.GetUserAsync(User);
          if (user == null || (existingRecipe.UserId != user.Id && !User.IsInRole("Admin")))
          {
            return Forbid();
          }

          existingRecipe.Title = recipe.Title;
          existingRecipe.Description = recipe.Description;
          existingRecipe.Ingredients = recipe.Ingredients;
          existingRecipe.Instructions = recipe.Instructions;
          existingRecipe.PrepTimeMinutes = recipe.PrepTimeMinutes;
          existingRecipe.CookTimeMinutes = recipe.CookTimeMinutes;
          existingRecipe.Difficulty = recipe.Difficulty;
          existingRecipe.Servings = recipe.Servings;
          existingRecipe.ImageUrl = recipe.ImageUrl;
          existingRecipe.UpdatedAt = DateTime.Now;

          await _context.SaveChangesAsync();
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
      return View(recipe);
    }

    // GET: Recipes/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var recipe = await _context.Recipes
          .Include(r => r.User)
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

    // GET: Recipes/MyRecipes
    public async Task<IActionResult> MyRecipes()
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null)
      {
        return Challenge();
      }

      var recipes = await _context.Recipes
          .Where(r => r.UserId == user.Id)
          .OrderByDescending(r => r.CreatedAt)
          .ToListAsync();

      return View(recipes);
    }

    // GET: Recipes/Search
    [AllowAnonymous]
    public async Task<IActionResult> Search(string searchTerm, string? difficulty, int? minPrepTime, int? maxPrepTime, string? sortBy)
    {
      var viewModel = new RecipeSearchViewModel
      {
        SearchTerm = searchTerm,
        Difficulty = difficulty,
        MinPrepTime = minPrepTime,
        MaxPrepTime = maxPrepTime,
        SortBy = sortBy
      };

      // Start with all recipes
      var query = _context.Recipes
          .Include(r => r.User)
          .AsQueryable();

      // Apply search term filter
      if (!string.IsNullOrWhiteSpace(searchTerm))
      {
        searchTerm = searchTerm.ToLower();
        query = query.Where(r =>
            r.Title.ToLower().Contains(searchTerm) ||
            r.Description.ToLower().Contains(searchTerm) ||
            r.Ingredients.ToLower().Contains(searchTerm) ||
            r.Instructions.ToLower().Contains(searchTerm) ||
            (r.User != null && (r.User.FirstName + " " + r.User.LastName).ToLower().Contains(searchTerm)) ||
            (r.User != null && r.User.UserName != null && r.User.UserName.ToLower().Contains(searchTerm))
        );
      }

      // Apply difficulty filter
      if (!string.IsNullOrWhiteSpace(difficulty))
      {
        query = query.Where(r => r.Difficulty == difficulty);
      }

      // Apply time filters
      if (minPrepTime.HasValue)
      {
        query = query.Where(r => (r.PrepTimeMinutes + r.CookTimeMinutes) >= minPrepTime.Value);
      }
      if (maxPrepTime.HasValue)
      {
        query = query.Where(r => (r.PrepTimeMinutes + r.CookTimeMinutes) <= maxPrepTime.Value);
      }

      // Apply sorting
      viewModel.SortBy = sortBy;
      query = sortBy switch
      {
        "Oldest First" => query.OrderBy(r => r.CreatedAt),
        "Most Difficult" => query.OrderByDescending(r => r.Difficulty == "Hard")
            .ThenByDescending(r => r.Difficulty == "Medium")
            .ThenBy(r => r.Difficulty == "Easy"),
        "Least Difficult" => query.OrderBy(r => r.Difficulty == "Easy")
            .ThenBy(r => r.Difficulty == "Medium")
            .ThenByDescending(r => r.Difficulty == "Hard"),
        "Shortest Time" => query.OrderBy(r => r.PrepTimeMinutes + r.CookTimeMinutes),
        "Longest Time" => query.OrderByDescending(r => r.PrepTimeMinutes + r.CookTimeMinutes),
        _ => query.OrderByDescending(r => r.CreatedAt) // Newest First
      };

      viewModel.Recipes = await query.ToListAsync();

      return View(viewModel);
    }

    // GET: Recipes/SearchMyRecipes
    public async Task<IActionResult> SearchMyRecipes(string searchTerm, string? difficulty, int? minPrepTime, int? maxPrepTime, string? sortBy)
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null)
      {
        return Challenge();
      }

      var viewModel = new RecipeSearchViewModel
      {
        SearchTerm = searchTerm,
        Difficulty = difficulty,
        MinPrepTime = minPrepTime,
        MaxPrepTime = maxPrepTime,
        SortBy = sortBy
      };

      // Start with user's recipes only
      var query = _context.Recipes
          .Where(r => r.UserId == user.Id)
          .Include(r => r.User)
          .AsQueryable();

      // Apply search term filter
      if (!string.IsNullOrWhiteSpace(searchTerm))
      {
        searchTerm = searchTerm.ToLower();
        query = query.Where(r =>
            r.Title.ToLower().Contains(searchTerm) ||
            r.Description.ToLower().Contains(searchTerm) ||
            r.Ingredients.ToLower().Contains(searchTerm) ||
            r.Instructions.ToLower().Contains(searchTerm)
        );
      }

      // Apply difficulty filter
      if (!string.IsNullOrWhiteSpace(difficulty))
      {
        query = query.Where(r => r.Difficulty == difficulty);
      }

      // Apply time filters
      if (minPrepTime.HasValue)
      {
        query = query.Where(r => (r.PrepTimeMinutes + r.CookTimeMinutes) >= minPrepTime.Value);
      }
      if (maxPrepTime.HasValue)
      {
        query = query.Where(r => (r.PrepTimeMinutes + r.CookTimeMinutes) <= maxPrepTime.Value);
      }

      // Apply sorting
      viewModel.SortBy = sortBy;
      query = sortBy switch
      {
        "Oldest First" => query.OrderBy(r => r.CreatedAt),
        "Most Difficult" => query.OrderByDescending(r => r.Difficulty == "Hard")
            .ThenByDescending(r => r.Difficulty == "Medium")
            .ThenBy(r => r.Difficulty == "Easy"),
        "Least Difficult" => query.OrderBy(r => r.Difficulty == "Easy")
            .ThenBy(r => r.Difficulty == "Medium")
            .ThenByDescending(r => r.Difficulty == "Hard"),
        "Shortest Time" => query.OrderBy(r => r.PrepTimeMinutes + r.CookTimeMinutes),
        "Longest Time" => query.OrderByDescending(r => r.PrepTimeMinutes + r.CookTimeMinutes),
        _ => query.OrderByDescending(r => r.CreatedAt) // Newest First
      };

      viewModel.Recipes = await query.ToListAsync();

      return View(viewModel);
    }

    private bool RecipeExists(int id)
    {
      return _context.Recipes.Any(e => e.Id == id);
    }
  }
}