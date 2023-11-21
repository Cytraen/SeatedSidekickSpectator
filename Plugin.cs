using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;
using SeatedSidekickSpectator.Windows;
using CharacterModes = FFXIVClientStructs.FFXIV.Client.Game.Character.Character.CharacterModes;
using CharacterStruct = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace SeatedSidekickSpectator;

internal class Plugin : IDalamudPlugin
{
	private const string ConfigWindowCommandName = "/sss";

	public Plugin(DalamudPluginInterface pluginInterface)
	{
		pluginInterface.Create<Services>();

		Services.Config = Services.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

		Services.ConfigWindow = new ConfigWindow();
		Services.PassengerListWindow = new PassengerListWindow();
		Services.WindowSystem.AddWindow(Services.ConfigWindow);
		Services.WindowSystem.AddWindow(Services.PassengerListWindow);

		Services.CommandManager.AddHandler(ConfigWindowCommandName, new CommandInfo(OnConfigWindowCommand)
		{
			HelpMessage = "Opens the Seated Sidekick Spectator config window."
		});

		Services.PluginInterface.UiBuilder.Draw += DrawUi;
		Services.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;

		Services.Condition.ConditionChange += OnConditionChange;

		InitMountMembers();
		Services.SetModeHook = new SetModeHook();
		OnConditionChange(ConditionFlag.Mounted, Services.Condition[ConditionFlag.Mounted]);
	}

	public void OnConditionChange(ConditionFlag condition, bool value)
	{
		if (condition is not ConditionFlag.Mounted)
		{
			return;
		}
		if (!value)
		{
			Services.PassengerListWindow.IsOpen = false;
			return;
		}

		var seats = Helpers.GetNumberOfSeats() ?? 7;

		Services.PassengerListWindow.IsOpen = seats != 0 && Services.Config.ShowPassengerListWindow;
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
				var passengerName = character.Name.TextValue;
				var passengerWorldName = Services.DataManager.GetExcelSheet<World>()
					?.GetRow(charStruct->HomeWorld)?.Name.ToString();

				var passengerNameString = passengerName + (char)SeIconChar.CrossWorld + passengerWorldName;

				Services.MountMembers[charStruct->ModeParam] = new Tuple<uint, string>(charStruct->GameObject.GetObjectID().ObjectID, passengerNameString);
			}
		}
	}

	public void Dispose()
	{
		Services.SetModeHook.Dispose();

		Services.CommandManager.RemoveHandler(ConfigWindowCommandName);
		Services.WindowSystem.RemoveAllWindows();
		Services.PassengerListWindow.Dispose();
		Services.ConfigWindow.Dispose();

		Services.PluginInterface.UiBuilder.Draw -= DrawUi;
		Services.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUi;
	}

	private static void OnConfigWindowCommand(string command, string args)
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

	private static void DrawUi()
	{
		Services.WindowSystem.Draw();
	}

	private static void DrawConfigUi()
	{
		Services.ConfigWindow.IsOpen = true;
	}
}
