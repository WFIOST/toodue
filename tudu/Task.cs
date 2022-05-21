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
			Task t = YAML.TaskList.Tasks[p[0]];
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
			YAML.TaskList.Tasks.Add(item);
		YAML.SaveTodoFile();
	}

	public static Task GetTask(string pos, bool getParent = false)
	{
		int[] loc = StringToIntArray(pos);
		Task t = YAML.TaskList.Tasks[loc[0]];
		for (int i = 1; i < loc.Length; i++)
		{
			if (getParent && i == loc.Length - 1)
				break;
			t = t.SubTasks[loc[i]];
		}
		return t;
	}

	public static void SlashTask(string pos)
	{
		Task t = GetTask(pos);
		if (t.IsSlashed)
			return; 
		t.IsSlashed = true;
		YAML.SaveTodoFile();
	}

	public static void UnslashTask(string pos)
	{
		Task t = GetTask(pos);
		if (!t.IsSlashed)
			return;
		t.IsSlashed = false;
		YAML.SaveTodoFile();
	}

	public static void RemoveTask(string pos)
	{
		int index = StringToIntArray(pos)[-1];
		Task t = GetTask(pos, true);
		t.SubTasks.RemoveAt(index);
		YAML.SaveTodoFile();
	}
}