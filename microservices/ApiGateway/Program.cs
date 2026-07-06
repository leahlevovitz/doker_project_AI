using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(ctx =>
    {
        if (ctx.Route.ClusterId == "product-catalog-cluster")
        {
            ctx.AddResponseTransform(responseCtx =>
            {
                var proxyResponse = responseCtx.ProxyResponse;
                if (proxyResponse != null)
                {
                    // Check both response headers and content headers
                    if (proxyResponse.Headers.TryGetValues("X-Container-Id", out var values))
                    {
                        responseCtx.HttpContext.Response.Headers["X-Container-Id"] = values.FirstOrDefault();
                    }
                    else if (proxyResponse.Content.Headers.TryGetValues("X-Container-Id", out var contentValues))
                    {
                        responseCtx.HttpContext.Response.Headers["X-Container-Id"] = contentValues.FirstOrDefault();
                    }
                }
                return ValueTask.CompletedTask;
            });
        }
    });

var app = builder.Build();

app.MapReverseProxy();
app.Run();
