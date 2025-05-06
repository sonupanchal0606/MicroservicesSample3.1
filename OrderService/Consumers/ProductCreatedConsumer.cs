using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using Shared.Messages;

// RabbitMQ consumer: On receiving a ProductCreated or ProductUpdated event, insert or update the local product record:
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

			var existingProduct = await _dbContext.Products
				.FirstOrDefaultAsync(p => p.Id == message.ProductId);

			if (existingProduct == null)
			{
				_dbContext.Products.Add(new Product
				{
					Id = message.ProductId,
					Name = message.Name,
					Quantity = message.Quantity,
					Price = message.Price
				});
			}
			else
			{
				existingProduct.Name = message.Name;
				existingProduct.Price = message.Price;
				existingProduct.Quantity = message.Quantity;
			}

			await _dbContext.SaveChangesAsync();
		}
	}
}
