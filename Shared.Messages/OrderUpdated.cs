using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messages
{
	public class OrderUpdated
	{
		public Guid ProductId { get; set; }
		public int QuantityDifference { get; set; } // newQuantity - oldQuantity
	}
}
