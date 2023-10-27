using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace SeatedSidekickSpectator;

internal class Services
{
	[PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;

	[PluginService] public static IClientState ClientState { get; private set; } = null!;

	[PluginService] public static ICommandManager CommandManager { get; private set; } = null!;

	[PluginService] public static IChatGui ChatGui { get; private set; } = null!;

	[PluginService] public static IToastGui ToastGui { get; private set; } = null!;

	[PluginService] public static IPluginLog PluginLog { get; private set; } = null!;

	[PluginService] public static IFramework Framework { get; private set; } = null!;

	[PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;

	[PluginService] public static ISigScanner SigScanner { get; private set; } = null!;

	[PluginService] public static IDataManager DataManager { get; private set; } = null!;

	[PluginService] public static IGameInteropProvider GameInteropProvider { get; private set; } = null!;

	public static Configuration Config { get; internal set; } = null!;

	public static Dictionary<byte, uint> MountMembers { get; internal set; } = new();
}
