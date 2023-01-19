using System.ComponentModel;
using System.Net.NetworkInformation;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace NetGraph
{
   public class SettingsWindow : Window
   {
      private double SentMax { get; set; }
      private double ReceivedMax { get; set; }
      private int CurrentNetwork { get; set; }
      private NetworkInterface[] Interfaces { get; set; }
      private GraphWindow GraphWindow { get; set; }

      public SettingsWindow()
      {
         InitializeComponent();

         Settings.Load();

         SentMax = Settings.SentMax;
         ReceivedMax = Settings.ReceivedMax;

         if (!NetworkInterface.GetIsNetworkAvailable())
            throw new NetworkInformationException();

         Interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(i => i.OperationalStatus == OperationalStatus.Up).ToArray();

         CurrentNetwork = 0;
         for (int i = 0; i < Interfaces.Length; i++)
            if (Interfaces[i].Id == Settings.CurrentNetwork)
               CurrentNetwork = i;

         this.DataContext = this;
      }

      private void InitializeComponent()
      {
         AvaloniaXamlLoader.Load(this);
      }

      private void OkButton_Click(object serder, RoutedEventArgs e)
      {
         Settings.CurrentNetwork = Interfaces[CurrentNetwork].Id;
         Settings.SentMax = SentMax;
         Settings.ReceivedMax = ReceivedMax;
         Settings.Save();

         var stat = Interfaces[CurrentNetwork].GetIPv4Statistics();
         GraphWindow?.Close();
         GraphWindow = new GraphWindow
         {
            LastTraffic = (Sent: stat.BytesSent, Received: stat.BytesReceived),
            Network = Interfaces[CurrentNetwork],
            SentMax = SentMax,
            ReceivedMax = ReceivedMax,
         };
         GraphWindow.Show();
         GraphWindow.Position = Settings.Position;
         WindowState = WindowState.Minimized;
      }

      private void QuitButton_Click(object serder, RoutedEventArgs e)
      {
         Close();
      }

      private void Window_Closing(object sender, CancelEventArgs e)
      {
         GraphWindow?.Close();
      }
   }
}