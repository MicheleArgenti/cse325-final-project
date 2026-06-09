namespace RecipeManagement.Models
{
  public class CategoryFilterViewModel
  {
    public List<Category> AllCategories { get; set; } = new();
    public int[] SelectedCategoryIds { get; set; } = new int[0];
    public List<Recipe> Recipes { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? Difficulty { get; set; }
    public int? MinTime { get; set; }
    public int? MaxTime { get; set; }
    public bool MatchAllCategories { get; set; } = false;
  }
}