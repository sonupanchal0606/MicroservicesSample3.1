using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using Shared.Messages;

namespace ProductService.Consumers
{
	public class OrderUpdatedConsumer : IConsumer<OrderUpdated>
	{
		private readonly ProductDbContext _context;

		public OrderUpdatedConsumer(ProductDbContext context)
		{
			_context = context;
		}

		public async Task Consume(ConsumeContext<OrderUpdated> context)
		{
			var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == context.Message.ProductId);
			if (product != null)
			{
				product.Quantity -= context.Message.QuantityDifference;
				await _context.SaveChangesAsync();
			}
		}
	}

}
