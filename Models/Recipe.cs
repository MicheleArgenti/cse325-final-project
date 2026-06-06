using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeManagement.Models
{
    public class Recipe
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Ingredients { get; set; } = string.Empty;

        [Required]
        public string Instructions { get; set; } = string.Empty;

        public int PrepTimeMinutes { get; set; }

        public int CookTimeMinutes { get; set; }

        [StringLength(50)]
        public string Difficulty { get; set; } = "Medium";

        public int Servings { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Foreign key to user
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}