using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Spedit.UI.Components
{
    /// <summary>
    /// Interaction logic for ColorChangeControl.xaml
    /// </summary>
    public partial class ColorChangeControl : UserControl
    {
        public static readonly RoutedEvent ColorChangedEvent = EventManager.RegisterRoutedEvent(
        "ColorChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ColorChangeControl));

        public event RoutedEventHandler ColorChanged
        {
            add { AddHandler(ColorChangedEvent, value); }
            remove { RemoveHandler(ColorChangedEvent, value); }
        }

        bool RaiseEventAllowed = false;
        public ColorChangeControl()
        {
            InitializeComponent();
        }

        public void SetContent(string SHName, Color c)
        {
            ColorName.Text = SHName;
            BrushRect.Fill = new SolidColorBrush(c);
            RSlider.Value = (double)c.R;
            GSlider.Value = (double)c.G;
            BSlider.Value = (double)c.B;
            RaiseEventAllowed = true;
        }

        public Color GetColor()
        {
            return Color.FromArgb(0xFF, (byte)((int)RSlider.Value), (byte)((int)GSlider.Value), (byte)((int)BSlider.Value));
        }

        private void SliderValue_Changed(object sender, RoutedEventArgs e)
        {
            if (RaiseEventAllowed)
            {
                Color c = Color.FromArgb(0xFF, (byte)((int)RSlider.Value), (byte)((int)GSlider.Value), (byte)((int)BSlider.Value));
                BrushRect.Fill = new SolidColorBrush(c);
                RoutedEventArgs raiseEvent = new RoutedEventArgs(ColorChangeControl.ColorChangedEvent);
                this.RaiseEvent(raiseEvent);
            }
        }
    }
}
