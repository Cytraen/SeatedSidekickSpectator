using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;

namespace SeatedSidekickSpectator.Windows;

internal sealed class PassengerListWindow : Window
{
	internal PassengerListWindow()
		: base(
			"###SeatedSidekickSpectatorPassengerList",
			ImGuiWindowFlags.NoScrollbar
				| ImGuiWindowFlags.NoScrollWithMouse
				| ImGuiWindowFlags.NoTitleBar
				| ImGuiWindowFlags.AlwaysAutoResize
				| ImGuiWindowFlags.NoMove
		) { }

	public override void Draw()
	{
		SizeConstraints = new WindowSizeConstraints
		{
			MinimumSize = new Vector2(
				Helpers
					.CalcTextSize(
						$"MWMWMWMW MWMWMWMW{SeIconChar.CrossWorld.ToIconChar()}Adamantoise"
					)
					.X + (ImGui.GetStyle().ItemSpacing.X * 2),
				0
			),
			MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
		};

		ImGui.Text("Passengers:");

		if (ImGui.IsWindowHovered())
		{
			ImGui.SameLine();
			var isLocked = (Flags & ImGuiWindowFlags.NoMove) == ImGuiWindowFlags.NoMove;
			var windowLockText =
				$"(window {(isLocked ? "locked" : "unlocked")}, right-click to {(isLocked ? "unlock" : "lock")}.)";
			ImGui.SetCursorPosX(
				ImGui.GetWindowSize().X
					- Helpers.CalcTextSize(windowLockText).X
					- ImGui.GetStyle().WindowPadding.X
			);
			ImGui.Text(windowLockText);
			if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
			{
				Flags ^= ImGuiWindowFlags.NoMove;
			}
		}

		Helpers.ImGuiDrawPassengerList();
	}
}
