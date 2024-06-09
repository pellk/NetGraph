using System.Globalization;
using System.Net.Sockets;

public class RouterStatus
{
	public static void SetSpeedSetting(RouterInfo router)
	{
		string html = Fetch(router).Result;
		if (html is null) return;

		if (int.TryParse(router.DownStreamReg.Match(html).Groups[1].Value, CultureInfo.InvariantCulture, out int downStream))
			Settings.ReceivedMax = Math.Round(downStream / 1024.0, 1);

		if (int.TryParse(router.UpStreamReg.Match(html).Groups[1].Value, CultureInfo.InvariantCulture, out int upStream))
			Settings.SentMax = Math.Round(upStream / 1024.0, 1);
	}

	public static float? GetSnr(RouterInfo router)
	{
		string html = Fetch(router).Result;
		if (html is null) return null;

		if (int.TryParse(router.DownStreamReg.Match(html).Groups[1].Value, CultureInfo.InvariantCulture, out int downStream))
			Settings.ReceivedMax = Math.Round(downStream / 1024.0, 1);

		if (int.TryParse(router.UpStreamReg.Match(html).Groups[1].Value, CultureInfo.InvariantCulture, out int upStream))
			Settings.SentMax = Math.Round(upStream / 1024.0, 1);

		if (float.TryParse(router.SnrReg.Match(html).Groups[1].Value, CultureInfo.InvariantCulture, out float snr))
			return snr;

		return 0;
	}

	private async static Task<string> Fetch(RouterInfo router)
	{
		using var cancelTokenSource = new CancellationTokenSource();
		cancelTokenSource.CancelAfter(200);
		using var msg = new HttpRequestMessage(HttpMethod.Get, router.Url);
		if (!string.IsNullOrWhiteSpace(router.AuthorizationHeader))
			msg.Headers.Add("Authorization", router.AuthorizationHeader);
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
		catch (SocketException ex)
		{
			return null;
		}
		catch (Exception ex)
		{
			return null;
		}
		/* System.AggregateException: One or more errors occurred. (Cannot access a disposed object.
Object name: 'System.Net.Http.HttpConnectionResponseContent'.) */
	}
}
