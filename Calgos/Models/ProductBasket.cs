using System.ComponentModel.DataAnnotations.Schema;

namespace Calgos.Models
{

    public class ProductBasket
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        // cheie primara compusa (Id, ArticleId, BookmarkId)
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public int? BasketId { get; set; }

        public virtual Product? Product { get; set; }
        public virtual Basket? Basket { get; set; }

    }
}
