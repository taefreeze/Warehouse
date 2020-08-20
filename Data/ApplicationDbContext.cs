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
