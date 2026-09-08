using Hacienda.Application.Interfaces;
using Hacienda.Application.Services;
using Hacienda.Domain.Builders;
using Hacienda.Domain.Factories;
using Hacienda.Domain.Factories.Reses;
using Hacienda.Domain.Factories.Vacunas;
using Hacienda.Domain.Factories.Productos;
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
builder.Services.AddScoped<IDomainEventPublisher, DespachadorDeEventos>();

// ── Cross-cutting (Singleton) ──
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IGuidProvider, GuidProviderSistema>();
builder.Services.AddSingleton<IHasher, HasherBcrypt>();

// ── Application Services (Scoped) ──
builder.Services.AddScoped<IGestorPotreros, GestorPotreros>();
builder.Services.AddScoped<IGestorReses, GestorReses>();
builder.Services.AddScoped<IServicioVacunacion, ServicioVacunacion>();
builder.Services.AddScoped<IServicioVentas, ServicioVentas>();
builder.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();
builder.Services.AddScoped<IAutorizador, AutorizadorRbca>();
builder.Services.AddScoped<IResLocator, ResLocator>();
builder.Services.AddScoped<IInstaladorChip, InstaladorChip>();
builder.Services.AddScoped<IServicioChip, ServicioChip>();
builder.Services.AddScoped<IServicioGeolocalizacion, ServicioGeolocalizacion>();

// ── Repositories (SQLite) ──
var directorioDatos = Path.Combine(builder.Environment.ContentRootPath, "Datos");
Directory.CreateDirectory(directorioDatos);
var connectionString = $"Data Source={Path.Combine(directorioDatos, "hacienda.db")}";

DatabaseInitializer.Initialize(connectionString);

builder.Services.AddScoped<IRepositorioPotrero>(sp =>
    new RepositorioPotreroSqlite(connectionString, sp.GetRequiredService<IGuidProvider>(), sp.GetRequiredService<IRegistroDeReses>(), sp.GetRequiredService<IRepositorioVacuna>()));
builder.Services.AddScoped<IRepositorioRes>(sp =>
    new RepositorioResSqlite(connectionString, sp.GetRequiredService<IGuidProvider>(), sp.GetRequiredService<IRegistroDeReses>()));
builder.Services.AddScoped<IRepositorioVacuna>(sp =>
    new RepositorioVacunaSqlite(connectionString, sp.GetRequiredService<IGuidProvider>()));
builder.Services.AddScoped<IRepositorioVenta>(sp =>
    new RepositorioVentaSqlite(connectionString, sp.GetRequiredService<IGuidProvider>(), sp.GetRequiredService<IRegistroDeReses>()));
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

// ── Reto 2 · TO-BE: creators concretos (Factory Method) ──
builder.Services.AddTransient<FabricaDeRes, FabricaTernero>();
builder.Services.AddTransient<FabricaDeRes, FabricaCebon>();
builder.Services.AddTransient<FabricaDeRes, FabricaNovillo>();
builder.Services.AddTransient<FabricaDeRes, FabricaVacaLechera>();
builder.Services.AddTransient<FabricaDeVacuna, FabricaBacteriana>();
builder.Services.AddTransient<FabricaDeVacuna, FabricaViva>();
builder.Services.AddTransient<FabricaDeProducto, FabricaLacteo>();
builder.Services.AddTransient<FabricaDeProducto, FabricaCarne>();
builder.Services.AddTransient<FabricaDeProducto, FabricaPiel>();

// ── Reto 2 · TO-BE: registros (punto único de decisión) ──
builder.Services.AddScoped<IRegistroDeReses, RegistroDeReses>();
builder.Services.AddScoped<IRegistroDeVacunas, RegistroDeVacunas>();
builder.Services.AddScoped<IRegistroDeProductos, RegistroDeProductos>();

// ── Reto 2 · TO-BE: builder de ventas (SC-1) ──
builder.Services.AddScoped<VentaBuilder>();

// ── Reto 2 · TO-BE: repositorio de productos (SC-1) ──
builder.Services.AddScoped<IRepositorioProducto>(sp =>
    new RepositorioProductoSqlite(connectionString, sp.GetRequiredService<IRegistroDeProductos>()));

// ── Reto 2 · TO-BE: handlers Observer — el ORDEN es el contrato (consola primero, stock después) ──
builder.Services.AddTransient<IManejadorDeEventos, HandlerConsola>();
builder.Services.AddScoped<IManejadorDeEventos, HandlerStockDerivados>();

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