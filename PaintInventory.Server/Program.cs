using Microsoft.EntityFrameworkCore;
using PaintInventory.Server.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=PaintInventoryDb;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<PaintInventoryDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Ensure database created and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaintInventoryDbContext>();
    db.Database.Migrate();

    // Seed if empty
    if (!db.PaintItems.Any())
    {
        db.PaintItems.Add(new PaintInventory.Server.Models.PaintItem { Barcode = "7029350108807", SKU = "J001", Name = "Sample Red Paint", ColorCode = "R-100", Volume = 1.0m, Unit = "L", Manufacturer = "Acme" });
        db.PaintItems.Add(new PaintInventory.Server.Models.PaintItem { Barcode = "3372689-1-+-1:2", SKU = "J002", Name = "Sample White Paint", ColorCode = "W-200", Volume = 0.5m, Unit = "L", Manufacturer = "Acme" });
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.UseDefaultFiles();
app.MapStaticAssets();

app.MapFallbackToFile("/index.html");

app.Run();
