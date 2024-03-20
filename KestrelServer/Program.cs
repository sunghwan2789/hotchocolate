using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.MapGet("/", Bar);

_ = DoSubscription(app.Lifetime.ApplicationStopped);

app.Run();

async Task DoSubscription(CancellationToken stopped) {
    var client = new HttpClient();

    while (!stopped.IsCancellationRequested) {
        try {
            await foreach (var response in client.GetFromJsonAsAsyncEnumerable<int>("http://localhost:5241/", stopped)) {
                Console.WriteLine(response);
            }
        } catch (Exception ex) {
            Console.WriteLine("retry... " + ex);
            await Task.Delay(1000, stopped);
        }
    }
}

async IAsyncEnumerable<int> Bar(IHttpContextAccessor contextAccessor, [EnumeratorCancellation] CancellationToken cancellationToken) {
    var context = contextAccessor.HttpContext;
    Console.WriteLine(context?.TraceIdentifier);
    var i = 0; 
    while (!cancellationToken.IsCancellationRequested) {
        yield return i++;
        await Task.Delay(1000, cancellationToken);
    }
}
