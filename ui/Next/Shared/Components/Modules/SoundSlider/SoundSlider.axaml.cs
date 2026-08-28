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
        string value = CurrentValue.ToString();
        if(value == null) return;
        currnentValueText.Text = value;
        App.Reader.SendData(value);
        App.Logger.WriteLine(value);
    }
}