using Microsoft.EntityFrameworkCore;
using Neurabrain.Infrastructure.Data;

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
            policy.WithOrigins("https://localhost:7001", "http://localhost:5001") // Τα URLs του Blazor
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 4. Προσθήκη Swagger (Για να δοκιμάζουμε το API μας μέσα από UI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseAuthorization();

// Χαρτογράφηση των Controllers
app.MapControllers();

app.Run();