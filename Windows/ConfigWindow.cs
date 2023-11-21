using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace SeatedSidekickSpectator.Windows;

internal class ConfigWindow : Window, IDisposable
{
	public const string WindowTitle = "Seated Sidekick Spectator Settings";

	internal ConfigWindow() : base(
		WindowTitle,
		ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
	{
		Size = new Vector2(Helpers.CalcTextSize(WindowTitle).X + ImGui.GetStyle().ItemSpacing.X * 8, 0);
		SizeCondition = ImGuiCond.Always;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public override void Draw()
	{
		var changed = false;

		changed |= ImGui.Checkbox("Show in toast", ref Services.Config.ShowToastNotifications);

		changed |= ImGui.Checkbox("Show in chat window", ref Services.Config.ShowChatNotifications);

		if (ImGui.Checkbox("Show passenger list window", ref Services.Config.ShowPassengerListWindow))
		{
			Services.PassengerListWindow.IsOpen = Services.Config.ShowPassengerListWindow && Services.Condition[ConditionFlag.Mounted];
			changed = true;
		}
		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip("Creates a persistent window that lists all passengers while on a multi-seat mount.");
		}

		if (changed)
		{
			Services.Config.Save();
		}

		ImGui.NewLine();

		var mounted = Services.Condition[ConditionFlag.Mounted];
		ImGui.Text(mounted ? "Passengers:" : "Not mounted");

		ImGui.BeginDisabled(!mounted);

		Helpers.ImGuiDrawPassengerList();

		ImGui.EndDisabled();
	}
}
