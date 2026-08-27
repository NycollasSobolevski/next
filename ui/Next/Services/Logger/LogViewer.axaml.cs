using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using Avalonia.Threading;
using System.Collections.ObjectModel;

namespace Next.Services;

public partial class LogViewer : UserControl
{
    public ObservableCollection<string> Logs { get; set; } = [];
    private Logger? _loggerService;
    public LogViewer()
    {
        InitializeComponent();

        var logService = App.Services!.GetService<Logger>();
        logService!.ExecuteOnLog += UpdateLog;

        this.DataContext = this;
    }
    public void UpdateLog(string value)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Logs.Add(value);
            
        });
        Dispatcher.UIThread.Post(() =>
    {
        logScroll.ScrollToEnd();
        
    }, DispatcherPriority.Default);
    }

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            if (_loggerService is not null)
                _loggerService.ExecuteOnLog -= UpdateLog;
            
            base.OnDetachedFromVisualTree(e);
        }
}