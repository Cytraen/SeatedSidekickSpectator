using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;

namespace SeatedSidekickSpectator;

internal class Plugin : IDalamudPlugin
{
	private const string ConfigWindowCommandName = "/sss";

	public Plugin(IDalamudPluginInterface pluginInterface)
	{
		pluginInterface.Create<Services>();

		Services.Config =
			Services.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

		Services.ConfigWindow = new();
		Services.PassengerListWindow = new();
		Services.WindowSystem.AddWindow(Services.ConfigWindow);
		Services.WindowSystem.AddWindow(Services.PassengerListWindow);

		Services.CommandManager.AddHandler(
			ConfigWindowCommandName,
			new(OnConfigWindowCommand)
			{
				HelpMessage = "Opens the Seated Sidekick Spectator config window.",
			}
		);

		Services.PluginInterface.UiBuilder.Draw += DrawUi;
		Services.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;

		Services.Condition.ConditionChange += OnConditionChange;

		Services.Framework.RunOnTick(() =>
		{
			InitMountMembers();
			OnConditionChange(ConditionFlag.Mounted, Services.Condition[ConditionFlag.Mounted]);
		});

		Services.SetModeHook = new();
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
		if (
			Services.ClientState.LocalPlayer is null
			|| ((Character*)Services.ClientState.LocalPlayer.Address)->Mode
				!= CharacterModes.Mounted
		)
			return;

		for (var i = 0; i < 100; i++)
		{
			if (Services.ObjectTable[i * 2] is not ICharacter character)
				continue;

			var charStruct = (Character*)character.Address;
			if (
				charStruct->Mode == CharacterModes.RidingPillion
				&& charStruct->GameObject.OwnerId == Services.ClientState.LocalPlayer.GameObjectId
			)
			{
				var passengerName = character.Name.TextValue;
				var passengerWorldName = Services
					.DataManager.GetExcelSheet<World>()
					.GetRow(charStruct->HomeWorld)
					.Name.ToString();

				var passengerNameString =
					passengerName + (char)SeIconChar.CrossWorld + passengerWorldName;

				Services.MountMembers[charStruct->ModeParam] = new Tuple<uint, string>(
					charStruct->GameObject.GetGameObjectId().ObjectId,
					passengerNameString
				);
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
