using System;
using System.Collections.Generic;
using System.Text;
using Warehouse.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Warehouse.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
        public class ProductContext
        {
            public static List<Product> ProductsList { get; set; }

            public static void InitData(int productCount)
            {
                ProductsList = CreateProductList(productCount);
            }

            private static List<Product> CreateProductList(int productCount)
            {
                List<Product> productList = new List<Product>();
                for (int i = 1; i < productCount + 1; i++)
                {
                    productList.Add(
                        new Product()
                        {
                            ProductId = i,
                            Product_Name = "Product_Name" + i,
                        }
                    );
                }
                return productList;
            }
        }
        public DbSet<Product> Products { get; set; }
		public DbSet<Order> Order { get; set; }
		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<ProductType>().HasKey(t => t.TypeId);
			builder.Entity<Product>().HasKey(p => p.ProductId);
			builder.Entity<Product>().HasOne(p => p.ProductType).WithMany(t => t.Products).HasForeignKey(p => p.TypeId);
		}
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}
		
	}
}
