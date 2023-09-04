using Serilog;
using Emgu.CV;

VideoCapture captureStream = null;
DateTime? lastCapture = null;
 Task Warm()
{
    return Task.Run(() =>
    {
        Log.Information("Inicializando camara..");
        captureStream = new VideoCapture(0); // 0 es la camara pordefecto
        captureStream.Start();
        Log.Information("Captura Iniciada");

    });
}


async Task<byte[]> GetFrame()
{
    Log.Information("Captura requerida");
    var frame = captureStream.QueryFrame();
    Log.Debug("Frame Capturado");
    lastCapture= DateTime.Now;
    var tempFile = "temp.jpg";
    frame.Save(tempFile);
    Log.Debug("Archivo temporal guardado");
    var content= await File.ReadAllBytesAsync(tempFile);
    File.Delete(tempFile);
    Log.Debug("Archivo temporal eliminado");
    return content;
}



var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog(Log.Logger);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.MapGet("/weatherforecast", () =>
{
 
    return "Hola Mundo";
})
.WithName("GetWeatherForecast");

_ = Warm();

app.Map("/", () => new
{
    LastCapture = lastCapture
});

app.Map("/Frame", async () =>
{
    var content = await GetFrame();
    return Results.File(content, "image/jpeg");
});

app.Run(); 
