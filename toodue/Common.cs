using System.Diagnostics;
using System.Runtime.InteropServices;

namespace toodue;

public static class Common
{
	//converts something like "0-10-6-8" to [0, 10, 6, 8]
	public static int[] StringToIntArray(string str)
	{
		string[] stra = str.Split('-');
		int[] inta = new int[stra.Length];
		for (int i = 0; i < stra.Length; i++)
			inta[i] = int.Parse(stra[i]);
		return inta;
	}

	public static void OpenURL(string url)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			Process.Start(new ProcessStartInfo(url.Replace("&", "^&")) { UseShellExecute = true });
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			Process.Start("xdg-open", url);
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			Process.Start("open", url);
	}
}

public static class Extensions
{
	public static int[] ToIntArray(this string str) => Common.StringToIntArray(str);
}