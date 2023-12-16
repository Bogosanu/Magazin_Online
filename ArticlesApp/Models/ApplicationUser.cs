using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;


// PASUL 1 - useri si roluri 

namespace ArticlesApp.Models 
{
    public class ApplicationUser : IdentityUser
    {
        // un user poate posta mai multe reviewuri
        public virtual ICollection<Review>? Reviews { get; set; }

        // un user poate posta mai multe articole
        public virtual ICollection<Product>? Products { get; set; }

        // un user poate sa creeze mai multe cosuri?(schimbam)
        public virtual Basket Baskets { get; set; }

        // atribute suplimentare adaugate pentru user
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        // variabila in care vom retine rolurile existente in baza de date
        // pentru popularea unui dropdown list
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }

    }
}
