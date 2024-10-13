using System.ComponentModel;
using System.Windows;

namespace Fazbot.UI;

public partial class VolumeAdjustment
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private float _volume;
    public float Volume
    {
        get => _volume;
        set
        {
            if (_volume == value) return;
            _volume = value;
            TextBox.Text = _volume.ToString("0.00");
            OnPropertyChanged(nameof(Volume));
        }
    }
    
    public VolumeAdjustment()
    {
        InitializeComponent();
    }

    private void OnOkButtonClicked(object sender, RoutedEventArgs e)
    {
        Volume = float.Parse(TextBox.Text);
        Close();
    }
}