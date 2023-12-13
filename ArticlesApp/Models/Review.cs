using System.ComponentModel.DataAnnotations;

namespace ArticlesApp.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        public string Content { get; set; }

        public int Points { get; set; }

        public DateTime Date { get; set; }

        // un review apartine unui produs
        public int? ProductId { get; set; }

        // un review este postat de catre un user
        public string? UserId { get; set; }

        // PASUL 6 - useri si roluri
        public virtual ApplicationUser? User { get; set; }

        public virtual Product? Product { get; set; }
    }

}
