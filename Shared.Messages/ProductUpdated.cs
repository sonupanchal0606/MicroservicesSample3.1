using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messages
{
	public class ProductUpdated
	{
		public Guid ProductId { get; set; }
		public string Name { get; set; } = string.Empty;
		public int Quantity { get; set; }
		public decimal Price { get; set; }
	}
}
