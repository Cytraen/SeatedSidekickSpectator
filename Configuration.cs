using Dalamud.Configuration;

namespace SeatedSidekickSpectator;

[Serializable]
public class Configuration : IPluginConfiguration
{
	public int Version { get; set; } = 0;

	public bool ShowToastNotifications { get; set; } = true;

	public bool ShowChatNotifications { get; set; } = false;

	public bool ShowListWindow { get; set; } = false;

	public void Save()
	{
		Services.PluginInterface.SavePluginConfig(this);
	}
}
