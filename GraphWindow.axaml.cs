using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.NetworkInformation;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace NetGraph;

public partial class GraphWindow : Window, INotifyPropertyChanged
{
   public new event PropertyChangedEventHandler PropertyChanged;
   public readonly SolidColorBrush ReceivedColour = new(Color.FromRgb(213, 0, 0));
   public readonly SolidColorBrush SentColour = new(Color.FromRgb(104, 159, 56));
   private const double MiB = 8.0 /* mb to MB */ / 0x10_00_00 /* MB */ * 25.0 /* window height */;
   private const int Period = 25;
   private bool Dragging { get; set; }
   private DispatcherTimer Timer;
   private ObservableCollection<Traffic> Traffic { get; set; } = new();

   public double SentMax { get; set; }
   public double ReceivedMax { get; set; }
   public (long Sent, long Received) LastTraffic { get; set; }
   public NetworkInterface Network { get; set; }

   private readonly SolidColorBrush DownColour = new(Color.FromArgb(200, 220, 38, 38));
   private readonly SolidColorBrush UnstableColour = new(Color.FromArgb(150, 250, 204, 21));
   private readonly SolidColorBrush StableColour = new(Color.FromArgb(85, 85, 85, 85));
   private int TimerCount = 0;
   private SolidColorBrush StatusColour { get; set; }

   public GraphWindow()
   {
      InitializeComponent();

      StatusColour = StableColour;

      Timer = new DispatcherTimer
      {
         Interval = TimeSpan.FromSeconds(1)
      };
      Timer.Tick += Timer_Tick;
      Traffic.Clear();
      Timer.Start();

      DataContext = this;
   }

   private void Timer_Tick(object sender, EventArgs e)
   {
      GetRouterStatus();
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

   private void GetRouterStatus()
   {
      if (TimerCount == 0)
      {
         float snr = RouterStatus.GetSnr();
         if (snr <= 2.5)
            StatusColour = DownColour;
         else if (snr <= 8)
            StatusColour = UnstableColour;
         else StatusColour = StableColour;
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusColour)));

         ReceivedMax = Settings.ReceivedMax;
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReceivedMax)));
         SentMax = Settings.SentMax;
      }
      TimerCount = TimerCount > 3 ? 0 : TimerCount + 1;
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