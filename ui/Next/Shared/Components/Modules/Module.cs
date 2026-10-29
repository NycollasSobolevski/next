using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Next.Shared.Components.Modules;

public partial class Module : UserControl
{
    public static readonly StyledProperty<string> ModuleNameProperty =
        AvaloniaProperty.Register<Module, string>(nameof(ModuleName), defaultValue: "Module");

    public string ModuleName
    {
        get => GetValue(ModuleNameProperty);
        set => SetValue(ModuleNameProperty, value);
    }

    public int DeckPosition { get; protected set; }

    public Module(string moduleName, int position)
    {
        ModuleName = moduleName;
        DeckPosition = position;
    }

    public Module() : this("Default Module", 0) { }
}