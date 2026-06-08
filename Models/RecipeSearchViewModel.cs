namespace RecipeManagement.Models
{
  public class RecipeSearchViewModel
  {
    public string? SearchTerm { get; set; }
    public string? Difficulty { get; set; }
    public int? MinPrepTime { get; set; }
    public int? MaxPrepTime { get; set; }
    public string? SortBy { get; set; }
    public List<Recipe> Recipes { get; set; } = new();

    // For filter options
    public List<string> Difficulties { get; set; } = new() { "Easy", "Medium", "Hard" };
    public List<string> SortOptions { get; set; } = new()
        {
            "Newest First",
            "Oldest First",
            "Most Difficult",
            "Least Difficult",
            "Shortest Time",
            "Longest Time"
        };
  }
}