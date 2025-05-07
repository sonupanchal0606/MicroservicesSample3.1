using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Consumers;
using OrderService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<OrderDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Subscribes to ProductCreated messages.
// Consumes them via a MassTransit consumer to sync or react to the new product.
builder.Services.AddMassTransit(x =>
{
	x.AddConsumer<ProductCreatedConsumer>();
	x.AddConsumer<ProductUpdatedConsumer>();
	x.AddConsumer<ProductDeletedConsumer>();

	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
		{
			h.Username(builder.Configuration["RabbitMq:Username"]);
			h.Password(builder.Configuration["RabbitMq:Password"]);
		});

		cfg.ReceiveEndpoint("product-created-event", e =>
		{
			e.ConfigureConsumer<ProductCreatedConsumer>(context);
		});

		cfg.ReceiveEndpoint("product-updated-event", e =>
			e.ConfigureConsumer<ProductUpdatedConsumer>(context));

		cfg.ReceiveEndpoint("product-deleted-event", e =>
			e.ConfigureConsumer<ProductDeletedConsumer>(context));
	});
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
