# Clean Architecture with .NET

A small ASP.NET Core API for registering and looking up simulated transfers. It illustrates Clean Architecture and CQRS with MediatR. The API uses in-memory storage and never moves funds.

| Project | Responsibility |
|---|---|
| Domain | Transfer rules and immutable transfer data |
| Application | Registration command, lookup query and repository interface |
| Infrastructure | Thread-safe in-memory implementation of the repository |
| API | HTTP endpoints and dependency injection |
| Tests | xUnit checks for registration, validation and idempotency |

The application depends on the domain. Infrastructure implements the application interface, and the API wires the layers together. Commands and queries share an in-memory store.

## Run locally

Requires the .NET 10 SDK.

```sh
dotnet test
dotnet run --project src/Transfers.Api --no-launch-profile -- --urls http://localhost:5080
```

`POST /transfers` with `Content-Type: application/json`

```json
{
  "id": "e87442e2-f8c7-437c-8aec-d1285beac673",
  "sourceAccount": "ACCT-01",
  "destinationAccount": "ACCT-02",
  "amount": 125.50
}
```

The first request returns `201 Created`, the registered transfer, and a `Location` header. Repeating the same request returns `200 OK`. Reusing the ID with different details returns `409 Conflict`.

`GET /transfers/e87442e2-f8c7-437c-8aec-d1285beac673` returns the transfer or `404 Not Found`.

GitHub Actions builds the solution and runs the tests. The sample uses fictional account identifiers. Storage resets on restart.
