using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using TranscriptsProcessor.Services;
using TranscriptsProcessor.TranscriptionService;

namespace TranscriptsProcessor
{
    public class Program
    {
        static int Main(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Set up Dependency Injection
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(configuration.GetSection("Logging"));
                    builder.AddConsole();
                })
                .BuildServiceProvider();

            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices((hostContext, services) =>
                 {
                     ConfigureServices(services);
                 })
                 .Build();

            var app = new CommandLineApplication();
            app.HelpOption();

            var pathArg = app.Argument("path", "Path to get user data").IsRequired();

            app.OnExecute(() =>
            {
                try
                {
                    var services = host.Services.CreateScope().ServiceProvider;
                    var logger = serviceProvider.GetService<ILogger<Program>>();
                    var transcriptService = serviceProvider.GetService<ITranscriptiService>();
                    var sender = serviceProvider.GetService<ISender>();
                    var fileManager = serviceProvider.GetService<IFileManager>();

                    var scheduler = new Scheduler(logger, fileManager, sender);
                    scheduler.Start(pathArg.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Something went wrong: " + ex.Message);
                }
            });

            return app.Execute(args);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Processor>();
            services.AddSingleton<Scheduler>();
            services.AddSingleton<IFileValidator, FileValidator>();
            services.AddSingleton<ISender, Sender>();
            services.AddSingleton<IFileManager, FileManager>();
            services.AddSingleton<ITranscriptiService, TranscriptService>();
        }
    }
}