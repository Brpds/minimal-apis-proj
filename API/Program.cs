using MinimalApi;

static IHostBuilder CreateHostBuilder(string [] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHost(webBuilder => 
        {
            webBuilder.UseKestrel();
            webBuilder.UseStartup<Startup>();
        });
}

CreateHostBuilder(args).Build().Run();