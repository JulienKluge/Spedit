using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Spedit.UI.Components
{
    /// <summary>
    /// Interaction logic for ColorChangeControl.xaml
    /// </summary>
    public partial class ColorChangeControl
    {
        private bool _raiseEventAllowed;

        public static readonly RoutedEvent ColorChangedEvent = EventManager.RegisterRoutedEvent(
            "ColorChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ColorChangeControl));

        public event RoutedEventHandler ColorChanged
        {
            add { AddHandler(ColorChangedEvent, value); }
            remove { RemoveHandler(ColorChangedEvent, value); }
        }

        
        public ColorChangeControl()
        {
            InitializeComponent();
        }

        public void SetContent(string shName, Color c)
        {
            ColorName.Text = shName;
			UpdateColor(c);
        }

        public Color GetColor()
        {
            return Color.FromArgb(0xFF, (byte)((int)RSlider.Value), (byte)((int)GSlider.Value), (byte)((int)BSlider.Value));
        }

        private void SliderValue_Changed(object sender, RoutedEventArgs e)
        {
            if (!_raiseEventAllowed)
                return;

            var c = Color.FromArgb(0xFF, (byte) (int) RSlider.Value, (byte) (int) GSlider.Value,
                (byte) (int) BSlider.Value);

            UpdateColor(c, true, false);
            RaiseEvent(new RoutedEventArgs(ColorChangedEvent));
        }

		private void UpdateColor(Color c, bool updateTextBox = true, bool updateSlider = true)
		{
            var colorChannelMean = (c.R + c.G + c.B) / 3.0;

            _raiseEventAllowed = false;
			BrushRect.Background = new SolidColorBrush(c);
			BrushRect.Foreground = new SolidColorBrush((colorChannelMean > 128.0) ? Colors.Black : Colors.White);

			if (updateTextBox)
				BrushRect.Text = ((c.R << 16) | (c.G << 8) | (c.B)).ToString("X").PadLeft(6,'0');

			if (updateSlider)
			{
				RSlider.Value = c.R;
				GSlider.Value = c.G;
				BSlider.Value = c.B;
			}

			RaiseEvent(new RoutedEventArgs(ColorChangedEvent));
			_raiseEventAllowed = true;
		}

		private void BrushRect_TextChanged(object sender, TextChangedEventArgs e)
		{
		    if (!_raiseEventAllowed)
		        return;

		    var cVal = 0;
		    int result;
		    var parseString = BrushRect.Text.Trim();

		    if (parseString.StartsWith("0x", System.StringComparison.InvariantCultureIgnoreCase) && parseString.Length > 2)
		        parseString = parseString.Substring(2);

		    if (int.TryParse(parseString, System.Globalization.NumberStyles.HexNumber,
		        System.Globalization.CultureInfo.InvariantCulture, out result))
		        cVal = result;

		    UpdateColor(
		        Color.FromArgb(0xFF, (byte) ((cVal >> 16) & 0xFF), (byte) ((cVal >> 8) & 0xFF), (byte) (cVal & 0xFF)), false);
		}
	}
}
