using Microsoft.EntityFrameworkCore;
using ChatbotTienda.Logica.Estadisticas;
using ChatbotTienda.Logica.Conversaciones;
using ChatbotTienda.Logica.Chatbot;
using Microsoft.Extensions.AI;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IEstadisticasServicio, EstadisticasServicio>();
builder.Services.AddScoped<IConversacionServicio, ConversacionServicio>();
builder.Services.AddScoped<IChatbotServicio, ChatbotServicio>();

// Cliente de IA: Ollama corriendo en la PC, con el modelo configurado en appsettings
var ollamaUrl = new Uri(builder.Configuration["Ollama:Url"]!);
var ollamaModelo = builder.Configuration["Ollama:Modelo"]!;
builder.Services.AddChatClient(new OllamaApiClient(ollamaUrl, ollamaModelo));

builder.Services.AddDbContext<ChatbotDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("ChatbotTienda")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
