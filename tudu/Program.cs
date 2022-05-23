using System;
using System.Windows.Input;
using CommandLine;

namespace tudu
{
	internal class Program
	{
		public static string AppName = "toodue";

		public static string DataLoc =
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);

		static void Main(string[] args)
		{
			Console.WriteLine(DataLoc);
			if (!Directory.Exists(DataLoc))
				Directory.CreateDirectory(DataLoc);
			if (!File.Exists(Path.Combine(DataLoc, "todo.txt")))
				File.Create(Path.Combine(DataLoc, "todo.txt")).Close();
			YAML.LoadTodoFile();

			Parser.Default.ParseArguments<RemTask, NewTask, TaskInfo, SlashTask, ListTasks, SyncAll>(args)
				.WithParsed<ICommand>(t => t.Execute());
		}
	}

	public interface ICommand { void Execute(); }

	[Verb("rem", HelpText = "Remove todo item.")]
	public class RemTask : ICommand
	{
		[Option('i', "id", Required = true, HelpText = "Todo item to slash.")]
		public string ID { get; set; }

		public void Execute()
		{
			Console.WriteLine($"Removing {ID}");
			Task.RemoveTask(ID);
		}
	}

	[Verb("slash", HelpText = "Mark todo item as finished.")]
	public class SlashTask : ICommand
	{
		[Option('i', "id", Required = true, HelpText = "Todo item to slash.")]
		public string ID { get; set; }

		public void Execute()
		{
			Console.WriteLine($"Slashing {ID}");
			Task.SlashTask(ID);
		}
	}

	[Verb("info", HelpText = "Mark todo item as finished.")]
	public class TaskInfo : ICommand
	{
		[Option('i', "id", Required = true, HelpText = "Todo item to slash.")]
		public string ID { get; set; }

		public void Execute()
		{
			Task task = Task.GetTask(ID);
			if(task.IsSlashed)
				Console.WriteLine($"--{task.Name}--");
			else
				Console.WriteLine($"  {task.Name}");
			Console.WriteLine($"\n{task.Body}\n");

			int subcount = (task.SubTasks != null) ? task.SubTasks.Count : 0;
			
			Console.WriteLine($"Task has {subcount} subtasks.");
		}
	}

	[Verb("list", HelpText = "Lists all tasks.")]
	public class ListTasks : ICommand
	{
		public void Execute()
		{
			string list = "";
			for (int i = 0; i < YAML.TaskList.Tasks.Count; i++)
			{
				char s = ' ';
				if (YAML.TaskList.Tasks[i].IsSlashed) s = '-';
				list += $"{i}: {YAML.TaskList.Tasks[i].Name} {s}\n";
				string? sti = GetSubTaskInfo(YAML.TaskList.Tasks[i], i.ToString(), 1);
				if (sti != null) list += sti;
			}
			Console.WriteLine(list);
		}

		public static string? GetSubTaskInfo(Task toptask, string parent, int depth)
		{
			if (toptask.SubTasks == null || toptask.SubTasks.Count == 0) return null;
			string? str = "";
			for (int i = 0; i < toptask.SubTasks.Count; i++)
			{
				string padding = "";
				for (int x = 0; x < depth; x++) padding += "   ";
				string s = "";
				if (toptask.SubTasks[i].IsSlashed) s += "-";
				str += $"{padding}{parent}-{i}: {toptask.SubTasks[i].Name} {s}\n";
				string? sti = GetSubTaskInfo(toptask.SubTasks[i], $"{parent}-{i}", depth + 1);
				if (sti != null) str +=  sti;
			}
			return str;
		}
	}

	[Verb("new", HelpText = "Create new todo item.")]
	public class NewTask : ICommand
	{
		[Option('n', "name", Required = true, HelpText = "Name of the todo message")]
		public string Name { get; set; }

		[Option('m', "message", Required = false, HelpText = "Caption of the todo message")]
		public string Message { get; set; }

		[Option('p', "parent", Required = false, HelpText = "Put as a subtask of this task.")]
		public string? ParentTask { get; set; }

		public void Execute()
		{
			var task = new Task();
			task.Name = Name;
			task.Body = Message;
			Task.AddTask(task, ParentTask);
		}
	}
	
	[Verb("sync", HelpText = "Sync to selected provider.")]
	public class SyncAll : ICommand
	{
		public void Execute()
		{
			Sync.DropBox.OAuth2();
			Sync.DropBox.r();
		}
	}
}