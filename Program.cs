var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IGuidService, GuidService>();
// builder.Services.AddScoped<IGuidService, GuidService>();
// builder.Services.AddSingleton<IGuidService, GuidService>();
builder.Services.AddSingleton<ConsumeGuidService>();
var app = builder.Build();
app.UseHttpsRedirection();

app.Use(async (httpContext, next) => {
    var guidService = httpContext.RequestServices.GetRequiredService<ConsumeGuidService>();
    Console.WriteLine("Incoming request M1");
    Console.WriteLine("1st Service req: {0}",guidService.Id);
    await next();
});

app.Use(async (httpContext, next) => {
    //service with a different scope
    using (var scope =  httpContext.RequestServices.CreateScope())
    {
        var diffScopedGuidService = scope
        .ServiceProvider    
        .GetRequiredService<ConsumeGuidService>();

        Console.WriteLine("2nd Service req: {0}",diffScopedGuidService.Id);
    }

    var guidService = httpContext.RequestServices.GetRequiredService<ConsumeGuidService>();
    Console.WriteLine("Incoming request M2");
    Console.WriteLine("3rd Service req: {0}", guidService.Id);
    await next();
});

// app.Use(async (context, next) =>
// {
//     app.Logger.LogInformation("Incoming request {0} {1}",
//      context.Request.Method, context.Request.Path);
//     await next(context);
//     app.Logger.LogInformation("Outgoing request {0} {1}", 
//     context.Response.StatusCode, context.Response.ContentType);
// });

app.MapGet("/", () => "Hello World!");

app.MapGet("/guid", (ConsumeGuidService guidService) 
=> $"4th service req: {guidService.Id}");

// app.MapGet("/hello", () => "Hello path!");
// app.MapPost("/user", (User user) => user);
// app.MapPut("/", () => "This is a PUT");
// app.MapDelete("/", () => "This is a DELETE");

app.Run();

// class User 
// {
//     public string? Name { get; set; }
//     public int Age { get; set; }
// }

interface IGuidService
{
    Guid GetGuid();
}

class GuidService : IGuidService
{
    private readonly Guid Id;
    public GuidService()
    {
        Id = Guid.NewGuid();
    }
    public Guid GetGuid() => Id;
}

class ConsumeGuidService(IGuidService guidService)
{
    public Guid Id { get; set; } = guidService.GetGuid();
}