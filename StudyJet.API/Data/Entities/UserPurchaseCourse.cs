using System.ComponentModel.DataAnnotations;

namespace StudyJet.API.Data.Entities
{
    public class UserPurchaseCourse
    {
        public int ID { get; set; }

        [Required]
        public string UserID { get; set; }


        [Required]
        public string UserName { get; set; }

        [Required]
        public int CourseID { get; set; }

        public DateTime PurchaseDate { get; set; }

        public User User { get; set; }

        public Course Course { get; set; }
    }
}
