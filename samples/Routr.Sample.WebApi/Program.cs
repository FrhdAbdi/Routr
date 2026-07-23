using Routr;
using Routr.Sample.WebApi.Behaviors;
using Routr.Sample.WebApi.Features;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRoutrHandlers();
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));


var app = builder.Build();

app.MapPost("/users", async (CreateUserCommand cmd, ISender sender) =>
    await sender.Send(cmd));

app.MapGet("/users/{id:guid}", async (Guid id, ISender sender) =>
    await sender.Send(new GetUserQuery(id)));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();