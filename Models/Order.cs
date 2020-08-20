using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Warehouse.Models
{
	public class Order
	{
		[DisplayName("เลขออเดอร์")]
		public int OrderId { get; set; }
		[DisplayName("รหัสสินคัา")]
		public int ProductId { get; set; }
		public Product Product { get; set; }
		[DisplayName("วันที่")]
		public DateTime Date { get; set; }
		[DisplayName("ราคาสินค้า")]
		public decimal Price { get; set; }
		[DisplayName("จำนวนที่ขาย")]
		public int Quantity_O { get; set; }
		[DisplayName("ราคารวม")]
		public decimal Total_Price { get; set; }
	}
}
