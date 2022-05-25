using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace tudu;

public static class Serialisation
{
	public static readonly string TODO_FILE = Path.Combine(Path.Combine(Program.DATA_LOCATION, "todo.txt"));

	private static ISerializer s_serializer = new SerializerBuilder()
		.WithNamingConvention(CamelCaseNamingConvention.Instance)
		.Build();
	private static IDeserializer s_deserializer = new DeserializerBuilder()
		.WithNamingConvention(CamelCaseNamingConvention.Instance) 
		.Build();
	
	public static TodoFile LoadTodoFile()
	{
		var tasklist = s_deserializer.Deserialize<TodoFile>(File.ReadAllText(TODO_FILE));
		tasklist.Tasks ??= new List<Task>();
		return tasklist;
	}
	
	public static void SaveTodoFile(TodoFile td) => File.WriteAllText(TODO_FILE, s_serializer.Serialize(td));
}