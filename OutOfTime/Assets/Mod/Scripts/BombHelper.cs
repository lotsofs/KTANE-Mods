using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombHelper : MonoBehaviour {

    KMBombInfo _bombInfo;
    KMAudio _bombAudio;

    [NonSerialized] public int ModuleId;
    static int _moduleIdCounter = 1;

    KMAudio.KMAudioRef _customSound;

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
        _bombInfo = GetComponent<KMBombInfo>();
        _bombAudio = GetComponent<KMAudio>();
        _bombInfo.OnBombExploded += StopCustomSound;
	}

    /// <summary>
    /// Generic button press click sound & bomb oomph
    /// </summary>
    /// <param name="button"></param>
    /// <param name="intensityModifier"></param>
    public void GenericButtonPress(KMSelectable button, bool sound, float intensityModifier = 1) {
        button.AddInteractionPunch(intensityModifier);
        if (sound) {
            _bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
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
        char ch = _bombInfo.GetSerialNumber()[digit - 1];
        if ("02468".Contains(ch.ToString())) {
            return true;
        }
        return false;
    }

	#region sound

	public void PlayGameSound(KMSoundOverride.SoundEffect sound, Transform origin) {
        if (_bombAudio == null) return;
        _bombAudio.PlayGameSoundAtTransform(sound, origin.transform);
    }

    public void PlayCustomSound(string name, Transform origin) {
        if (_bombAudio == null) return;
        _bombAudio.PlaySoundAtTransform(name, origin);
    }

    public void PlayCustomSoundWithRef(string name, Transform origin) {
        if (_bombAudio == null) return;
        if (_customSound != null) return;
        _customSound = _bombAudio.PlaySoundAtTransformWithRef(name, origin);
    }

    public void StopCustomSound() {
        if (_bombAudio == null) return;
        if (_customSound != null) {
            _customSound.StopSound();
            _customSound = null;
        }
    }

	#endregion

}
