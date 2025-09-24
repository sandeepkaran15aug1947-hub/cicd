using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using MyWebApi.Data;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Register DbContext with SQL Server


if (builder.Environment.IsProduction())
{
    var keyVaultURL = builder.Configuration.GetSection("KeyVault:KeyVaultURL");
    var keyVaultClientId = builder.Configuration.GetSection("KeyVault:ClientId");
    var keyVaultClientSecret = builder.Configuration.GetSection("KeyVault:ClientSecret");
    var keyVaultDirectoryID = builder.Configuration.GetSection("KeyVault:DirectoryID");
    var credential = new ClientSecretCredential(keyVaultDirectoryID.Value!.ToString(), keyVaultClientId.Value!.ToString(), keyVaultClientSecret.Value!.ToString());
    builder.Configuration.AddAzureKeyVault(keyVaultURL.Value!.ToString(), keyVaultClientId.Value!.ToString(), keyVaultClientSecret.Value!.ToString(), new DefaultKeyVaultSecretManager());
    var client = new SecretClient(new Uri(keyVaultURL.Value!.ToString()), credential);
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(client.GetSecret("ProdConnection").Value.Value.ToString());
    });
}

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
}


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
