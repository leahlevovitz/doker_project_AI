var builder = WebApplication.CreateBuilder(args);

var catalogUrl = builder.Configuration.GetValue<string>("Services:ProductCatalog") ?? "http://product-catalog-1:8080";
var orderUrl = builder.Configuration.GetValue<string>("Services:Order") ?? "http://order:8080";

builder.Services.AddHttpClient("catalog", c => c.BaseAddress = new Uri(catalogUrl));
builder.Services.AddHttpClient("order", c => c.BaseAddress = new Uri(orderUrl));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
