using System;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace NetGraph
{
    public struct Traffic
    {
        public double Sent { get; set; }
        public double Received { get; set; }
        public double Overlap => Math.Min(Sent, Received);
    }
    public class GraphWindow : Window
    {
        private const double MiB = 8.0 /* mB to MB */ / 0x10_00_00 /* MB */ * 25.0 /* window height */;
        private const int Period = 25;
        private bool Dragging { get; set; }
        private DispatcherTimer Timer;
        private ObservableCollection<Traffic> Traffic { get; set; }

        public int SentMax { get; set; }
        public int ReceivedMax { get; set; }
        public (long Sent, long Received) LastTraffic { get; set; }
        public NetworkInterface Network { get; set; }

        public GraphWindow()
        {
            InitializeComponent();

            Traffic = new ObservableCollection<Traffic>();

            Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            Timer.Tick += Timer_Tick;
            Traffic.Clear();
            Timer.Start();

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
                Sent = (stat.BytesSent - LastTraffic.Sent) / SentMax * MiB,
                Received = (stat.BytesReceived - LastTraffic.Received) / ReceivedMax * MiB,
            });
            LastTraffic = (Sent: stat.BytesSent, Received: stat.BytesReceived);
            if (Traffic.Count > Period)
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
}