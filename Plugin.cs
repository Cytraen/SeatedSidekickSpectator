using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using SeatedSidekickSpectator.Windows;

namespace SeatedSidekickSpectator;

internal class Plugin : IDalamudPlugin
{
	private const string ConfigWindowCommandName = "/sss";
	private readonly WindowSystem _windowSystem;
	private readonly ConfigWindow _configWindow;
	private readonly MountHook _mountHook;
	private readonly DismountHook _dismountHook;

	public Plugin(DalamudPluginInterface pluginInterface)
	{
		pluginInterface.Create<Services>();

		Services.Config = Services.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

		_windowSystem = new WindowSystem("SeatedSidekickSpectator");
		_configWindow = new ConfigWindow();
		_windowSystem.AddWindow(_configWindow);

		Services.CommandManager.AddHandler(ConfigWindowCommandName, new CommandInfo(OnConfigWindowCommand)
		{
			HelpMessage = "Opens the Seated Sidekick Spectator config window."
		});

		Services.Condition.ConditionChange += OnConditionChange;
		Services.PluginInterface.UiBuilder.Draw += DrawUi;
		Services.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;

		_mountHook = new MountHook();
		_dismountHook = new DismountHook();
	}

	public void OnConditionChange(ConditionFlag flag, bool state)
	{
		if (flag is not (ConditionFlag.Mounted or ConditionFlag.Mounted2))
		{
			return;
		}
		Services.MountMembers.Clear();
		if (state)
		{
			_mountHook.Enable();
			_dismountHook.Enable();
			Services.PluginLog.Debug("Started listening for mounts/dismounts");
		}
		else
		{
			Services.PluginLog.Debug("Stopped listening for mounts/dismounts");
			_mountHook.Disable();
			_dismountHook.Disable();
		}
	}

	public void Dispose()
	{
		_mountHook.Dispose();
		_dismountHook.Dispose();

		Services.CommandManager.RemoveHandler(ConfigWindowCommandName);
		_windowSystem.RemoveAllWindows();
		_configWindow.Dispose();

		Services.Condition.ConditionChange -= OnConditionChange;
		Services.PluginInterface.UiBuilder.Draw -= DrawUi;
		Services.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUi;
	}

	private void OnConfigWindowCommand(string command, string args)
	{
		switch (args)
		{
			case "":
				DrawConfigUi();
				break;

			default:
				Services.ChatGui.PrintError($"Unknown command: '/{command} {args}'");
				break;
		}
	}

	private void DrawUi()
	{
		_windowSystem.Draw();
	}

	private void DrawConfigUi()
	{
		_configWindow.IsOpen = true;
	}
}
