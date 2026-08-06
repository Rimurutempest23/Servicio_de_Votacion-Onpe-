using EF_POO_II.Data;
using EF_POO_II.Data.Grpc;
using EF_POO_II.Data.Repositories;
using EF_POO_II.Data.Services;
using EF_POO_II.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>       
{
    options.ListenLocalhost(5212, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });

    options.ListenLocalhost(5213, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddDbContext<SistemaVotacionContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("cnx")));


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("Security:SessionTimeoutMinutes", 20));
        options.SlidingExpiration = true;
    });


builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();
builder.Services.AddGrpc();
builder.Services.AddSession();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IVotoRepository, VotoRepository>();
builder.Services.AddScoped<IVotacionService, VotacionService>();
builder.Services.AddScoped<ICandidatoRepository, CandidatoRepository>();
builder.Services.AddScoped<IReporteExportadoRepository, ReporteExportadoRepository>();
builder.Services.AddSingleton<IApiTokenService, ApiTokenService>();

var app = builder.Build();

RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

await DbInitializer.InicializarAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Errores/Exception");
}

app.UseStatusCodePagesWithReExecute("/Errores/Status/{0}");

app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<ResultadosGrpcService>();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");


app.Run();
