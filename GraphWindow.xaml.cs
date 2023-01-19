using System.Collections.ObjectModel;
using System.Net.NetworkInformation;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;

namespace NetGraph
{
   public struct Traffic
   {
      public double Overlap { get; set; }
      public double Difference { get; set; }
      public SolidColorBrush DifferenceColour { get; set; }
   }
   public class GraphWindow : Window
   {
      public readonly SolidColorBrush ReceivedColour = new(Color.FromRgb(213, 0, 0));
      public readonly SolidColorBrush SentColour = new(Color.FromRgb(104, 159, 56));
      private const double MiB = 8.0 /* mb to MB */ / 0x10_00_00 /* MB */ * 25.0 /* window height */;
      private const int Period = 25;
      private bool Dragging { get; set; }
      private DispatcherTimer Timer;
      private ObservableCollection<Traffic> Traffic { get; set; } = new();

      public int SentMax { get; set; }
      public int ReceivedMax { get; set; }
      public (long Sent, long Received) LastTraffic { get; set; }
      public NetworkInterface Network { get; set; }

      public GraphWindow()
      {
         InitializeComponent();

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
         double sent = (stat.BytesSent - LastTraffic.Sent) / SentMax * MiB;
         double received = (stat.BytesReceived - LastTraffic.Received) / ReceivedMax * MiB;
         Traffic.Add(new Traffic
         {
            Overlap = Math.Min(sent, received),
            Difference = Math.Abs(sent - received),
            DifferenceColour = sent > received ? SentColour : ReceivedColour,
         });
         LastTraffic = (Sent: stat.BytesSent, Received: stat.BytesReceived);
         if (Traffic.Count >= Period)
            Traffic.RemoveAt(0);
      }

      private void Window_PointerPressed(object sender, PointerPressedEventArgs e)
      {
         Dragging = true;
         Cursor = new Cursor(StandardCursorType.Hand);
      }

      private void Window_PointerReleased(object sender, PointerReleasedEventArgs e)
      {
         if (!Dragging) return;
         Cursor = Cursor.Default;
         Dragging = false;
         Settings.Save();
      }

      private void Window_PointerMoved(object sender, PointerEventArgs e)
      {
         if (!Dragging) return;
         var p = e.GetPosition(this);
         Position = new PixelPoint(Position.X + (int)p.X, Position.Y + (int)p.Y);
         Settings.Position = Position;
      }
   }
}