using Microsoft.AspNetCore.Identity;

namespace RecipeManagement.Models
{
  public class ApplicationUser : IdentityUser
  {
    [PersonalData]
    public string? FirstName { get; set; }

    [PersonalData]
    public string? LastName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<Recipe>? Recipes { get; set; }
  }
}