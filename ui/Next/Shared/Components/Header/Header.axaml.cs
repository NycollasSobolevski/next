using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Next.Services;

namespace Next.Shared.Components;

public partial class Header : UserControl
{
    public Header()
    {
        InitializeComponent();
    }

    public void OnUsbClicked(object sender, RoutedEventArgs args)
    {
        Console.WriteLine();
    }
    public void OnConnectClicked(object sender, RoutedEventArgs args)
    {
        Console.WriteLine();
    }
}