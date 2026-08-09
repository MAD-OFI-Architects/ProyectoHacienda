using Hacienda.Application.Interfaces;
using Hacienda.Application.Services;
using Hacienda.Application.Validaciones;
using Hacienda.Domain.Factories;
using Hacienda.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Hacienda.Infrastructure.CrossCutting;
using Hacienda.Infrastructure.Events;
using Hacienda.Infrastructure.Persistence;
using Hacienda.Infrastructure.Persistence.Sqlite;
using Hacienda.Infrastructure.Policies;

var builder = WebApplication.CreateBuilder(args);

// ── MVC + Razor ──
builder.Services.AddControllersWithViews();

// ── Authentication ──
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "HaciendaSoft.Auth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddHttpContextAccessor();

// ── Domain Events ──
builder.Services.AddScoped<IDomainEventPublisher, DomainEventPublisherConsola>();

// ── Cross-cutting (Singleton) ──
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IGuidProvider, GuidProviderSistema>();
builder.Services.AddSingleton<IHasher, HasherBcrypt>();

// ── Factories (Transient) ──
builder.Services.AddTransient<IResFactory, FabricaRes>();
builder.Services.AddTransient<IVacunaFactory, FabricaVacuna>();
builder.Services.AddTransient<IVentaFactory, FabricaVenta>();
builder.Services.AddTransient<IPotreroFactory, FabricaPotrero>();

// ── Application Services (Scoped) ──
builder.Services.AddScoped<IGestorPotreros, GestorPotreros>();
builder.Services.AddScoped<IGestorReses, GestorReses>();
builder.Services.AddScoped<IServicioVacunacion, ServicioVacunacion>();
builder.Services.AddScoped<IServicioVentas, ServicioVentas>();
builder.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
builder.Services.AddScoped<IAutorizador, AutorizadorRbca>();
builder.Services.AddScoped<IServicioChip, ServicioChip>();
builder.Services.AddScoped<IServicioGeolocalizacion, ServicioGeolocalizacion>();

// ── Validation (Transient) ──
builder.Services.AddTransient<IValidarRes, ValidadorRes>();
builder.Services.AddTransient<IValidarPotrero, ValidadorPotrero>();
builder.Services.AddTransient<IValidarVacuna, ValidadorVacuna>();
builder.Services.AddTransient<IValidarVenta, ValidadorVenta>();

// ── Repositories (SQLite) ──
var directorioDatos = Path.Combine(builder.Environment.ContentRootPath, "Datos");
Directory.CreateDirectory(directorioDatos);
var connectionString = $"Data Source={Path.Combine(directorioDatos, "hacienda.db")}";

DatabaseInitializer.Initialize(connectionString);

builder.Services.AddScoped<IRepositorioPotrero>(sp =>
    new RepositorioPotreroSqlite(connectionString, sp.GetRequiredService<IGuidProvider>()));
builder.Services.AddScoped<IRepositorioRes>(sp =>
    new RepositorioResSqlite(connectionString, sp.GetRequiredService<IGuidProvider>()));
builder.Services.AddScoped<IRepositorioVacuna>(sp =>
    new RepositorioVacunaSqlite(connectionString, sp.GetRequiredService<IGuidProvider>()));
builder.Services.AddScoped<IRepositorioVenta>(sp =>
    new RepositorioVentaSqlite(connectionString, sp.GetRequiredService<IGuidProvider>()));
builder.Services.AddScoped<IRepositorioUsuario>(sp =>
    new RepositorioUsuarioSqlite(connectionString, sp.GetRequiredService<IGuidProvider>()));
builder.Services.AddScoped<IRepositorioChip>(sp =>
    new RepositorioChipSqlite(connectionString, sp.GetRequiredService<IGuidProvider>()));
builder.Services.AddScoped<IRepositorioGeolocalizacion>(sp =>
    new RepositorioGeolocalizacionSqlite(connectionString));

// ── Authorization Policies (Transient - Plugin Registry) ──
builder.Services.AddTransient<IPoliticaPermisos, PoliticaAdmin>();
builder.Services.AddTransient<IPoliticaPermisos, PoliticaEmpleado>();
builder.Services.AddTransient<IPoliticaPermisos, PoliticaVisitante>();

// ── Data Seeder ──
builder.Services.AddScoped<IDataSeeder>(sp =>
    new DataLoader(
        sp.GetRequiredService<IRepositorioUsuario>(),
        sp.GetRequiredService<IRepositorioPotrero>(),
        sp.GetRequiredService<IGuidProvider>(),
        sp.GetRequiredService<IHasher>(),
        connectionString));

var app = builder.Build();

// ── Cargar datos iniciales ──
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.CargarDatosAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "login",
    pattern: "",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "login-post",
    pattern: "",
    defaults: new { controller = "Account", action = "Login" },
    constraints: new { httpMethod = "POST" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();