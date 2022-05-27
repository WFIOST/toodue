using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using toodue;

namespace toodue_desktop_app.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public string                     Greeting      => "Toodue!";

		public static ObservableCollection<ObservableTask> TaskList      { get; set; }
		public static ObservableCollection<ObservableTask> TaskListSelectedItems { get;}
		public static TodoFile                             tf;

		public void NewRootTask()
		{
			Task t = new Task();
			t.Name = "New Task";
			tf.AddTask(t);
			TaskList.Add(new ObservableTask(t));
		}

		public MainWindowViewModel()
		{
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