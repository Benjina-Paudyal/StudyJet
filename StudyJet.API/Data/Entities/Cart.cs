using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyJet.API.Data.Entities
{
    public class Cart
    {
        [Key]
        public int CartID { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        public int CourseID { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalPrice { get; set; }
    
        public User User { get; set; }
        public Course Course { get; set; }

    }
}
