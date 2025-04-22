using System.ComponentModel.DataAnnotations;

namespace StudyJet.API.Data.Entities
{
    public class Wishlist
    {
        [Key]
        public int WishlistID { get; set; } 

        [Required]
        public string UserID { get; set; }

        [Required]
        public int CourseID { get; set; }

        public User User { get; set; }
        public Course Course { get; set; }
    }
}
