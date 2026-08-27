using Avalonia.Controls.Primitives;
using Microsoft.Extensions.DependencyInjection;
using Next.Services;

namespace Next.Shared.Components.Modules;

public partial class SoundSlider : Module
{

    public int CurrentValue { get; set; } = 75;

    public SoundSlider() : base("Slider Module 01",0)
    {
        InitializeComponent();
        ModuleLabel.Text = this.ModuleName;
        SliderValue.Value = CurrentValue;

    }

    public void OnChange(object sender, RangeBaseValueChangedEventArgs args)
    { 
        CurrentValue = (int)args.NewValue;
        currnentValueText.Text = CurrentValue.ToString();
        App.Services.GetService<Logger>()!.WriteLine(CurrentValue.ToString());
    }
}