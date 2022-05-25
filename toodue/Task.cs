namespace toodue;

public struct Task
{
	public string Name { get; set; }
	public string Body { get; set; }
	public bool IsSlashed { get; set; }
	public List<Task> SubTasks { get; set; }
}

public class TodoFile
{
	public List<Task>? Tasks { get; set; }

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
