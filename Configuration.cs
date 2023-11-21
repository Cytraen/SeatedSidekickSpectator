using Dalamud.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SeatedSidekickSpectator;

[Serializable]
public class Configuration : IPluginConfiguration
{
	public int Version { get; set; } = 0;

	[JsonInclude]
	public bool ShowToastNotifications = true;

	[JsonInclude]
	public bool ShowChatNotifications = false;

	[JsonInclude]
	public bool ShowPassengerListWindow = false;

	[JsonInclude]
	public bool PassengerListWindowLocked = false;

	private static string FilePath => Services.PluginInterface.ConfigFile.FullName;

	public static void Load()
	{
		if (File.Exists(FilePath))
		{
			Services.Config = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(FilePath))!;
		}
		else
		{
			Services.Config = new Configuration();

			Services.Config.Save();
		}
	}

	public void Save()
	{
		Services.PluginInterface.SavePluginConfig(this);
	}
}
