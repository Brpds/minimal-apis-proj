using MinimalApi;

static IHostBuilder CreateHostBuilder(string [] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHost(webBuilder => 
        {
            webBuilder.UseUrls("http://0.0.0.0:5223");
            webBuilder.UseKestrel();
            webBuilder.UseStartup<Startup>();
        });
}

CreateHostBuilder(args).Build().Run();