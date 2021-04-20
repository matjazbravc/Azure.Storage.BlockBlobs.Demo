using Client.ConsoleApp.Configuration;
using Client.ConsoleApp.Services;
using Client.ConsoleApp.Services.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Client.ConsoleApp
{
    /// <summary>
    /// This simple client console application communicate with Azure.Storage.BlockBLobs.Demo Web API
    /// and uploads (large) file(s) to the Azure Storage Blob Container.
    /// </summary>
    internal class Program
    {
        public static ILogger<Program> Logger { get; private set; }

        private static void Main(string[] args)
        {
            // Global Exception Handler
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            // Add configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true).Build();

            var serviceProvider = new ServiceCollection()
                .AddOptions()
                .AddLogging()
                .AddSingleton<IFileUploader, FileUploader>()
                .AddTransient<IRestClientHelper, RestClientHelper>()
                .AddTransient<IUploadStatistics, UploadStatistics>()
                .AddTransient<ITaskHelper, TaskHelper>()
                // Configure BlobsWebApiConfig
                .Configure<AppConfig>(configuration.GetSection("AppConfig"))
                // Register configuration as Singleton service
                .AddSingleton(provider => provider.GetService<IOptions<AppConfig>>().Value)
                .BuildServiceProvider();

            // Create logger with filters
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("Client.ConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });

            // Read configuration
            var config = serviceProvider.GetService<AppConfig>();

            Logger = loggerFactory.CreateLogger<Program>();
            Logger.LogInformation($"Start uploading file {config.FullPathFileName} ...");

            // Start file upload
            var fileUploader = serviceProvider.GetService<IFileUploader>();
            fileUploader.UploadFileAsync(config.FullPathFileName, config.BlockSize, config.MaxThreads).Wait();

            Logger.LogInformation($"File {config.FullPathFileName} was successfuly uploaded. Press <Enter> key for exit.");
            Console.ReadLine();
        }

        internal static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.LogError($"{e.ExceptionObject} \r\nPress <Enter> key for exit.");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
