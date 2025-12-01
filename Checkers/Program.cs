using CheckersGameProject.Api.Services;
using CheckersGameProject.Contracts;
using CheckersGameProject.Services;

var builder = WebApplication.CreateBuilder(args);

// ... configurasi CORS ...
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJS",
        policy => { policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod(); });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- DEPENDENCY INJECTION ---
// 1. Register Factory
builder.Services.AddSingleton<ICheckersGameFactory, CheckersGameFactory>();

// 2. Register GameService (Singleton State)
builder.Services.AddSingleton<GameService>();

var app = builder.Build();

// ... middleware ...
app.UseCors("AllowNextJS");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();