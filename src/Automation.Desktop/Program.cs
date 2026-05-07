using Automation.Desktop.Application.Managers;
using Automation.Desktop.Application.Services;
using Automation.Desktop.Application.Workflow;
using Automation.Desktop.Core.Interfaces;
using Automation.Desktop.Infrastructure.Adb;
using Automation.Desktop.Infrastructure.Database;
using Automation.Desktop.Infrastructure.Imaging;
using Automation.Desktop.Infrastructure.Imaging.Detection;
using Automation.Desktop.Infrastructure.Logging;
using Automation.Desktop.Infrastructure.OCR;
using Automation.Desktop.Infrastructure.OCR.Detection;
using Automation.Desktop.UI.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Automation.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();
        ConfigureServices(services);

        using var provider = services.BuildServiceProvider();
        Application.Run(provider.GetRequiredService<MainForm>());
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder => builder
            .ClearProviders()
            .AddConsole()
            .SetMinimumLevel(LogLevel.Information));

        services.AddSingleton<IAdbService, AdbService>();
        services.AddSingleton<IDeviceManager, DeviceManager>();
        services.AddSingleton<IDatabaseService, SqliteDatabaseService>();
        services.AddSingleton<ILogService, LogService>();
        services.AddSingleton<OCRTextDetector>();
        services.AddSingleton<IOcrService, OcrService>();
        services.AddSingleton<TemplateMatcher>();
        services.AddSingleton<IImageDetectionService, ImageDetectionService>();
        services.AddSingleton<WorkflowExecutor>();
        services.AddSingleton<WorkflowManager>();
        services.AddSingleton<IAutomationEngine, AutomationEngine>();

        services.AddSingleton<MainForm>();
    }
}
