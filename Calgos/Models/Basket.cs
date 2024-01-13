using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace Calgos.Models
{
    public class Basket
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // relatia many-to-many dintre Product si Basket
        public virtual ICollection<ProductBasket>? ProductBaskets { get; set; }

    }
}
