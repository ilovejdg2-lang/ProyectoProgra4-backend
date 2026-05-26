var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<proyecto_cafe_una_backend.Services.ProductosService>();
builder.Services.AddSingleton<proyecto_cafe_una_backend.Services.UsuariosService>();
builder.Services.AddSingleton<proyecto_cafe_una_backend.Services.InformacionService>();
builder.Services.AddSingleton<proyecto_cafe_una_backend.Services.VoluntariadoService>();
builder.Services.AddSingleton<proyecto_cafe_una_backend.Services.AuthService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
