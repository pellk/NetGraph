using System.Text.RegularExpressions;
using Avalonia;

public class Settings
{
	private const char EqualChar = '=';
	private static readonly string settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");
	public static double SentMax { get; set; }
	public static double ReceivedMax { get; set; }
	public static PixelPoint Position { get; set; }
	public static string CurrentNetwork { get; set; }
	public static int RouterIndex { get; set; }

	public static void Load()
	{
		int x = 0, y = 0;
		foreach (string line in File.ReadAllLines(settingsPath))
		{
			int equals = line.IndexOf(EqualChar);
			string label = line[..equals];
			string value = line[(equals + 1)..];
			switch (label)
			{
				case "SentMax":
					SentMax = double.Parse(value);
					break;
				case "ReceivedMax":
					ReceivedMax = double.Parse(value);
					break;
				case "CurrentNetwork":
					CurrentNetwork = value;
					break;
				case "Router":
					if (int.TryParse(value, out int indx))
						RouterIndex = indx;
					else
						RouterIndex = 0;
					break;
				case "X":
					x = int.Parse(value);
					break;
				case "Y":
					y = int.Parse(value);
					break;
			}
		}
		Position = new PixelPoint(x, y);
		RouterStatus.SetSpeedSetting(Routers.List[RouterIndex]);
	}

	public static void Save()
	{
		var lines = new List<string>();
		lines.Add($"SentMax{EqualChar}{SentMax}");
		lines.Add($"ReceivedMax{EqualChar}{ReceivedMax}");
		lines.Add($"CurrentNetwork{EqualChar}{CurrentNetwork}");
		lines.Add($"Router{EqualChar}{RouterIndex}");
		lines.Add($"X{EqualChar}{Position.X}");
		lines.Add($"Y{EqualChar}{Position.Y}");
		File.WriteAllLines(settingsPath, lines);
	}
}