using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombHelper : MonoBehaviour {

    [SerializeField] KMBombInfo _bombInfo;
    [SerializeField] KMAudio _bombAudio;

    [NonSerialized] public int ModuleId;
    static int _moduleIdCounter = 1;

    /// <summary>
    /// Increment Module ID
    /// </summary>
    void IncrementID() {
        ModuleId = _moduleIdCounter;
        _moduleIdCounter++;
    }

    /// <summary>
    /// Start
    /// </summary>
    void Awake() {
        IncrementID();
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
        if (ch == '0' || ch == '2' || ch == '4' || ch == '6' || ch == '8') {
            return true;
        }
        return false;
    }

}
