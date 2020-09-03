using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Warehouse.Models
{
	public class SaleViewModel
	{
		public int OrderId { get; set; }
		public int ProductId { get; set; }
		public int Amount { get; set; }
	}
}
