using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Neurabrain.Domain.Interfaces;
using Neurabrain.Infrastructure.Data;
using Neurabrain.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Ρύθμιση της Βάσης Δεδομένων (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Προσθήκη Controllers
builder.Services.AddControllers();

// 3. Προσθήκη CORS (Επιτρέπουμε στο Blazor να κάνει αιτήματα στο API μας)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy =>
        {
            policy.WithOrigins("https://localhost:7034", "http://localhost:5018", "https://localhost:7134", "http://localhost:5016", "https://localhost:7093", "http://localhost:5095") // Τα URLs του Blazor
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// 4. Προσθήκη Swagger (Για να δοκιμάζουμε το API μας μέσα από UI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IAiService, AiService>();

// 5. Ρύθμιση του Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "NeurabrainAuth";
        options.Cookie.HttpOnly = true;
        // Το SameSite.Lax επιτρέπει στο cookie να παίζει σωστά ανάμεσα στα subdomains
        options.Cookie.SameSite = SameSiteMode.Lax;

        // Όταν μπει σε παραγωγή (π.χ. neurabrain.gr), θα προσθέσουμε:
        // options.Cookie.Domain = ".neurabrain.gr"; 

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401; // Αντί να κάνει redirect, το API απλά λέει "Απαγορεύεται"
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });

var app = builder.Build();

// Ρύθμιση του HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ενεργοποίηση του CORS
app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

// Χαρτογράφηση των Controllers
app.MapControllers();

app.Run();