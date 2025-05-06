namespace Shared.Messages
{
	// Reference this project in both ProductService and OrderService.
	public class ProductCreated
	{
		public Guid ProductId { get; set; }
		public string Name { get; set; } = string.Empty;
		public int Quantity { get; set; }
		public decimal Price { get; set; }
	}
}
