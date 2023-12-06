using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace SeatedSidekickSpectator.Windows;

internal class PassengerListWindow : Window, IDisposable
{
	internal PassengerListWindow() : base(
		"###SeatedSidekickSpectatorPassengerList",
		ImGuiWindowFlags.NoScrollbar
		| ImGuiWindowFlags.NoScrollWithMouse
		| ImGuiWindowFlags.NoTitleBar
		| ImGuiWindowFlags.AlwaysAutoResize
		| (Services.Config.PassengerListWindowLocked ? ImGuiWindowFlags.NoMove : 0))
	{
	}

	public override void Draw()
	{
		SizeConstraints = new WindowSizeConstraints
		{
			MinimumSize = new Vector2(Helpers.CalcTextSize($"MWMWMWMW MWMWMWMW{SeIconChar.CrossWorld}Adamantoise").X +
									  ImGui.GetStyle().ItemSpacing.X * 2, 0),
			MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
		};

		ImGui.Text("Passengers:");

		if (ImGui.IsWindowHovered())
		{
			ImGui.SameLine();
			var windowLockText =
				$"(window {(Services.Config.PassengerListWindowLocked ? "locked" : "unlocked")}, right-click to {(Services.Config.PassengerListWindowLocked ? "unlock" : "lock")}.)";
			ImGui.SetCursorPosX(ImGui.GetWindowSize().X - Helpers.CalcTextSize(windowLockText).X - ImGui.GetStyle().WindowPadding.X);
			ImGui.Text(windowLockText);
			if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
			{
				Services.Config.PassengerListWindowLocked ^= true;
				Services.Config.Save();
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
