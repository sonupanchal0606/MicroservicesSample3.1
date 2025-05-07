using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using Shared.Messages;

namespace OrderService.Consumers
{
	public class ProductUpdatedConsumer : IConsumer<ProductUpdated>
	{
		private readonly OrderDbContext _context;

		public ProductUpdatedConsumer(OrderDbContext context)
		{
			_context = context;
		}

		public async Task Consume(ConsumeContext<ProductUpdated> context)
		{
			var message = context.Message;

			var product = await _context.Products.FindAsync(message.ProductId);
			if (product == null) return;

			product.Name = message.Name;
			product.Price = message.Price;
			product.Quantity = message.Quantity;

			await _context.SaveChangesAsync();
		}
	}
}
