using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombHelper : MonoBehaviour {

	[NonSerialized] public KMBombInfo BombInfo;
	[NonSerialized] public KMAudio Audio;
	[NonSerialized] public KMBombModule Module;
	[NonSerialized] public KMColorblindMode ColorblindMode;
	[NonSerialized] public KMSelectable Selectable;

	[NonSerialized] public int ModuleId;
	string _logTag;
	static int _moduleIdCounter = 1;

	KMAudio.KMAudioRef _customSound;

	public bool ColorBlindModeActive = false;

	[SerializeField] float _defaultInteractionPunchIntensity = 0.1f;

	/// <summary>
	/// Increment Module ID
	/// </summary>
	void IncrementID() {
		ModuleId = _moduleIdCounter;
		_moduleIdCounter++;
	}

	void Awake() {
		IncrementID();
	}

	void Start() {
		BombInfo = GetComponent<KMBombInfo>();
		Module = GetComponent<KMBombModule>();
		Audio = GetComponent<KMAudio>();
		ColorblindMode = GetComponent<KMColorblindMode>();
		Selectable = GetComponent<KMSelectable>();

		BombInfo.OnBombExploded += StopCustomSound;
		_logTag = string.Format("[{0} #{1}] ", Module.ModuleDisplayName, ModuleId);
		ColorBlindModeActive = ColorblindMode != null && ColorblindMode.ColorblindModeActive;
	}

	public void Log(string msg) {
		string message = _logTag + msg;
		Debug.LogFormat(message);
	}

	public void LogWarning(string msg) {
		string message = _logTag + msg;
		Debug.LogWarningFormat(message);
	}

	/// <summary>
	/// Boiler plate code that adds an interaction punch + clicky sound to every button on the bomb.
	/// </summary>
	/// <param name="buttonPressIntensities">Table of interaction punch intensities for different buttons, otherwise uses a preset value for all or remaining</param>
	public void AddGenericButtonPresses(List<float> buttonPressIntensities = null) {
		for (int i = 0; i < Selectable.Children.Length; i++) {
			KMSelectable button = Selectable.Children[i];
			if (buttonPressIntensities != null && buttonPressIntensities.Count > i) {
				button.OnInteract += () => {
					ExecuteGenericButtonPress(button, true, buttonPressIntensities[i]);
					return true;
				};
			}
			else {
				button.OnInteract += () => {
					ExecuteGenericButtonPress(button, true, _defaultInteractionPunchIntensity);
					return true;
				};
			}
		}

	}

	/// <summary>
	/// Generic button press click sound & bomb oomph
	/// </summary>
	/// <param name="button"></param>
	/// <param name="intensityModifier"></param>
	public void ExecuteGenericButtonPress(KMSelectable button, bool sound = true, float intensityModifier = 1) {
		button.AddInteractionPunch(intensityModifier);
		if (sound) {
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
		}
	}

	/// <summary>
	/// Checks if a digit in the serial number is even (numbers only).
	/// </summary>
	/// <param name="digit"></param>
	/// <returns></returns>
	public bool IsSerialDigitEven(int digit) {
		if (digit > 6 || digit < 1) {
			return false;
		}
		char ch = BombInfo.GetSerialNumber()[digit - 1];
		if ("02468".Contains(ch.ToString())) {
			return true;
		}
		return false;
	}

	#region sound

	public void PlayGameSound(KMSoundOverride.SoundEffect sound, Transform origin) {
		if (Audio == null) return;
		Audio.PlayGameSoundAtTransform(sound, origin.transform);
	}

	public void PlayCustomSound(string name, Transform origin) {
		if (Audio == null) return;
		Audio.PlaySoundAtTransform(name, origin);
	}

	public void PlayCustomSoundWithRef(string name, Transform origin) {
		if (Audio == null) return;
		if (_customSound != null) return;
		_customSound = Audio.PlaySoundAtTransformWithRef(name, origin);
	}

	public void StopCustomSound() {
		if (Audio == null) return;
		if (_customSound != null) {
			_customSound.StopSound();
			_customSound = null;
		}
	}

	#endregion

}
