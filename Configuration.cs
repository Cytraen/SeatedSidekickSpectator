using System.Text.Json;

namespace SeatedSidekickSpectator;

public sealed class Configuration
{
	public bool ShowToastNotifications { get; set; } = true;

	public bool ShowChatNotifications { get; set; } = false;

	public bool ShowPassengerListWindow { get; set; } = false;

	private static string FilePath => Services.PluginInterface.ConfigFile.FullName;

	public static Configuration Load()
	{
		if (!File.Exists(Services.PluginInterface.ConfigFile.FullName))
		{
			return new Configuration();
		}

		var bytes = File.ReadAllBytes(Services.PluginInterface.ConfigFile.FullName);
		return JsonSerializer.Deserialize<Configuration>(bytes) ?? new Configuration();
	}

	public static void Save(Configuration config)
	{
		config.Save();
	}

	public void Save()
	{
		var str = JsonSerializer.Serialize(this);
		File.WriteAllText(Services.PluginInterface.ConfigFile.FullName, str);
	}
}
