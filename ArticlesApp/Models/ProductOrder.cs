using System.ComponentModel.DataAnnotations.Schema;

namespace ArticlesApp.Models
{

    public class ProductOrder
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        // cheie primara compusa (Id, ArticleId, BookmarkId)
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public int? OrderId { get; set; }

        public virtual Product? Product { get; set; }
        public virtual Order? Order { get; set; }

    }
}
