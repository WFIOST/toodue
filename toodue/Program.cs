using System;
using System.Windows.Input;
using CommandLine;

namespace toodue;

internal class Program
{
	public const string APP_NAME = "toodue";

	public static readonly string DATA_LOCATION =
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APP_NAME);

	public static TodoFile Todo;

	public static void Main(string[] args)
	{
		Console.WriteLine(DATA_LOCATION);
		if (!Directory.Exists(DATA_LOCATION))
			Directory.CreateDirectory(DATA_LOCATION);
		if (!File.Exists(Path.Combine(DATA_LOCATION, "todo.txt")))
			File.Create(Path.Combine(DATA_LOCATION, "todo.txt")).Close();

		Todo = Serialisation.LoadTodoFile();

		Parser.Default.ParseArguments<RemoveTask, NewTask, TaskInfo, SlashTask, ListTasks, SyncAll>(args)
			.WithParsed<ICommand>(t => t.Execute());
	}
}

public interface ICommand
{
	void Execute();
}

[Verb("remove", HelpText = "Remove todo item.")]
public class RemoveTask : ICommand
{
	[Option('i', "id", Required = true, HelpText = "Todo item to slash.")]
	public string ID { get; set; }

	public void Execute()
	{
		Console.WriteLine($"Removing {ID}");
		Program.Todo.RemoveTask(ID);
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
		Program.Todo.SlashTask(ID);
	}
}

[Verb("info", HelpText = "Mark todo item as finished.")]
public class TaskInfo : ICommand
{
	[Option('i', "id", Required = true, HelpText = "Todo item to slash.")]
	public string ID { get; set; }

	public void Execute()
	{
		Task task = Program.Todo.GetTask(ID);

		if (task.IsSlashed)
			Console.WriteLine($"--{task.Name}--");
		else
			Console.WriteLine($"  {task.Name}");

		Console.WriteLine($"\n{task.Body}\n");

		int subcount = task.SubTasks?.Count ?? 0;

		Console.WriteLine($"Task has {subcount} subtasks.");
	}
}

[Verb("list", HelpText = "Lists all tasks.")]
public class ListTasks : ICommand
{
	public void Execute() => Console.WriteLine(Program.Todo.Tree());
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
		Program.Todo.AddTask(new Task
		{
			Name = Name,
			Body = Message
		}, ParentTask);
	}
}

[Verb("sync", HelpText = "Sync to selected provider.")]
public class SyncAll : ICommand
{
	[Option('f', "force", Required = true, HelpText = "Force a download/upload")]
	public string? Force { get; set; }

	public void Execute()
	{
		Sync.DropBox.Sync(Force!);
	}
}