using System.Text.RegularExpressions;

public record RouterInfo(
	string Label,
	string Url,
	string AuthorizationHeader,
	Regex SnrReg,
	Regex DownStreamReg,
	Regex UpStreamReg
);

public class Routers
{
	private static List<RouterInfo> _routers = new(){
		new(
			Label: "D-Link",
			Url: "http://192.168.1.1/adslconfig.htm",
			AuthorizationHeader: "Basic YWRtaW46YWRtaW4=",
			SnrReg: new Regex(@"(?:SNR Margin Down Stream\s<\/td>\s<td class='form_label_col'>\s)(\d+(?:\.\d+))", RegexOptions.Multiline),
			DownStreamReg: new Regex(@"(?:Down Stream\s<\/td>\s<td class='form_label_col'>\s)(\d+)(?: kbps)", RegexOptions.Multiline),
			UpStreamReg: new Regex(@"(?:Up Stream\s<\/td>\s<td class='form_label_col'>\s)(\d+)(?: kbps)", RegexOptions.Multiline)
		),
		new(
			Label: "DataSheen",
			Url: "http://192.168.1.1/adslconfig.htm",
			AuthorizationHeader: null,
			SnrReg: new Regex(@"(?:<th>SNR Margin Down Stream\s<\/th>\s+<td>\s+)(\d+(?:\.\d+))", RegexOptions.Multiline),
			DownStreamReg: new Regex(@"(?:<th>Down Stream<\/th>\s+<td>\s+)(\d+)(?: kbps)", RegexOptions.Multiline),
			UpStreamReg: new Regex(@"(?:<th>Up Stream<\/th>\s+<td>\s+)(\d+)(?: kbps)", RegexOptions.Multiline)
		)
	};

	public static List<RouterInfo> List => _routers;
}