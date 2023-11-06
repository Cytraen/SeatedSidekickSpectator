using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Lumina.Excel.GeneratedSheets;
using CharacterModes = FFXIVClientStructs.FFXIV.Client.Game.Character.Character.CharacterModes;
using CharacterStruct = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace SeatedSidekickSpectator;

internal unsafe class SetModeHook : IDisposable
{
	private const string SetModeSig = "E8 ?? ?? ?? ?? 48 8B 4B 08 44 8B CF";

	private delegate void SetModeDelegate(CharacterStruct* a1, CharacterModes a2, byte a3);

	private readonly Hook<SetModeDelegate> _hook;

	internal SetModeHook()
	{
		if (!Services.SigScanner.TryScanText(SetModeSig, out var ptr))
			throw new NullReferenceException("'SetModeHook' sig could not be found");

		_hook = Services.GameInteropProvider.HookFromAddress<SetModeDelegate>(ptr, SetModeDetour);
		Enable();
	}

	private void SetModeDetour(CharacterStruct* setCharStruct, CharacterModes newCharMode,
		byte newModeParam)
	{
		var oldCharMode = setCharStruct->Mode;
		var oldModeParam = setCharStruct->ModeParam;

		_hook.Original(setCharStruct, newCharMode, newModeParam);

		if (Services.ClientState.LocalPlayer!.ObjectId == setCharStruct->GameObject.GetObjectID().ObjectID
			|| (oldCharMode == newCharMode && oldModeParam == newModeParam)
			|| (oldCharMode is not CharacterModes.RidingPillion && newCharMode is not CharacterModes.RidingPillion))
		{
			return;
		}

		var setChar = Services.ObjectTable.CreateObjectReference((nint)setCharStruct);
		var setCharName = setChar!.Name.TextValue;
		var setCharWorldName = Services.DataManager.GetExcelSheet<World>()
			?.GetRow(setCharStruct->HomeWorld)?.Name.ToString();

		var notifSeString = new SeString(new List<Payload>
		{
			new TextPayload(setCharName),
			new IconPayload(BitmapFontIcon.CrossWorld),
			new TextPayload(setCharWorldName),
			new TextPayload(
				$" {(newCharMode is CharacterModes.RidingPillion ? "boarded" : "exited")} your mount.")
		});

		if (newCharMode is CharacterModes.RidingPillion)
		{
			if (setChar.OwnerId != Services.ClientState.LocalPlayer.ObjectId) return;

			if (Services.MountMembers.TryGetValue(newModeParam, out var currentSeatId) &&
				currentSeatId == setChar.ObjectId) return;
			Services.MountMembers[newModeParam] = setChar.ObjectId;
			if (Services.Config.ShowChatNotifications) Services.ChatGui.Print(notifSeString);
			if (Services.Config.ShowToastNotifications) Services.ToastGui.ShowNormal(notifSeString);
		}
		else
		{
			if (!Services.MountMembers.Remove(oldModeParam)) return;

			Services.Framework.RunOnTick(() =>
			{
				if (((CharacterStruct*)Services.ClientState.LocalPlayer.Address)->Mode !=
					CharacterModes.Mounted) return;
				if (Services.Config.ShowChatNotifications) Services.ChatGui.Print(notifSeString);
				if (Services.Config.ShowToastNotifications) Services.ToastGui.ShowNormal(notifSeString);
			});
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
