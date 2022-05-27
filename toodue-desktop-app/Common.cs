using toodue;

namespace toodue_desktop_app;

public static class Common
{
	public static string NameWithStrikethrough(this Task task)
	{
		if (task.IsSlashed)
			return $"~~{task.Name}~~";
		else
			return task.Name;
	}
}