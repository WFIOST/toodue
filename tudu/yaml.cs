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