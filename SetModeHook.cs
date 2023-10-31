using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Lumina.Excel.GeneratedSheets;
using CharacterStruct = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace SeatedSidekickSpectator;

internal unsafe class SetModeHook : IDisposable
{
	private const string SetModeSig = "E8 ?? ?? ?? ?? 48 8B 4B 08 44 8B CF";

	private delegate void SetModeDelegate(CharacterStruct* a1, CharacterStruct.CharacterModes a2, byte a3);

	private readonly Hook<SetModeDelegate> _hook;

	internal SetModeHook()
	{
		if (!Services.SigScanner.TryScanText(SetModeSig, out var ptr))
			throw new NullReferenceException("'SetModeHook' sig could not be found");

		_hook = Services.GameInteropProvider.HookFromAddress<SetModeDelegate>(ptr, SetModeDetour);
		Enable();
	}

	private void SetModeDetour(CharacterStruct* setCharStruct, CharacterStruct.CharacterModes newCharMode, byte newModeParam)
	{
		var oldCharMode = setCharStruct->Mode;
		var oldModeParam = setCharStruct->ModeParam;
		var targetId = setCharStruct->GetTargetId();

		_hook.Original(setCharStruct, newCharMode, newModeParam);

		if (oldCharMode == newCharMode && oldModeParam == newModeParam)
		{
			return;
		}

		if (oldCharMode is not (CharacterStruct.CharacterModes.Mounted or CharacterStruct.CharacterModes.RidingPillion)
			&& newCharMode is not (CharacterStruct.CharacterModes.Mounted or CharacterStruct.CharacterModes.RidingPillion))
		{
			return;
		}

		if (Services.ObjectTable.CreateObjectReference((nint)setCharStruct) is not Character setChar)
		{
			Services.PluginLog.Warning("SetMode called on non-Character?");
			return;
		}

		if (Services.ClientState.LocalPlayer is null)
		{
			Services.PluginLog.Warning("null LocalPlayer witnessed SetMode?");
			return;
		}

		if (setCharStruct->GameObject.GetObjectID() == Services.ClientState.LocalPlayer.ObjectId
			&& (oldCharMode is CharacterStruct.CharacterModes.Mounted || newCharMode is CharacterStruct.CharacterModes.Mounted))
		{
			if (oldCharMode == CharacterStruct.CharacterModes.Normal &&
				newCharMode == CharacterStruct.CharacterModes.Mounted)
			{
				Services.Framework.RunOnTick(() =>
				{
					for (byte i = 1; i < 8; i++)
					{
						if (!Services.MountMembers.TryGetValue(i, out var objId)) continue;

						if (Services.ObjectTable.SearchById(objId) is not Character passenger ||
							((CharacterStruct*)passenger.Address)->Mode != CharacterStruct.CharacterModes.RidingPillion)
						{
							Services.MountMembers.Remove(i);
						}
					}
				}, delayTicks: 0);
			}
			return;
		}

		if (oldCharMode is not CharacterStruct.CharacterModes.RidingPillion && newCharMode is not CharacterStruct.CharacterModes.RidingPillion)
		{
			return;
		}

		var setCharName = setChar.Name.TextValue;
		var setCharWorldName = Services.DataManager.GetExcelSheet<World>()
			?.GetRow(setCharStruct->HomeWorld)?.Name.ToString();

		if (newCharMode is CharacterStruct.CharacterModes.RidingPillion)
		{
			if (targetId != Services.ClientState.LocalPlayer.ObjectId)
			{
				return;
			}

			Services.MountMembers[newModeParam] = setChar.ObjectId;
		}
		else
		{
			if (!Services.MountMembers.Remove(oldModeParam)) return;
		}

		var notifSeString = new SeString(new List<Payload>
		{
			new TextPayload(setCharName),
			new IconPayload(BitmapFontIcon.CrossWorld),
			new TextPayload(setCharWorldName),
			new TextPayload($" {(newCharMode is CharacterStruct.CharacterModes.RidingPillion ? "boarded" : "exited")} your mount.")
		});

		if (Services.Config.ShowChatNotifications) Services.ChatGui.Print(notifSeString);
		if (Services.Config.ShowToastNotifications) Services.ToastGui.ShowNormal(notifSeString);
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
