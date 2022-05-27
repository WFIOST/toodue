using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using toodue;
using toodue_desktop_app.ViewModels;

namespace toodue_desktop_app.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void TreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
		{
			var t = MainWindowViewModel.SelectedTask = (e.AddedItems[0] as MainWindowViewModel.ObservableTask);
			MainWindowViewModel.mwvm.NameEditor = t.Task.Name;
			MainWindowViewModel.mwvm.BodyEditor = t.Task.Body;
		}
	}
}