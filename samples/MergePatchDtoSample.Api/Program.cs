using MergePatchDtoSample.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<PersonStore>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
