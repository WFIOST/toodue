namespace tudu;

public class TodoFile
{
	public List<Task> Tasks { get; set; }
}

public class Task
{
	public string     Name      { get; set; }
	public string     Body      { get; set; }
	public bool       IsSlashed { get; set; }
	public List<Task> SubTasks  { get; set; }
	
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
			Task t = yaml.TaskList.Tasks[p[0]];
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
			yaml.TaskList.Tasks.Add(item);
		yaml.SaveTodoFile();
	}

	public static Task GetTask(string ID)
	{
		int id = int.Parse(ID);
		return yaml.TaskList.Tasks[id];
	}

	public static void SlashTask(int index)
	{
		if (yaml.TaskList.Tasks[index].IsSlashed)
		{
			Console.WriteLine("Item is already slashed!");
			return;
		}
		yaml.TaskList.Tasks[index].IsSlashed = true;
		yaml.SaveTodoFile();
	}

	public static void RemoveTask(int index)
	{
		yaml.TaskList.Tasks.RemoveAt(index);
		yaml.SaveTodoFile();
	}
}