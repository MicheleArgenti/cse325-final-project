using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeManagement.Models
{
  public class RecipeCategory
  {
    public int RecipeId { get; set; }
    public int CategoryId { get; set; }

    // Navigation properties
    [ForeignKey("RecipeId")]
    public virtual Recipe? Recipe { get; set; }

    [ForeignKey("CategoryId")]
    public virtual Category? Category { get; set; }
  }
}