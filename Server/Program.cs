var builder = WebApplication.CreateSlimBuilder(args);

var gql = builder.Services.AddGraphQLServer();

gql.AddServerTypes();

var app = builder.Build();

app.MapGraphQL("/");

app.Run();
