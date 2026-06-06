namespace RecipeManagement.Models
{
  public class HomeViewModel
  {
    public List<Recipe> RecentRecipes { get; set; } = new();
    public List<Recipe> PopularRecipes { get; set; } = new();
    public int TotalRecipes { get; set; }
    public int TotalUsers { get; set; }
    public string? UserName { get; set; }
    public int UserRecipesCount { get; set; }
  }
}