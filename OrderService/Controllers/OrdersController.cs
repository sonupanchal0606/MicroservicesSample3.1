using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using Shared.Messages;

namespace OrderService.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class OrdersController : ControllerBase
	{
		private readonly OrderDbContext _context;
		private readonly IPublishEndpoint _publishEndpoint;

		public OrdersController(OrderDbContext context, IPublishEndpoint publishEndpoint)
		{
			_context = context;
			_publishEndpoint = publishEndpoint;
		}

		[HttpGet]
		//  This gives you product info per order without actual EF navigation or foreign key.
		public async Task<IActionResult> GetOrders()
		{
			var orders = await _context.Orders.ToListAsync();
			var productIds = orders.Select(o => o.ProductId).ToList();
			var products = await _context.Products
				.Where(p => productIds.Contains(p.Id))
				.ToDictionaryAsync(p => p.Id);

			var response = orders.Select(order => new
			{
				order.Id,
				order.ProductId,
				order.Quantity,
				order.TotalPrice,
				Product = products.ContainsKey(order.ProductId)
					? new
					{
						products[order.ProductId].Id,
						products[order.ProductId].Name,
						products[order.ProductId].Price,
						products[order.ProductId].Quantity
					}
					: null
			});

			return Ok(response);
		}

		[HttpGet("getProducts")]
		public async Task<IActionResult> GetProducts()
		{
			var orders = await _context.Products.ToListAsync();
			return Ok(orders);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			var order = await _context.Orders.FindAsync(id);
			if (order == null) return NotFound();
			var product = await _context.Products.FindAsync(order.ProductId);

			return Ok(new
			{
				order.Id,
				order.ProductId,
				order.Quantity,
				order.TotalPrice,
				Product = product
			});
		}

		[HttpPost]
		public async Task<IActionResult> CreateOrder([FromBody] Order orderRequest)
		{
			var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == orderRequest.ProductId);
			if (product == null || product.Quantity < orderRequest.Quantity)
				return BadRequest("ProductReadModel not available or insufficient quantity.");

			var order = new Order
			{
				Id = Guid.NewGuid(),
				ProductId = orderRequest.ProductId,
				Quantity = orderRequest.Quantity,
				TotalPrice = product.Price * orderRequest.Quantity
			};

			// Local product quantity update (optional)
			product.Quantity -= order.Quantity;

			_context.Orders.Add(order);
			await _context.SaveChangesAsync();

			// Publish event to reduce product quantity in ProductService
			var message = new OrderCreated
			{
				ProductId = order.ProductId,
				Quantity = order.Quantity
			};
			await _publishEndpoint.Publish(message);

			return Ok(order);
		}	

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] Order updatedOrder)
		{
			var existingOrder = await _context.Orders.FindAsync(id);
			if (existingOrder == null)
				return NotFound("Order not found");

			var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == updatedOrder.ProductId);
			if (product == null)
				return BadRequest("ProductReadModel not found");

			int quantityDiff = updatedOrder.Quantity - existingOrder.Quantity;
			if (product.Quantity < quantityDiff)
				return BadRequest("Not enough stock to increase order quantity.");

			product.Quantity -= quantityDiff;

			existingOrder.ProductId = updatedOrder.ProductId;
			existingOrder.Quantity = updatedOrder.Quantity;
			existingOrder.TotalPrice = product.Price * updatedOrder.Quantity;

			await _context.SaveChangesAsync();

			// Publish OrderUpdated event
			var message = new OrderUpdated
			{
				ProductId = updatedOrder.ProductId,
				QuantityDifference = quantityDiff
			};
			await _publishEndpoint.Publish(message);

			return Ok(existingOrder);
		}

		// Publish This Event on Failure or Order Cancellation
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteOrder(Guid id)
		{
			var existingOrder = await _context.Orders.FindAsync(id);
			if (existingOrder == null)
				return NotFound("Order not found");

			var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == existingOrder.ProductId);
			if (product != null)
				product.Quantity += existingOrder.Quantity;

			_context.Orders.Remove(existingOrder);
			await _context.SaveChangesAsync();

			// Publish OrderDeleted event
			var message = new OrderDeleted
			{
				ProductId = existingOrder.ProductId,
				Quantity = existingOrder.Quantity
			};
			await _publishEndpoint.Publish(message);

			return NoContent();
		}

	}
}
