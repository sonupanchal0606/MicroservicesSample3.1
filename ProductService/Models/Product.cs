namespace ProductService.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
	public class ProductCreateDto
	{
		public string Name { get; set; } = string.Empty;
		public int Quantity { get; set; }
		public decimal Price { get; set; }
	}

	public class ProductUpdateDto
	{
		public string? Name { get; set; }
		public int? Quantity { get; set; }
		public decimal? Price { get; set; }
	}

}
