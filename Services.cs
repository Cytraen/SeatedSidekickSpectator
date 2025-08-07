using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace SeatedSidekickSpectator;

internal sealed class Services
{
	[PluginService]
	public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

	[PluginService]
	public static IClientState ClientState { get; private set; } = null!;

	[PluginService]
	public static ICommandManager CommandManager { get; private set; } = null!;

	[PluginService]
	public static IChatGui ChatGui { get; private set; } = null!;

	[PluginService]
	public static IToastGui ToastGui { get; private set; } = null!;

	[PluginService]
	public static ICondition Condition { get; private set; } = null!;

	[PluginService]
	public static IPluginLog PluginLog { get; private set; } = null!;

	[PluginService]
	public static IFramework Framework { get; private set; } = null!;

	[PluginService]
	public static IObjectTable ObjectTable { get; private set; } = null!;

	[PluginService]
	public static ISigScanner SigScanner { get; private set; } = null!;

	[PluginService]
	public static IDataManager DataManager { get; private set; } = null!;

	[PluginService]
	public static IGameInteropProvider GameInteropProvider { get; private set; } = null!;

	internal static Configuration Config = null!;

	internal static SetModeHook SetModeHook = null!;

	internal static readonly Dictionary<byte, Tuple<uint, string>> MountMembers = [];
}
