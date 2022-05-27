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

		private MainWindowViewModel.ObservableTask t;
		private void SetTaskName(object? sender, KeyEventArgs e)
		{
			Debug.WriteLine("Hallo!");
			t.Task.Name = (sender as TextBox).Text;
		}
		
		private void TreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
		{
			t = (e.AddedItems[0] as MainWindowViewModel.ObservableTask);
		}
	}
}