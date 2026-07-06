using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(ctx =>
    {
        if (ctx.Route.ClusterId == "product-catalog-cluster")
        {
            ctx.AddResponseTransform(async responseCtx =>
            {
                if (responseCtx.ProxyResponse?.Headers.TryGetValues("X-Container-Id", out var values) == true)
                    responseCtx.HttpContext.Response.Headers["X-Container-Id"] = values.FirstOrDefault();
                await Task.CompletedTask;
            });
        }
    });

var app = builder.Build();

app.MapReverseProxy();
app.Run();
