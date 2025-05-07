using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Consumers;
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
		public async Task<IActionResult> Create(ProductCreateDto input)
		{
			var product = new Product
			{
				Id = Guid.NewGuid(),
				Name = input.Name,
				Quantity = input.Quantity,
				Price = input.Price
			};

			_context.Products.Add(product);
			await _context.SaveChangesAsync();

			await _publishEndpoint.Publish(new ProductCreated
			{
				ProductId = product.Id,
				Name = product.Name,
				Price = product.Price,
				Quantity = product.Quantity
			});

			return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
		}


		[HttpPut("{id}")]
		public async Task<IActionResult> Update(Guid id, ProductUpdateDto updated)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null) return NotFound();

			product.Name = string.IsNullOrWhiteSpace(updated.Name) ? product.Name : updated.Name;
			product.Quantity = updated.Quantity.HasValue ? (int)updated.Quantity : product.Quantity;
			product.Price = updated.Price.HasValue ? (int)updated.Price : product.Price;

			await _context.SaveChangesAsync();

			await _publishEndpoint.Publish(new ProductUpdated
			{
				ProductId = product.Id,
				Name = product.Name,
				Quantity = product.Quantity,
				Price = product.Price
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

			await _publishEndpoint.Publish(new ProductDeleted
			{
				ProductId = product.Id
			});

			return NoContent();
		}
	}
}
