using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Lumina.Excel.GeneratedSheets;
using CharacterStruct = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace SeatedSidekickSpectator;

internal class DismountHook : IDisposable
{
	private const string DismountFuncSig = "E8 ?? ?? ?? ?? 48 8B 5C 24 ?? 48 83 C4 38 C3 48 8D 8B ?? ?? ?? ??";

	private delegate void DismountFuncDelegate(long a1, long a2, float a3);

	private readonly Hook<DismountFuncDelegate> _hook;

	internal DismountHook()
	{
		if (!Services.SigScanner.TryScanText(DismountFuncSig, out var ptr))
		{
			throw new NullReferenceException("'DismountHook' sig could not be found");
		}

		_hook = Services.GameInteropProvider.HookFromAddress<DismountFuncDelegate>(ptr, DismountFuncDetour);
		Enable();
	}

	private unsafe void DismountFuncDetour(long a1, long a2, float a3)
	{
		_hook.Original(a1, a2, a3);

		try
		{
			if (Services.ObjectTable.CreateObjectReference((nint)(a1 - 1648)) is not Character dismountingCharacter)
			{
				Services.PluginLog.Warning("null Character exited mount?");
				return;
			}

			var dismountingCharName = dismountingCharacter.Name.TextValue;
			var dismounterWorldName = Services.DataManager.GetExcelSheet<World>()
				?.GetRow(((CharacterStruct*)dismountingCharacter.Address)->HomeWorld)?.Name.ToString();

			Services.Framework.RunOnTick(() =>
			{
				if (!Services.MountMembers.Remove(dismountingCharName + (char)BitmapFontIcon.CrossWorld + dismounterWorldName)) return;

				var notifSeString = new SeString(new List<Payload>
				{
					new TextPayload(dismountingCharacter.Name.TextValue),
					new IconPayload(BitmapFontIcon.CrossWorld),
					new TextPayload(dismounterWorldName),
					new TextPayload(" exited your mount.")
				});

				if (Services.Config.ShowChatNotifications)
				{
					Services.ChatGui.Print(notifSeString);
				}
				if (Services.Config.ShowToastNotifications)
				{
					Services.ToastGui.ShowNormal(notifSeString);
				}
			}, delayTicks: 3);

			Services.PluginLog.Debug(
				$"{dismountingCharName}{(char)SeIconChar.CrossWorld}{dismounterWorldName} dismounted.");
		}
		catch (Exception ex)
		{
			Services.PluginLog.Error(ex, "An error occurred during post-DismountFunc");
		}
	}

	internal void Enable()
	{
		_hook.Enable();
	}

	internal void Disable()
	{
		_hook.Disable();
	}

	public void Dispose()
	{
		Disable();
		_hook.Dispose();
		GC.SuppressFinalize(this);
	}
}
