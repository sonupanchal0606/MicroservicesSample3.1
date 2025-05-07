using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using Shared.Messages;

namespace OrderService.Consumers
{
	public class ProductDeletedConsumer : IConsumer<ProductDeleted>
	{
		private readonly OrderDbContext _context;

		public ProductDeletedConsumer(OrderDbContext context)
		{
			_context = context;
		}

		public async Task Consume(ConsumeContext<ProductDeleted> context)
		{
			var message = context.Message;

			var product = await _context.Products.FindAsync(message.ProductId);
			if (product == null) return;

			_context.Products.Remove(product);
			await _context.SaveChangesAsync();
		}
	}
}
