using System.ComponentModel.DataAnnotations;

namespace StudyJet.API.Data.Entities
{
    public class Notification
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public int? CourseID { get; set; }

        public virtual User User { get; set; }

        public virtual Course Course { get; set; }
    }
}
