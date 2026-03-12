    using backEndGamesTito.API.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do CORS (Fundamental para multiplatarforma)
builder.Services.AddCors(options =>
{
    options.AddPolicy("GamesTitoPolicy", policy =>
    {
        policy.AllowAnyOrigin() // Site web, desktop ou mobile acessem
              .AllowAnyHeader() // Envia tokens e JSON
              .AllowAnyMethod(); // Permite GET, POST, PUT, DELETE
    });
});

builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<backEndGamesTito.API.Services.EmailService>();
builder.Services.AddScoped<backEndGamesTito.API.Services.WhatsAppService>();
builder.Services.AddScoped<backEndGamesTito.API.Services.TelegramService>();

builder.Services.AddScoped<JogoRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("GamesTitoPolicy");

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
