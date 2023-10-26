using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Lumina.Excel.GeneratedSheets;
using CharacterStruct = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace SeatedSidekickSpectator;

internal class MountHook : IDisposable
{
	private const string MountFuncSig = "E8 ?? ?? ?? ?? 80 7F 19 2D";

	private delegate void MountFuncDelegate(long a1, ushort a2, uint a3, uint a4, int a5, byte a6, byte a7, byte a8);

	private readonly Hook<MountFuncDelegate> _hook;

	internal MountHook()
	{
		if (!Services.SigScanner.TryScanText(MountFuncSig, out var ptr))
			throw new NullReferenceException("'MountHook' sig could not be found");

		_hook = Services.GameInteropProvider.HookFromAddress<MountFuncDelegate>(ptr, MountFuncDetour);
		Enable();
	}

	private unsafe void MountFuncDetour(long a1, ushort a2, uint a3, uint a4, int a5, byte a6, byte a7, byte a8)
	{
		if (Services.ObjectTable.CreateObjectReference((nint)(a1 - 1648)) is not Character boardingCharacter)
		{
			Services.PluginLog.Warning("null Character entered mount?");
			_hook.Original(a1, a2, a3, a4, a5, a6, a7, a8);
			return;
		}

		var targetId = boardingCharacter.TargetObjectId;
		_hook.Original(a1, a2, a3, a4, a5, a6, a7, a8);

		try
		{
			var boardingCharStruct = (CharacterStruct*)boardingCharacter.Address;
			var selfMounted = (boardingCharStruct->Mount.Flags & 2) == 0;
			if (selfMounted) return;

			var boardingCharName = boardingCharacter.Name.TextValue;
			var boarderWorldName = Services.DataManager.GetExcelSheet<World>()
				?.GetRow(boardingCharStruct->HomeWorld)?.Name.ToString();

			if (Services.ObjectTable.SearchById(targetId) is Character targetCharacter)
			{
				var targetCharStruct = (CharacterStruct*)targetCharacter.Address;
				var targetCharName = targetCharacter.Name;
				var targetWorldName = Services.DataManager.GetExcelSheet<World>()
					?.GetRow(targetCharStruct->HomeWorld)?.Name.ToString();

				Services.PluginLog.Debug(
					$"{boardingCharName}{(char)SeIconChar.CrossWorld}{boarderWorldName} boarded {targetCharName}{(char)SeIconChar.CrossWorld}{targetWorldName}'s mount.");
			}
			else
			{
				Services.PluginLog.Warning("Non-character GameObject is mount target?");
			}

			if (Services.ClientState.LocalPlayer is not { } playerCharacter)
			{
				Services.PluginLog.Warning("null LocalPlayer witnessed mount?");
				return;
			}

			if (targetId != playerCharacter.ObjectId) return;

			Services.MountMembers[boardingCharName + (char)SeIconChar.CrossWorld + boarderWorldName] =
				boardingCharStruct->ModeParam;

			var notifSeString = new SeString(new List<Payload>
			{
				new TextPayload(boardingCharacter.Name.TextValue),
				new IconPayload(BitmapFontIcon.CrossWorld),
				new TextPayload(boarderWorldName),
				new TextPayload(" boarded your mount.")
			});

			if (Services.Config.ShowChatNotifications) Services.ChatGui.Print(notifSeString);
			if (Services.Config.ShowToastNotifications) Services.ToastGui.ShowNormal(notifSeString);
		}
		catch (Exception ex)
		{
			Services.PluginLog.Error(ex, "An error occurred during post-MountFunc");
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
