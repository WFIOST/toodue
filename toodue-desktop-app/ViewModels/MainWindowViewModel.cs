using System;
using System.Collections.Generic;
using System.Text;

namespace toodue_desktop_app.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public string Greeting => "Toodue!";
		public string[] Data => new string[] { "- Do Work", "- Gamer Time", "- Troll Actively" };
	}
}