using AvionesDistribuidos.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<SqlServerService>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


// Agregar el DbContext usando la cadena de conexión
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var continent = httpContext?.Request.Query["continent"].ToString();
    if (string.IsNullOrEmpty(continent))
    {
        continent = "Default";  // Asignar un valor por defecto si no se pasa el parámetro
    }
    var connectionString = (continent == "Europe" || continent == "Africa")
        ? builder.Configuration.GetConnectionString("SQLServer_EuropaAfrica")
        : builder.Configuration.GetConnectionString("SQLServer_Americas");
    options.UseSqlServer(connectionString);
});


builder.Services.AddDbContext<ApplicationDbContextEurope>((serviceProvider, options) =>
{
    var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
    var continent = httpContext?.Request.Query["continent"].ToString();
    if (string.IsNullOrEmpty(continent))
    {
        continent = "Default";  // Asignar un valor por defecto si no se pasa el parámetro
    }
    var connectionString = (continent == "Europe" || continent == "Africa")
        ? builder.Configuration.GetConnectionString("SQLServer_EuropaAfrica")
        : builder.Configuration.GetConnectionString("SQLServer_Americas");
    options.UseSqlServer(connectionString);
});


builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<MongoDbService>();


builder.Services.AddScoped<ConnectionResolver>();

// Agregar servicios a la aplicación.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Resto de la configuración...
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
