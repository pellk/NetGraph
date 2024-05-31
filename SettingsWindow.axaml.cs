using System.Net.NetworkInformation;

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NetGraph;

public partial class SettingsWindow : Window
{
	private double SentMax { get; set; }
	private double ReceivedMax { get; set; }
	private int CurrentNetwork { get; set; }
	private List<RouterInfo> RouterList { get; init; } = Routers.List;
	private int RouterIndex { get; set; }

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

		RouterIndex = Settings.RouterIndex;

		this.DataContext = this;
	}

	private void OkButton_Click(object serder, RoutedEventArgs e)
	{
		Settings.CurrentNetwork = Interfaces[CurrentNetwork].Id;
		Settings.SentMax = SentMax;
		Settings.ReceivedMax = ReceivedMax;
		Settings.RouterIndex = RouterIndex;
		Settings.Save();

		var stat = Interfaces[CurrentNetwork].GetIPv4Statistics();
		GraphWindow?.Close();
		GraphWindow = new GraphWindow
		{
			LastTraffic = (Sent: stat.BytesSent, Received: stat.BytesReceived),
			Network = Interfaces[CurrentNetwork],
			SentMax = SentMax,
			ReceivedMax = ReceivedMax,
			Router = Routers.List[RouterIndex],
		};
		GraphWindow.Show();
		GraphWindow.Position = Settings.Position;
		WindowState = WindowState.Minimized;
	}

	private void QuitButton_Click(object serder, RoutedEventArgs e)
	{
		Close();
	}

	private void Window_Closing(object sender, WindowClosingEventArgs e)
	{
		GraphWindow?.Close();
	}
}