using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace SeatedSidekickSpectator.Windows;

internal class PassengerListWindow : Window, IDisposable
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
			MinimumSize = new(
				Helpers.CalcTextSize($"MWMWMWMW MWMWMWMW{SeIconChar.CrossWorld}Adamantoise").X
					+ ImGui.GetStyle().ItemSpacing.X * 2,
				0
			),
			MaximumSize = new(float.MaxValue, float.MaxValue),
		};

		ImGui.Text("Passengers:");

		if (ImGui.IsWindowHovered())
		{
			ImGui.SameLine();
			var windowLockText =
				$"(window {((Flags & ImGuiWindowFlags.NoMove) == 0 ? "unlocked" : "locked")}, right-click to {((Flags & ImGuiWindowFlags.NoMove) == 0 ? "lock" : "unlock")}.)";
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

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}
}
