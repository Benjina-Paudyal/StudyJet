using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using StudyJet.API.Data.Enums;

namespace StudyJet.API.Data.Entities
{
    public class CourseUpdate
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int CourseID { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }


        [Column(TypeName = "decimal(10, 2)")]
        public decimal? Price { get; set; }

        public string? VideoUrl { get; set; }

        public CourseStatus Status { get; set; } = CourseStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public Course Course { get; set; }

    }
}
