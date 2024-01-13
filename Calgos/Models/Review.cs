using System.ComponentModel.DataAnnotations;

namespace Calgos.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }


        [Required(ErrorMessage = "Continutul este obligatoriu")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Rating-ul este obligatoriu")]
        public int Points { get; set; }

        public DateTime Date { get; set; }

        // un review apartine unui produs
        public int? ProductId { get; set; }

        // un review este postat de catre un user
        public string? UserId { get; set; }

       
        public virtual ApplicationUser? User { get; set; }

        public virtual Product? Product { get; set; }
    }

}
