using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace tudu;

public static class yaml
{
	public static ISerializer serializer = new SerializerBuilder()
		.WithNamingConvention(CamelCaseNamingConvention.Instance)
		.Build();
	public static IDeserializer deserializer = new DeserializerBuilder()
		.WithNamingConvention(CamelCaseNamingConvention.Instance) 
		.Build();
	public static TodoFile TaskList;
	public static readonly string TODO_FILE = Path.Combine(Path.Combine(Program.DataLoc, "todo.txt"));
	
	
	//converts something like "0-10-6-8" to [0, 10, 6, 8]
	public static int[] StringToIntArray(string str)
	{
		string[] stra = str.Split('-');
		int[] inta = new int[stra.Length];
		for (int i = 0; i < stra.Length; i++)
			inta[i] = int.Parse(stra[i]); 
		return inta;
	}
	
	public static void AddTask(Task item, string? parent = null)
	{
		if (parent != null)
		{
			int[] p = StringToIntArray(parent);
			Task t = TaskList.Tasks[p[0]];
			for (int i = 1; i < p.Length; i++)
			{
				if (p[i] >= t.SubTasks.Count)
				{
					//TODO: error log
					return;
				}
				t = t.SubTasks[p[i]];
			}
			if (t.SubTasks == null) t.SubTasks = new List<Task>();
			t.SubTasks.Add(item);
		}
		else
			TaskList.Tasks.Add(item);
		SaveTodoFile();
	}

	public static Task GetTask(string ID)
	{
		int id = int.Parse(ID);
		return TaskList.Tasks[id];
	}

	public static void SlashTask(int index)
	{
		if (TaskList.Tasks[index].IsSlashed)
		{
			Console.WriteLine("Item is already slashed!");
			return;
		}
		TaskList.Tasks[index].IsSlashed = true;
		SaveTodoFile();
	}

	public static void RemoveTask(int index)
	{
		TaskList.Tasks.RemoveAt(index);
		SaveTodoFile();
	}

	public static void LoadTodoFile()
	{
		string todotxt = File.ReadAllText(TODO_FILE);
		TaskList = deserializer.Deserialize<TodoFile>(todotxt);
		if (TaskList == null)
			TaskList = new TodoFile();
		if (TaskList.Tasks == null)
			TaskList.Tasks = new List<Task>();
	}
	
	public static void SaveTodoFile()
	{
		var yml = serializer.Serialize(TaskList);
		File.WriteAllText(TODO_FILE, yml);
	}
}

public class TodoFile
{
	public List<Task> Tasks { get; set; }
}

public class Task
{
	public string Name { get; set; }
	public string Body { get; set; }
	public bool IsSlashed { get; set; }
	public List<Task> SubTasks { get; set; }
}