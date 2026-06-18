using Microsoft.AspNetCore.Identity;

namespace RecipeManagement.Models
{
  public class ApplicationUser : IdentityUser
  {
    [PersonalData]
    public string? FirstName { get; set; }

    [PersonalData]
    public string? LastName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Recipe>? Recipes { get; set; }

    // Add this property for favorites
    // public virtual ICollection<Favorite>? Favorites { get; set; }

    // Display name property
    public string DisplayName
    {
      get
      {
        if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
          return $"{FirstName} {LastName}";
        if (!string.IsNullOrEmpty(FirstName))
          return FirstName;
        if (!string.IsNullOrEmpty(LastName))
          return LastName;
        return UserName ?? Email ?? "Unknown User";
      }
    }
  }
}