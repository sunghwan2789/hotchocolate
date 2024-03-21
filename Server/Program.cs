using System.Runtime.CompilerServices;
using System.Text.Json;
using HotChocolate.Transport.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGraphQLServer()
    .AddQueryType(_ => _.Field("foo").Resolve(1))
    .AddSubscriptionType<Subscription>()
    ;
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.MapGraphQL();

_ = DoSubscription(app.Lifetime.ApplicationStopped);

app.Run();

async Task DoSubscription(CancellationToken stopped) {
    var httpClient = new HttpClient();
    using var client = GraphQLHttpClient.Create(httpClient, true);

    while (!stopped.IsCancellationRequested) {
        try {
            var request = new GraphQLHttpRequest("""subscription S { bar }""", new Uri("http://localhost:5241/graphql"));
            using var result = await client.SendAsync(request, stopped);
            await foreach (var response in result.ReadAsResultStreamAsync(stopped)) {
                Console.WriteLine(JsonSerializer.Serialize(response.Data));
            }
        } catch (Exception ex) {
            Console.WriteLine("retry... " + ex);
            await Task.Delay(1000, stopped);
        }
    }
}

public class Subscription {

    [Subscribe(With = nameof(BarStream))]
    public int Bar([EventMessage] int message) => message;

    private static async IAsyncEnumerable<int> BarStream([Service] IHttpContextAccessor contextAccessor, [Service] IHostApplicationLifetime lifetime, [EnumeratorCancellation] CancellationToken cancellationToken) {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(lifetime.ApplicationStopping, cancellationToken);
        var context = contextAccessor.HttpContext;
        Console.WriteLine(context?.TraceIdentifier);
        var i = 0; 
        while (!cts.Token.IsCancellationRequested) {
            yield return i++;
            await Task.Delay(1000, cts.Token);
            // await Task.Delay(1000);
        }
    }
}

