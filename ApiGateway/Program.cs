// Bellow code is without swagger
/*using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();
await app.UseOcelot();
app.Run();*/


using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot config
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Required for SwaggerGen internal services
builder.Services.AddEndpointsApiExplorer(); // <-- Don't forget this!
builder.Services.AddSwaggerGen(); // <-- Must be present before SwaggerForOcelot
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

// Configure SwaggerForOcelot UI
/*app.UseSwaggerForOcelotUI(opt =>
{
	opt.PathToSwaggerGenerator = "/swagger/docs";
	// Do NOT set opt.RoutePrefix = ""; in latest versions
	opt.DocumentTitle = "API Gateway - Swagger UI";
});*/

// Configure Swagger UI
app.UseSwaggerForOcelotUI(options =>
{
	options.PathToSwaggerGenerator = "/swagger/docs";
});


await app.UseOcelot();
app.Run();

