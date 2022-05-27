namespace toodue;

public class Task
{
	public string     Name      { get; set; }
	public string     Body      { get; set; }
	public bool       IsSlashed { get; set; }
	public List<Task> SubTasks  { get; set; }
	
	[YamlDotNet.Serialization.YamlIgnore]
	public string     NameMD    => IsSlashed ? $"~~{Name}~~" : Name;
	[YamlDotNet.Serialization.YamlIgnore]
	public string ID { get; set; }

	public Task AddEmptySubtask(string name = "New Task")
	{
		Task t = new Task();
		t.Name = name;
		if (SubTasks == null)
			SubTasks = new List<Task>();
		SubTasks.Add(t);
		return t;
	}
}

public class TodoFile
{
	public List<Task>? Tasks { get; set; }

	public string Tree()
	{
		string list = "";
		for (int i = 0; i < Tasks?.Count; i++)
		{
			char s = ' ';
			if (Tasks[i].IsSlashed) s = '-';
			list += $"{i}: {Tasks[i].Name} {s}\n";
			Tasks[i].ID = i.ToString();
			string sti = GetSubTaskInfo(Tasks[i], i.ToString(), 1);
			list += sti;
		}
		return list;
	}
	
	public static string GetSubTaskInfo(Task toptask, string parent, int depth)
	{
		if (toptask.SubTasks == null || toptask.SubTasks.Count == 0) return String.Empty;
		var str = String.Empty;
		for (int i = 0; i < toptask.SubTasks.Count; i++)
		{
			var padding = String.Empty;
			var s = String.Empty;

			for (int x = 0; x < depth; x++) padding += "   ";

			if (toptask.SubTasks[i].IsSlashed) s += "-";
			str += $"{padding}{parent}-{i}: {toptask.SubTasks[i].Name} {s}\n";
			toptask.SubTasks[i].ID = $"{parent}-{i}";
			string sti = GetSubTaskInfo(toptask.SubTasks[i], $"{parent}-{i}", depth + 1);
			str += sti;
		}
		return str;
	}

	public void AddTask(Task item, string? parent = null)
	{
		if (parent != null)
		{
			int[] p = parent.ToIntArray();
			Task t = Tasks![p[0]];
			for (var i = 1; i < p.Length; i++)
			{
				if (p[i] >= t.SubTasks.Count)
				{
					//TODO: error log
					return;
				}
				t = t.SubTasks[p[i]];
			}

			if (t.SubTasks == null)
				t.SubTasks = new List<Task>();
			t.SubTasks.Add(item);
		}
		else
			Tasks!.Add(item);
		Serialisation.SaveTodoFile(this);
	}

	public Task GetTask(string pos, bool getParent = false)
	{
		int[] loc = pos.ToIntArray();
		Task t = Tasks![loc[0]];
		for (int i = 1; i < loc.Length; i++)
		{
			if (getParent && i == loc.Length - 1)
				break;
			t = t.SubTasks[loc[i]];
		}
		return t;
	}

	public void SlashTask(string pos)
	{
		Task t = GetTask(pos);
		if (t.IsSlashed)
			return;
		t.IsSlashed = true;
		Serialisation.SaveTodoFile(this);
	}

	public void UnslashTask(string pos)
	{
		Task t = GetTask(pos);
		if (!t.IsSlashed)
			return;
		t.IsSlashed = false;
		Serialisation.SaveTodoFile(this);
	}

	public void RemoveTask(string pos)
	{
		int index = pos.ToIntArray()[-1];
		Task t = GetTask(pos, true);
		t.SubTasks.RemoveAt(index);
		Serialisation.SaveTodoFile(this);
	}
}
