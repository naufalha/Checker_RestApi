///
/// 
/// 
using CheckersGameProject.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJS",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") // URL Frontend kamu
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// 1. Add Services to the container
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- CRITICAL: Register GameService as Singleton ---
// Singleton means 1 instance for the whole app lifetime (keeps games in memory)
builder.Services.AddSingleton<GameService>();

var app = builder.Build();
app.UseCors("AllowNextJS");

// 2. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();