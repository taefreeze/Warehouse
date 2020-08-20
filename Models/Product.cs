using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Warehouse.Models
{
	public class Product
	{
		public int ProductId { get; set; }
		[DisplayName("ชื่อสินค้า")]
		public string Product_Name { get; set; }
		[DisplayName("รหัสชนิดสินค้า")]
		public int TypeId { get; set; }
		[DisplayName("ชนิดสินค้า")]
		public ProductType ProductType { get; set; }
		[DisplayName("ราคาสินค้า")]
		public decimal Price { get; set; }
		[DisplayName("จำนวนคงเหลือ")]
		public int Quantity_P { get; set; }
	}
}
