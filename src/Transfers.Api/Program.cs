using MediatR;
using Transfers.Application;
using Transfers.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssemblyContaining<RegisterTransfer>());
builder.Services.AddSingleton<ITransferRepository, InMemoryTransferRepository>();

var app = builder.Build();

app.MapPost("/transfers", async (RegisterTransfer request, ISender sender, CancellationToken cancellationToken) =>
{
    var result = await sender.Send(request, cancellationToken);
    return result.Status switch
    {
        RegistrationStatus.Created => Results.Created($"/transfers/{request.Id}", result.Transfer),
        RegistrationStatus.Replayed => Results.Ok(result.Transfer),
        RegistrationStatus.Conflict => Results.Problem(
            statusCode: 409, detail: "This transfer ID is already registered with different details."),
        RegistrationStatus.Invalid => Results.Problem(statusCode: 400, detail: result.Error),
        _ => throw new InvalidOperationException("Unexpected registration result.")
    };
});

app.MapGet("/transfers/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
{
    var transfer = await sender.Send(new GetTransfer(id), cancellationToken);
    return transfer is null ? Results.NotFound() : Results.Ok(transfer);
});

app.Run();
