using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFwarehouse.Models
{
	public class SummaryOrderViewModel
	{
		public int OrderId { get; set; }
		public decimal Sum { get; set; }
	}
}
