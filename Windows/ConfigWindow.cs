using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

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

	public override void Draw()
	{
		var showInChatWindow = Services.Config.ShowChatNotifications;
		var showToast = Services.Config.ShowToastNotifications;
		var showImGuiWindow = Services.Config.ShowListWindow;

		var changed = false;

		if (ImGui.Checkbox("Show in chat window", ref showInChatWindow))
		{
			Services.Config.ShowChatNotifications = showInChatWindow;
			changed = true;
		}

		if (ImGui.Checkbox("Show in toast", ref showToast))
		{
			Services.Config.ShowToastNotifications = showToast;
			changed = true;
		}

		if (ImGui.Checkbox("Show in new window", ref showImGuiWindow))
		{
			Services.Config.ShowListWindow = showImGuiWindow;
			changed = true;
		}

		if (changed)
		{
			Services.Config.Save();
		}

		for (var i = 1; i < 8; i++)
		{
			ImGui.Text($"{i}. {Services.MountMembers.FirstOrDefault(x => x.Value == i).Key}");
		}
	}
}
