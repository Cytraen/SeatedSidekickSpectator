using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace SeatedSidekickSpectator.Windows;

internal sealed class ConfigWindow : Window
{
	private const string WindowTitle = "Seated Sidekick Spectator Settings";
	private readonly Plugin _plugin;

	internal ConfigWindow(Plugin plugin)
		: base(WindowTitle, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
	{
		SizeCondition = ImGuiCond.Always;
		_plugin = plugin;
	}

	[SuppressMessage("ReSharper", "AssignmentInConditionalExpression")]
	public override void Draw()
	{
		Size = new Vector2(
			Helpers.CalcTextSize(WindowTitle).X + (ImGui.GetStyle().ItemSpacing.X * 8),
			0
		);
		var changed = false;

		var showToasts = Services.Config.ShowToastNotifications;
		if (changed |= ImGui.Checkbox("Show in toast", ref showToasts))
		{
			Services.Config.ShowToastNotifications = showToasts;
		}

		var showChat = Services.Config.ShowChatNotifications;
		if (changed |= ImGui.Checkbox("Show in chat window", ref showChat))
		{
			Services.Config.ShowChatNotifications = showChat;
		}

		var showPassengerListWindow = Services.Config.ShowPassengerListWindow;
		if (changed |= ImGui.Checkbox("Show passenger list window", ref showPassengerListWindow))
		{
			Services.Config.ShowPassengerListWindow = showPassengerListWindow;
			_plugin.OnConditionChange(
				ConditionFlag.Mounted,
				Services.Condition[ConditionFlag.Mounted]
			);
		}

		if (ImGui.IsItemHovered())
		{
			ImGui.SetTooltip(
				"Creates a persistent window that lists all passengers while on a multi-seat mount."
			);
		}

		if (changed)
		{
			Services.Config.Save();
		}

		ImGui.NewLine();

		var mounted = Services.Condition[ConditionFlag.Mounted];
		if (mounted)
		{
			var passengerCount = Helpers.GetNumberOfSeats();
			ImGui.Text(passengerCount == 0 ? "Mounted on single-rider mount" : "Passengers:");
		}
		else
		{
			ImGui.Text("Not mounted");
		}

		using (var disabled = ImRaii.Disabled(!mounted))
		{
			Helpers.ImGuiDrawPassengerList();
		}
	}
}
