using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using SeatedSidekickSpectator.Windows;

namespace SeatedSidekickSpectator;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once UnusedType.Global
internal sealed class Plugin : IDalamudPlugin
{
	private const string ConfigWindowCommandName = "/sss";
	private readonly WindowSystem _windowSystem = new("SeatedSidekickSpectator");
	private readonly ConfigWindow _configWindow;
	private readonly PassengerListWindow _passengerListWindow;

	public Plugin(IDalamudPluginInterface pluginInterface)
	{
		pluginInterface.Create<Services>();
		Services.Config = Configuration.Load();

		_configWindow = new ConfigWindow(this);
		_passengerListWindow = new PassengerListWindow();

		_windowSystem.AddWindow(_configWindow);
		_windowSystem.AddWindow(_passengerListWindow);

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

		Services.SetModeHook = new SetModeHook();
	}

	public void OnConditionChange(ConditionFlag condition, bool value)
	{
		if (condition is not ConditionFlag.Mounted)
		{
			return;
		}
		if (!value)
		{
			_passengerListWindow.IsOpen = false;
			return;
		}

		var seats = Helpers.GetNumberOfSeats() ?? 7;

		_passengerListWindow.IsOpen = seats != 0 && Services.Config.ShowPassengerListWindow;
	}

	private static unsafe void InitMountMembers()
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
		_windowSystem.RemoveAllWindows();

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
