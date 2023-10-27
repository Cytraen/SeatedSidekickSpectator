using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Numerics;
using CharacterStruct = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace SeatedSidekickSpectator.Windows;

internal class ConfigWindow : Window, IDisposable
{
	internal ConfigWindow() : base(
		"Seated Sidekick Spectator Settings",
		ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
	{
		Size = new Vector2(350, 400);
		SizeCondition = ImGuiCond.FirstUseEver;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public override unsafe void Draw()
	{
		var showInChatWindow = Services.Config.ShowChatNotifications;
		var showToast = Services.Config.ShowToastNotifications;

		var changed = false;

		if (ImGui.Checkbox("Show in toast", ref showToast))
		{
			Services.Config.ShowToastNotifications = showToast;
			changed = true;
		}

		if (ImGui.Checkbox("Show in chat window", ref showInChatWindow))
		{
			Services.Config.ShowChatNotifications = showInChatWindow;
			changed = true;
		}

		if (changed)
		{
			Services.Config.Save();
		}

		ImGui.NewLine();

		for (byte i = 1; i < 8; i++)
		{
			if (Services.MountMembers.TryGetValue(i, out var objId) && Services.ObjectTable.SearchById(objId) is Character passenger)
			{
				var passengerStruct = (CharacterStruct*)passenger.Address;
				var passengerName = passenger.Name.TextValue;
				var passengerWorldName = Services.DataManager.GetExcelSheet<World>()
					?.GetRow(passengerStruct->HomeWorld)?.Name.ToString();

				var passengerNameString = passengerName + (char)SeIconChar.CrossWorld + passengerWorldName;

				ImGui.Text($"{i}. {passengerNameString}");
			}
			else
			{
				ImGui.Text($"{i}.");
			}
		}
	}
}
