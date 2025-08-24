using Microsoft.EntityFrameworkCore;
using MyWebApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Register DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// ✅ Enable Swagger always (Dev + Prod)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // serve Swagger at root "/"
});

// Middlewares
app.UseHttpsRedirection();
app.UseAuthorization();

// ✅ Map controllers
app.MapControllers();

// ✅ Optional: Redirect root "/" → Swagger (if RoutePrefix is not empty)
// app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
