using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyJet.API.Data.Entities
{
    public class Transaction
    {
        [Key]
        public int TransactionID { get; set; }

        [Required]
        public string UserID { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount {  get; set; }

        public User User { get; set; }
    }
}
