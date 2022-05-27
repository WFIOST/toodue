using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using ReactiveUI;
using toodue;

namespace toodue_desktop_app.ViewModels
{
	public class MainWindowViewModel : ReactiveObject
	{
		public string                     Greeting      => "Toodue!";

		public static MainWindowViewModel mwvm;
		public static ObservableCollection<ObservableTask> TaskList      { get; set; }
		public static ObservableCollection<ObservableTask> TaskListSelectedItems { get;}
		public static TodoFile                             tf;
		public static ObservableTask SelectedTask;

		private string nameEditor;
		public string NameEditor
		{
			get => nameEditor;
			set
			{
				SelectedTask.Task.Name = value;
				this.RaiseAndSetIfChanged(ref nameEditor, value);
			}
		}
		
		private string bodyEditor;
		public string BodyEditor
		{
			get => bodyEditor;
			set
			{
				SelectedTask.Task.Body = value;
				this.RaiseAndSetIfChanged(ref bodyEditor, value);
			}
		}

		public void NewRootTask()
		{
			Task t = new Task();
			t.Name = "New Task";
			tf.AddTask(t);
			TaskList.Add(new ObservableTask(t));
		}

		public MainWindowViewModel()
		{
			mwvm = this;
			tf = toodue.Serialisation.LoadTodoFile();
			tf.Tree();
			TaskList = new ObservableCollection<ObservableTask>();
			foreach (var task in tf.Tasks)
				TaskList.Add(new ObservableTask(task));
		}
		

		public class ObservableTask
		{
			public ObservableTask(Task t)
			{
				Task = t;
				Subt = new ObservableCollection<ObservableTask>();
				SetSubtasks();
			}

			private void SetSubtasks()
			{
				if (Task.SubTasks != null)
				{
					foreach (var task in Task.SubTasks)
						Subt.Add(new ObservableTask(task));
				}
			}
			public Task                                 Task { get; set; }
			public ObservableCollection<ObservableTask> Subt { get; set; }
			public void AddEmptySubtask(string name = "New Task")
			{
				Subt.Add(new ObservableTask(Task.AddEmptySubtask()));
				Serialisation.SaveTodoFile(tf);
			}
		}
	}
}