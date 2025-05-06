using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Events;
using ProductService.Models;
using Shared.Messages;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class ProductsController : ControllerBase
	{
		private readonly ProductDbContext _context;
		private readonly IPublishEndpoint _publishEndpoint;

		public ProductsController(ProductDbContext context, IPublishEndpoint publishEndpoint)
		{
			_context = context;
			_publishEndpoint = publishEndpoint;
		}

		[HttpGet]
		public async Task<IActionResult> Get() => Ok(await _context.Products.ToListAsync());

		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id) =>
			Ok(await _context.Products.FindAsync(id));

		[HttpPost]
		public async Task<IActionResult> Create(Product product)
		{
			product.Id = Guid.NewGuid();
			_context.Products.Add(product);
			await _context.SaveChangesAsync();

			await _publishEndpoint.Publish(new ProductCreated
			{
				ProductId = product.Id,
				Name = product.Name,
				Price = product.Price,
				Quantity = product.Quantity
			});

			return Ok(product);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(Guid id, Product updated)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null) return NotFound();

			product.Name = updated.Name;
			product.Quantity = updated.Quantity;
			product.Price = updated.Price;
			await _context.SaveChangesAsync();

			await _publishEndpoint.Publish(new ProductCreated
			{
				ProductId = product.Id,
				Name = product.Name,
				Price = product.Price,
				Quantity = product.Quantity
			});

			return Ok(product);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null) return NotFound();
			_context.Products.Remove(product);
			await _context.SaveChangesAsync();
			return NoContent();
		}
	}
}
