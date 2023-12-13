using ArticlesApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;


// PASUL 3 - useri si roluri

namespace ArticlesApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<ProductBasket> ProductBaskets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           

            base.OnModelCreating(modelBuilder);

            // definire primary key compus
            modelBuilder.Entity<ProductBasket>()
                .HasKey(ab => new { ab.Id, ab.ProductId, ab.BasketId });


            // definire relatii cu modelele Basket si Product (FK)

            modelBuilder.Entity<ProductBasket>()
                .HasOne(ab => ab.Product)
                .WithMany (ab => ab.ProductBaskets)
                .HasForeignKey(ab => ab.ProductId);

            modelBuilder.Entity<ProductBasket>()
                .HasOne(ab => ab.Basket)
                .WithMany(ab => ab.ProductBaskets)
                .HasForeignKey(ab => ab.BasketId);
        }
    }
}