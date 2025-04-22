using StudyJet.API.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace StudyJet.API.Data.Entities
{
    public class Course
    {
        [Key]
        public int CourseID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }


        public string Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string ImageUrl { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public string InstructorID { get; set; }
        public User Instructor { get; set; }

        [Required]
        public int CategoryID { get; set; }
        public Category Category { get; set; }

        [Required]
        public DateTime CreationDate { get; set; }

        [Required]
        public DateTime LastUpdatedDate { get; set; }

        [MaxLength(255)]
        [Required]
        public string VideoUrl { get; set; }

        public bool IsPopular { get; set; } = false;

        public CourseStatus Status { get; set; }

        private bool _isApproved;

        [NotMapped]
        public bool IsApproved
        {
            get => Status == CourseStatus.Approved;
            set => _isApproved = value;
        }

        public bool IsArchived { get; set; }


        [JsonIgnore]
        public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

        [JsonIgnore]
        public ICollection<UserPurchaseCourse> UserPurchaseCourses { get; set; } = new List<UserPurchaseCourse>();


        [JsonIgnore]
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();

        [JsonIgnore]
        public ICollection<User> EnrolledStudents { get; set; } = new List<User>();

        public ICollection<CourseUpdate> CourseUpdates { get; set; } = new List<CourseUpdate>();
        public DateTime? LastApprovalDate { get; set; }

    }
}
