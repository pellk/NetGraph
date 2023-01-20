using System.Text.RegularExpressions;
public class RouterStatus
{
   private const string AuthorizationHeader = $"Basic YWRtaW46YWRtaW4=";
   private const string Url = "http://192.168.1.1/adslconfig.htm";
   private static readonly Regex snrReg = new(@"(?:SNR Margin Down Stream\s<\/td>\s<td class='form_label_col'>\s)(\d+(?:\.\d+))", RegexOptions.Multiline);
   private static readonly Regex downStreamReg = new(@"(?:Down Stream\s<\/td>\s<td class='form_label_col'>\s)(\d+)(?: kbps)", RegexOptions.Multiline);
   private static readonly Regex upStreamReg = new(@"(?:Up Stream\s<\/td>\s<td class='form_label_col'>\s)(\d+)(?: kbps)", RegexOptions.Multiline);

   public static void SetSpeedSetting()
   {
      string html = Fetch().Result;
      if (html is null) return;

      if (int.TryParse(downStreamReg.Match(html).Groups[1].Value, out int downStream))
         Settings.ReceivedMax = Math.Round(downStream / 1024.0, 1);

      if (int.TryParse(upStreamReg.Match(html).Groups[1].Value, out int upStream))
         Settings.SentMax = Math.Round(upStream / 1024.0, 1);
   }

   public static float? GetSnr()
   {
      string html = Fetch().Result;
      if (html is null) return null;

      if (int.TryParse(downStreamReg.Match(html).Groups[1].Value, out int downStream))
         Settings.ReceivedMax = Math.Round(downStream / 1024.0, 1);

      if (int.TryParse(upStreamReg.Match(html).Groups[1].Value, out int upStream))
         Settings.SentMax = Math.Round(upStream / 1024.0, 1);

      if (float.TryParse(snrReg.Match(html).Groups[1].Value, out float snr))
         return snr;

      return 0;
   }

   private async static Task<string> Fetch()
   {
      using var cancelTokenSource = new CancellationTokenSource();
      cancelTokenSource.CancelAfter(200);
      using var msg = new HttpRequestMessage(HttpMethod.Get, Url);
      msg.Headers.Add("Authorization", AuthorizationHeader);
      using var client = new HttpClient();
      try
      {
         using var response = client.Send(msg, cancelTokenSource.Token);
         if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync();
         return null;
      }
      catch (TaskCanceledException ex)
      {
         return null;
      }
   }
}