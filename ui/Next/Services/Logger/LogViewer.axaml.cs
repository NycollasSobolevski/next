using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;

namespace Next.Services;

public partial class LogViewer : UserControl
{
    public List<string> Logs { get; set; } = [];

    public LogViewer()
    {
        InitializeComponent();

        var logService = App.Services!.GetService<Logger>();
        Logs = logService!.Logs;

        this.DataContext = this;
    }
}