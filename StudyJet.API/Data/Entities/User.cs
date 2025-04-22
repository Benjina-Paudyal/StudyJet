using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudyJet.API.Data.Entities
{
    public class User : IdentityUser
    {
        // // Inherits basic identity properties (ID, Username, Email, PasswordHash) from IdentityUser


        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full Name must be between 2 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "Full Name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string? FullName { get; set; }

        [MaxLength(255)]
        public string? ProfilePictureUrl { get; set; }

        // Indicates whether the user needs to change their password on first login
        public bool NeedToChangePassword { get; set; } = false;

       
        [JsonIgnore]
        public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

        [JsonIgnore]
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();

        [JsonIgnore]
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<UserPurchaseCourse> UserCoursePurchases { get; set; } = new List<UserPurchaseCourse>();
      
        public ICollection<Course> CoursesTaught { get; set; } = new List<Course>(); 
        public ICollection<Course> CoursesEnrolled { get; set; } = new List<Course>(); 



    }
}
