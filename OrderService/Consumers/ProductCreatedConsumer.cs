using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using Shared.Messages;

// RabbitMQ consumer: On receiving a ProductCreated event, insert the local product record:
namespace OrderService.Consumers
{
	public class ProductCreatedConsumer : IConsumer<ProductCreated> // Subscribes to ProductCreated messages.
	{
		private readonly OrderDbContext _dbContext;

		public ProductCreatedConsumer(OrderDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task Consume(ConsumeContext<ProductCreated> context)
		{
			var message = context.Message;

			var existing = await _dbContext.Products.FindAsync(message.ProductId);
			if (existing != null) return;

			var product = new ProductReadModel
			{
				Id = message.ProductId,
				Name = message.Name,
				Quantity = message.Quantity,
				Price = message.Price
			};
			await _dbContext.AddRangeAsync(product);
			await _dbContext.SaveChangesAsync();
		}
	}
}
