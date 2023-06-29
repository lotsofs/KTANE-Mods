using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombHelper : MonoBehaviour {

	public KMBombInfo BombInfo { get { return _bombInfo == null ? GetComponent<KMBombInfo>() : _bombInfo; } set { _bombInfo = value; } }
	public KMAudio Audio { get { return _audio == null ? GetComponent<KMAudio>() : _audio; } set { _audio = value; } }
	public KMBombModule Module { get { return _module == null ? GetComponent<KMBombModule>() : _module; } set { _module = value; } }
	public KMColorblindMode ColorblindMode { get { return _colorblindMode == null ? GetComponent<KMColorblindMode>() : _colorblindMode; } set { _colorblindMode = value; } }
	public KMSelectable Selectable { get { return _selectable == null ? GetComponent<KMSelectable>() : _selectable; } set { _selectable = value; } }

	[NonSerialized] KMBombInfo _bombInfo;
	[NonSerialized] KMAudio _audio;
	[NonSerialized] KMBombModule _module;
	[NonSerialized] KMColorblindMode _colorblindMode;
	[NonSerialized] KMSelectable _selectable;

	[NonSerialized] public int ModuleId;
	string _logTag;
	string _logTagHidden;
	static int _moduleIdCounter = 1;

	KMAudio.KMAudioRef _customSound;

	public bool ColorBlindModeActive = false;

	[SerializeField] float _defaultInteractionPunchIntensity = 0.1f;
	Dictionary<KMSelectable, float> _interactionPunchIntensities = new Dictionary<KMSelectable, float>();

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

		_logTag = string.Format("[{0} #{1}] ", Module.ModuleDisplayName, ModuleId);
		_logTagHidden = string.Format("<{0} #{1}> ", Module.ModuleDisplayName, ModuleId);
		ColorBlindModeActive = ColorblindMode != null && ColorblindMode.ColorblindModeActive;
	}

	void OnDestroy() {
		StopCustomSound();
	}

	public void LogHidden(string msg) {
		string message = _logTagHidden + msg;
		Debug.LogFormat(message);
	}

	public void LogHiddenFormat(string msg, params object[] arguments) {
		string message = _logTagHidden + string.Format(msg, arguments);
		Debug.LogFormat(message);
	}

	public void Log(string msg) {
		string message = _logTag + msg;
		Debug.Log(message);
	}

	public void LogFormat(string msg, params object[] arguments) {
		string message = _logTag + string.Format(msg, arguments);
		Debug.LogFormat(message);
	}

	public void LogWarning(string msg) {
		string message = _logTag + msg;
		Debug.LogWarning(message);
	}

	/// <summary>
	/// Solves the module
	/// </summary>
	public void Solve() {
		_module.HandlePass();
	}

	/// <summary>
	/// Strikes the module
	/// </summary>
	public void Strike() {
		_module.HandleStrike();
	}

	/// <summary>
	/// Boiler plate code that adds an interaction punch + clicky sound to every button on the bomb.
	/// </summary>
	/// <param name="buttonPressIntensities">Table of interaction punch intensities for different buttons, otherwise uses a preset value for all or remaining</param>
	public void AddGenericButtonPresses(List<float> buttonPressIntensities = null) {
		for (int i = 0; i < Selectable.Children.Length; i++) {
			KMSelectable button = Selectable.Children[i];
			if (buttonPressIntensities != null && i < buttonPressIntensities.Count) {
				_interactionPunchIntensities.Add(button, buttonPressIntensities[i]);
				button.OnInteract += () => {
					ExecuteGenericButtonPress(button, true, _interactionPunchIntensities[button]);
					return false;
				};
			}
			else {
				button.OnInteract += () => {
					ExecuteGenericButtonPress(button, true, _defaultInteractionPunchIntensity);
					return false;
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

	#region serial number

	/// <summary>
	/// Checks if a digit in the serial number is even (numbers only).
	/// </summary>
	/// <param name="digit"></param>
	/// <returns></returns>
	public bool IsSerialDigitEven(int digit) {
		if (digit > 5 || digit < 0) {
			return false;
		}
		char ch = BombInfo.GetSerialNumber()[digit - 1];
		if ("02468".Contains(ch.ToString())) {
			return true;
		}
		return false;
	}

	/// <summary>
	/// Find the first letter in a serial number
	/// </summary>
	/// <param name="fallback">The char to return if there are no letters</param>
	/// <returns></returns>
	public char GetSerialFirstLetter(char fallback = '-') {
		foreach (char ch in BombInfo.GetSerialNumber()) {
			if (char.IsLetter(ch)) { return ch; }
		}
		return fallback;
	}

	/// <summary>
	/// Find the first digit in a serial number
	/// </summary>
	/// <param name="fallback">The char to return if there are no letters</param>
	/// <returns></returns>
	public char GetSerialFirstDigit(char fallback = '-') {
		foreach (char ch in BombInfo.GetSerialNumber()) {
			if (char.IsDigit(ch)) { return ch; }
		}
		return fallback;
	}

	#endregion

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
