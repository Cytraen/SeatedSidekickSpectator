using Dalamud.Interface.Utility;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Numerics;
using CharacterStruct = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace SeatedSidekickSpectator;

internal static class Helpers
{
	/// <summary>
	/// This function will attempt to calculate the size of the text while taking into consideration:
	/// <para>1. Dalamud global scale</para>
	/// <para>2. Our own scale if provided</para>
	/// <para>3. The font if provided</para>
	/// <para>4. The custom font size (that might be different from the font's default size,) if provided</para>
	/// The custom font size needs to be unscaled, that is, for example, not 32 * scale, but 32.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="scale"></param>
	/// <param name="font"></param>
	/// <param name="fontSize"></param>
	/// <returns></returns>
	public static Vector2 CalcTextSize(string text, float scale = 1f, ImFontPtr? font = null, float fontSize = -1)
	{
		var fontSizeMultiplier = 1f;

		if (font.HasValue)
		{
			fontSizeMultiplier = fontSize < 0 ? 1f : fontSize / font.Value.FontSize;

			ImGui.PushFont(font.Value);
		}

		var textSize = ImGui.CalcTextSize(text) / ImGuiHelpers.GlobalScale * scale * fontSizeMultiplier;

		if (font.HasValue)
		{
			ImGui.PopFont();
		}

		return textSize;
	}

	public static unsafe ushort? GetNumberOfSeats()
	{
		if (Services.ClientState.LocalPlayer is null) return null;
		var mountId = ((CharacterStruct*)Services.ClientState.LocalPlayer.Address)->Mount.MountId;
		return Services.DataManager.GetExcelSheet<Mount>()?.GetRow(mountId)?.ExtraSeats;
	}

	public static void ImGuiDrawPassengerList()
	{
		var seats = GetNumberOfSeats() + 1 ?? 8;

		for (byte i = 1; i < seats; i++)
		{
			if (Services.MountMembers.TryGetValue(i, out var charInfo))
			{
				ImGui.Text($"{i}. {charInfo.Item2}");
			}
			else
			{
				ImGui.Text($"{i}.");
			}
		}
	}
}
