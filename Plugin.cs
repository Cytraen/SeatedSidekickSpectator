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
	private readonly SetModeHook _setModeHook;

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

		Services.PluginInterface.UiBuilder.Draw += DrawUi;
		Services.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;

		_setModeHook = new SetModeHook();
	}

	public void Dispose()
	{
		_setModeHook.Dispose();

		Services.CommandManager.RemoveHandler(ConfigWindowCommandName);
		_windowSystem.RemoveAllWindows();
		_configWindow.Dispose();

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
