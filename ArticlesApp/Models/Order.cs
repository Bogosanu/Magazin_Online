using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace ArticlesApp.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }
        public string? UserId { get; set; }
        public string adresa { get; set; }
        public string metodaLivrare { get; set; }
        public string metodaPlata { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // relatia many-to-many dintre Product si Order
        public virtual ICollection<ProductOrder>? ProductOrders { get; set; }

    }
}
