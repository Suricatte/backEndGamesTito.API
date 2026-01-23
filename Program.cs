using backEndGamesTito.API.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<backEndGamesTito.API.Services.EmailService>();
builder.Services.AddScoped<backEndGamesTito.API.Services.WhatsAppService>();
builder.Services.AddScoped<backEndGamesTito.API.Services.TelegramService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
