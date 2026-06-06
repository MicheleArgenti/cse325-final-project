using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeManagement.Data;
using RecipeManagement.Models;
using System.Diagnostics;

namespace RecipeManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            // Get recent recipes (last 6)
            viewModel.RecentRecipes = await _context.Recipes
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(6)
                .ToListAsync();

            // Get popular recipes - using a different approach (most recent or most viewed)
            // Since we don't have a view count, we'll use the most recent ones after skipping the first 6
            viewModel.PopularRecipes = await _context.Recipes
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(6)
                .Take(4)
                .ToListAsync();

            // If there aren't enough recipes, get some from the beginning
            if (viewModel.PopularRecipes.Count < 4)
            {
                var additionalRecipes = await _context.Recipes
                    .Include(r => r.User)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(4)
                    .ToListAsync();

                foreach (var recipe in additionalRecipes)
                {
                    if (!viewModel.PopularRecipes.Contains(recipe) && viewModel.PopularRecipes.Count < 4)
                    {
                        viewModel.PopularRecipes.Add(recipe);
                    }
                }
            }

            // Get total recipe count
            viewModel.TotalRecipes = await _context.Recipes.CountAsync();

            // Get total users
            viewModel.TotalUsers = await _userManager.Users.CountAsync();

            // If user is logged in, get their info
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    viewModel.UserName = $"{user.FirstName} {user.LastName}".Trim();
                    if (string.IsNullOrEmpty(viewModel.UserName))
                    {
                        viewModel.UserName = user.UserName ?? user.Email;
                    }
                    viewModel.UserRecipesCount = await _context.Recipes
                        .CountAsync(r => r.UserId == user.Id);
                }
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View();
        }
    }
}