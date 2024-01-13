using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;


// PASUL 1 - useri si roluri 

namespace Calgos.Models 
{
    public class ApplicationUser : IdentityUser
    {
        // un user poate posta mai multe reviewuri
        public virtual ICollection<Review>? Reviews { get; set; }

        // un user poate posta mai multe articole
        public virtual ICollection<Product>? Products { get; set; }

        [BindNever]
        public virtual Basket Basket { get; set; }

        // atribute suplimentare adaugate pentru user
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        // variabila in care vom retine rolurile existente in baza de date
        // pentru popularea unui dropdown list
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }

    }
}
