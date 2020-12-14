using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.NetworkInformation;

namespace NetGraph
{
    public struct Traffic
    {
        public long Sent { get; set; }
        public long Received { get; set; }
    }
    public class GraphWindow : Window
    {
        private bool Dragging { get; set; }
        private int period = 25;
        public int SentMax { get; set; }
        public int ReceivedMax { get; set; }
        public Traffic LastTraffic { get; set; }
        private ObservableCollection<Traffic> Traffic { get; set; }
        public NetworkInterface Network { get; set; }
        public DispatcherTimer timer;

        public GraphWindow()
        {
            InitializeComponent();

            Traffic = new ObservableCollection<Traffic>();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            Traffic.Clear();
            timer.Start();

            this.DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var stat = Network.GetIPv4Statistics();
            Traffic.Add(new Traffic
            {
                Sent = stat.BytesSent - LastTraffic.Sent,
                Received = stat.BytesReceived - LastTraffic.Received
            });
            LastTraffic = new Traffic
            {
                Sent = stat.BytesSent,
                Received = stat.BytesReceived
            };
            if (Traffic.Count > period)
                Traffic.RemoveAt(0);
        }

        private void Window_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            Dragging = true;
            Cursor = new Cursor(StandardCursorType.Hand);
        }

        private void Window_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (Dragging)
            {
                Cursor = Cursor.Default;
                Dragging = false;
            }
        }

        private void Window_PointerMoved(object sender, PointerEventArgs e)
        {
            if (Dragging)
            {
                var p = e.GetPosition(this);
                Position = new PixelPoint(Position.X + (int)p.X, Position.Y + (int)p.Y);
            }
        }
    }

    public class MegabyteConverter : IMultiValueConverter
    {
        public object Convert(IList<object> value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value[0] is long && value[1] is int)
                return System.Convert.ToDouble(value[0]) / (System.Convert.ToDouble(value[1]) / 8.0 * 1048576) * 25.0;
            else
                return 0;
        }
    }
}