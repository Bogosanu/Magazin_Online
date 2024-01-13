using Calgos.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Migrations;




namespace Calgos.Data
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
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductOrder> ProductOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           

            base.OnModelCreating(modelBuilder);

            // definire primary key compus
            modelBuilder.Entity<ProductBasket>()
                .HasKey(ab => new { ab.Id, ab.ProductId, ab.BasketId });

            modelBuilder.Entity<ProductOrder>()
                .HasKey(ab => new { ab.Id, ab.ProductId, ab.OrderId });


            // definire relatii cu modelele Basket si Product (FK)

            modelBuilder.Entity<ProductBasket>()
                .HasOne(ab => ab.Product)
                .WithMany (ab => ab.ProductBaskets)
                .HasForeignKey(ab => ab.ProductId);

            modelBuilder.Entity<ProductBasket>()
                .HasOne(ab => ab.Basket)
                .WithMany(ab => ab.ProductBaskets)
                .HasForeignKey(ab => ab.BasketId);


            modelBuilder.Entity<ProductOrder>()
                .HasOne(ab => ab.Product)
                .WithMany(ab => ab.ProductOrders)
                .HasForeignKey(ab => ab.ProductId);

            modelBuilder.Entity<ProductOrder>()
                .HasOne(ab => ab.Order)
                .WithMany(ab => ab.ProductOrders)
                .HasForeignKey(ab => ab.OrderId);
        }
    }
}