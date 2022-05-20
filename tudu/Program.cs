using System;
using System.Windows.Input;
using CommandLine;

namespace tudu
{
	internal class Program
	{
		public static string AppName = "tudu";

		public static string DataLoc =
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);

		static void Main(string[] args)
		{
			Console.WriteLine(DataLoc);
			if (!Directory.Exists(DataLoc))
				Directory.CreateDirectory(DataLoc);
			if (!File.Exists(Path.Combine(DataLoc, "todo.txt")))
				File.Create(Path.Combine(DataLoc, "todo.txt"));
			yaml.LoadTodoFile();

			Parser.Default.ParseArguments<RemTask, NewTask, TaskInfo, SlashTasks, ListTasks>(args)
				.WithParsed<ICommand>(t => t.Execute());
		}
	}

	public interface ICommand
	{
		void Execute();
	}

	[Verb("rem", HelpText = "Remove todo item.")]
	public class RemTask : ICommand
	{
		[Option('i', "id", Required = true, HelpText = "Todo item to slash.")]
		public string ID { get; set; }

		public void Execute()
		{
			Console.WriteLine($"Removing {ID}");
			yaml.RemoveTask(int.Parse(ID));
		}
	}

	[Verb("slash", HelpText = "Mark todo item as finished.")]
	public class SlashTasks : ICommand
	{
		[Option('i', "id", Required = true, HelpText = "Todo item to slash.")]
		public string ID { get; set; }

		public void Execute()
		{
			Console.WriteLine($"Slashing {ID}");
			yaml.SlashTask(int.Parse(ID));
		}
	}

	[Verb("info", HelpText = "Mark todo item as finished.")]
	public class TaskInfo : ICommand
	{
		[Option('i', "id", Required = true, HelpText = "Todo item to slash.")]
		public string ID { get; set; }

		public void Execute()
		{
			Task task = yaml.GetTask(ID);
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
			for (int i = 0; i < yaml.TaskList.Tasks.Count; i++)
			{
				char s = ' ';
				if (yaml.TaskList.Tasks[i].IsSlashed) s = '-';
				Console.Write($"{i}: {yaml.TaskList.Tasks[i].Name} {s}\n");
				string? sti = GetSubTaskInfo(yaml.TaskList.Tasks[i], i.ToString(), 1);
				if(sti != null) Console.Write(sti);
			}
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
			yaml.AddTask(task, ParentTask);
		}
	}
}