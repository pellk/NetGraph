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
            case "X":
               x = int.Parse(value);
               break;
            case "Y":
               y = int.Parse(value);
               break;
         }
      }
      Position = new PixelPoint(x, y);
      LoadRouterData();
   }

   public static void Save()
   {
      var lines = new List<string>();
      lines.Add($"SentMax{EqualChar}{SentMax}");
      lines.Add($"ReceivedMax{EqualChar}{ReceivedMax}");
      lines.Add($"CurrentNetwork{EqualChar}{CurrentNetwork}");
      lines.Add($"X{EqualChar}{Position.X}");
      lines.Add($"Y{EqualChar}{Position.Y}");
      File.WriteAllLines(settingsPath, lines);
   }

   private static void LoadRouterData()
   {
      const string AuthorizationHeader = $"Basic YWRtaW46YWRtaW4=";
      // const snr = /(?:SNR Margin Down Stream\s<\/td>\s<td class='form_label_col'>\s)(\d+(?:\.\d+))/gm;
      var downStreamReg = new Regex(@"(?:Down Stream\s<\/td>\s<td class='form_label_col'>\s)(\d+)(?: kbps)", RegexOptions.Multiline);
      var upStreamReg = new Regex(@"(?:Up Stream\s<\/td>\s<td class='form_label_col'>\s)(\d+)(?: kbps)", RegexOptions.Multiline);

      using var msg = new HttpRequestMessage(HttpMethod.Get, "http://192.168.1.1/adslconfig.htm");
      msg.Headers.Add("Authorization", AuthorizationHeader);
      using var client = new HttpClient();
      using var response = client.Send(msg);
      if (!response.IsSuccessStatusCode) return;
      using var content = response.Content;
      string str = content.ReadAsStringAsync().Result;

      if (int.TryParse(downStreamReg.Match(str).Groups[1].Value, out int downStream))
      {
         ReceivedMax = Math.Round(downStream / 1024.0, 1);
      };
      if (int.TryParse(upStreamReg.Match(str).Groups[1].Value, out int upStream))
      {
         SentMax = Math.Round(upStream / 1024.0, 1);
      };
   }
}