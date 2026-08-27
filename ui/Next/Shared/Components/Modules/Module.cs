using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Next.Shared.Components.Modules;

public partial class Module : UserControl
{
    public string ModuleName { get; protected set; }
    public int DeckPosition { get; protected set; }

    /// <summary>
    /// Constructor of module
    /// </summary>
    /// <param name="moduleName">Name of this module (Slider, buttons, rotor)</param>
    /// <param name="position">Position of this module is inserted of the deck</param>
    public Module(string moduleName, int position)
    {
        ModuleName = moduleName;
        DeckPosition = position;
    }
}