using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calgos.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 100 de caractere")]
        [MinLength(5, ErrorMessage = "Titlul trebuie sa aiba mai mult de 5 caractere")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Continutul produsului este obligatoriu")]
        public string Description { get; set; }

        public int pret { get; set; }

        public double Rating { get; set; }

        public DateTime Date { get; set; }

        public string? Image { get; set; }


        [Required(ErrorMessage = "Categoria este obligatorie")]
 
        public int? CategoryId { get; set; }

       
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Category? Category { get; set; }

       
        public virtual ICollection<Review>? Reviews { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }

        // relatia many-to-many dintre Product si Basket
        public virtual ICollection<ProductBasket>? ProductBaskets { get; set; }
        public virtual ICollection<ProductOrder>? ProductOrders { get; set; }
    }

}


