using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

namespace NetGraph
{
    public class SettingsWindow : Window
    {
        private Dictionary<string, string> Settings { get; set; }
        private int SentMax { get; set; }
        private int ReceivedMax { get; set; }
        private int CurrentNetwork { get; set; }
        private PixelPoint GraphPosition { get; set; }
        private NetworkInterface[] Interfaces { get; set; }
        private GraphWindow graphWindow { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();

            if (!NetworkInterface.GetIsNetworkAvailable())
                throw new NetworkInformationException();
            Interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(i => i.OperationalStatus == OperationalStatus.Up).ToArray();
                var env = AppDomain.CurrentDomain.BaseDirectory;

            Settings = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"settings.config"))
                .ToDictionary(
                    s => s.Substring(0, s.IndexOf(':')),
                    s => s.Substring(s.IndexOf(':') + 1));

            SentMax = int.Parse(Settings["SentMax"]);
            ReceivedMax = int.Parse(Settings["ReceivedMax"]);
            CurrentNetwork = 0;
            for (int i = 0; i < Interfaces.Length; i++)
                if (Interfaces[i].Id == Settings["CurrentNetwork"])
                    CurrentNetwork = i;
            GraphPosition = new PixelPoint(int.Parse(Settings["X"]), int.Parse(Settings["Y"]));

            this.DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OkButton_Click(object serder, RoutedEventArgs e)
        {
            Settings["CurrentNetwork"] = Interfaces[CurrentNetwork].Id;
            Settings["SentMax"] = SentMax.ToString();
            Settings["ReceivedMax"] = ReceivedMax.ToString();
            File.WriteAllLines("settings.config", Settings.Select(s => s.Key + ":" + s.Value));

            var stat = Interfaces[CurrentNetwork].GetIPv4Statistics();
            graphWindow?.Close();
            graphWindow = new GraphWindow
            {
                LastTraffic = new Traffic { Sent = stat.BytesSent, Received = stat.BytesReceived },
                Network = Interfaces[CurrentNetwork],
                SentMax = SentMax,
                ReceivedMax = ReceivedMax
            };
            graphWindow.PositionChanged += Graph_PositionChanged;
            graphWindow.Show();
            graphWindow.Position = GraphPosition;
            WindowState = WindowState.Minimized;
        }

        private void QuitButton_Click(object serder, RoutedEventArgs e)
        {
            Close();
        }

        private void Graph_PositionChanged(object sender, PixelPointEventArgs e)
        {
            Settings["X"] = e.Point.X.ToString();
            Settings["Y"] = e.Point.Y.ToString();
            File.WriteAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"settings.config"), Settings.Select(s => s.Key + ":" + s.Value));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            graphWindow?.Close();
        }
    }
}