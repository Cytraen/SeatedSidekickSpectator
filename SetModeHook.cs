using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Text;
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

	private bool _inLoadScreen = false;

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

		try
		{
			if (Services.ClientState.LocalPlayer is null)
			{
				// apparently SetMode gets called when going to character select
				return;
			}

			var setChar = Services.ObjectTable.CreateObjectReference((nint)setCharStruct);
			var setCharName = setChar!.Name.TextValue;
			var setCharWorldName = Services.DataManager.GetExcelSheet<World>()
				?.GetRow(setCharStruct->HomeWorld)?.Name.ToString();

			if (setChar.ObjectKind != ObjectKind.Player) return;

			Services.PluginLog.Verbose($"SetMode called for {setCharName}{SeIconChar.CrossWorld.ToIconString()}{setCharWorldName}: '{oldCharMode} {oldModeParam}' -> '{newCharMode} {newModeParam}'");

			if (oldCharMode == newCharMode && oldModeParam == newModeParam)
			{
				return;
			}

			if (Services.ClientState.LocalPlayer.ObjectId == setCharStruct->GameObject.GetObjectID().ObjectID)
			{
				if (_inLoadScreen)
				{
					_inLoadScreen = false;
				}

				Services.Framework.RunOnTick(() =>
				{
					for (byte i = 1; i < 8; i++)
					{
						if (!Services.MountMembers.TryGetValue(i, out var charInfo)) continue;

						if (Services.ObjectTable.SearchById(charInfo.Item1) is not { } passenger ||
							((CharacterStruct*)passenger.Address)->Mode != CharacterModes.RidingPillion)
						{
							Services.MountMembers.Remove(i);
						}
					}
				}, delayTicks: 0);

				return;
			}

			if (oldCharMode is not CharacterModes.RidingPillion && newCharMode is not CharacterModes.RidingPillion)
			{
				return;
			}

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
					currentSeatId.Item1 == setCharStruct->GameObject.GetObjectID().ObjectID) return;

				var passengerName = setChar.Name.TextValue;
				var passengerWorldName = Services.DataManager.GetExcelSheet<World>()
					?.GetRow(setCharStruct->HomeWorld)?.Name.ToString();
				var passengerNameString = passengerName + (char)SeIconChar.CrossWorld + passengerWorldName;

				Services.MountMembers[newModeParam] = new Tuple<uint, string>(setCharStruct->GameObject.GetObjectID().ObjectID, passengerNameString);
			}
			else
			{
				if (!Services.MountMembers.Remove(oldModeParam)) return;

				if (((CharacterStruct*)Services.ClientState.LocalPlayer.Address)->Mode !=
					CharacterModes.Mounted) return;
			}

			if (Services.Config.ShowChatNotifications) Services.ChatGui.Print(notifSeString);
			if (Services.Config.ShowToastNotifications) Services.ToastGui.ShowNormal(notifSeString);
		}
		catch (Exception ex)
		{
			Services.PluginLog.Error(ex, "Error in SetModeHook");
		}
	}

	internal void OnTerritoryChanged(ushort _)
	{
		_inLoadScreen = true;
	}

	internal void Enable()
	{
		_hook.Enable();
		Services.ClientState.TerritoryChanged += OnTerritoryChanged;
	}

	internal void Disable()
	{
		Services.ClientState.TerritoryChanged -= OnTerritoryChanged;
		_hook.Disable();
	}

	public void Dispose()
	{
		Disable();
		_hook.Dispose();
		GC.SuppressFinalize(this);
	}
}
