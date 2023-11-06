using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using SeatedSidekickSpectator.Windows;
using CharacterModes = FFXIVClientStructs.FFXIV.Client.Game.Character.Character.CharacterModes;
using CharacterStruct = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

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

		Services.ClientState.TerritoryChanged += OnTerritoryChanged;
		_setModeHook = new SetModeHook();
		InitMountMembers();
	}

	public unsafe void InitMountMembers()
	{
		if (Services.ClientState.LocalPlayer is null || ((CharacterStruct*)Services.ClientState.LocalPlayer.Address)->Mode != CharacterModes.Mounted) return;

		for (var i = 0; i < 100; i++)
		{
			if (Services.ObjectTable[i * 2] is not Character character) continue;

			var charStruct = (CharacterStruct*)character.Address;
			if (charStruct->Mode == CharacterModes.RidingPillion
				&& charStruct->GameObject.OwnerID == Services.ClientState.LocalPlayer.ObjectId)
			{
				Services.MountMembers[charStruct->ModeParam] = charStruct->GameObject.GetObjectID().ObjectID;
			}
		}
	}

	public unsafe void OnTerritoryChanged(ushort _)
	{
		_setModeHook.Disable();
		Services.Framework.RunOnTick(() =>
		{
			for (byte i = 1; i < 8; i++)
			{
				if (!Services.MountMembers.TryGetValue(i, out var objId)) continue;

				if (Services.ObjectTable.SearchById(objId) is not Character passenger ||
					((CharacterStruct*)passenger.Address)->Mode != CharacterModes.RidingPillion)
				{
					Services.MountMembers.Remove(i);
				}
			}
		}, delayTicks: 0);
	}

	public void Dispose()
	{
		Services.ClientState.TerritoryChanged -= OnTerritoryChanged;
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
