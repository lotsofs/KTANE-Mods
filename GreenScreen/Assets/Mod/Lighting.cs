using System.Collections;
using System;
using UnityEngine;
using System.Reflection;

public class Lighting : MonoBehaviour {


	public const float OFF_LEVEL = 0.025f;
	public const float FADE_LEVEL = 0.1f;
	public const float ON_LEVEL = 0.5f;

	float _lightIntensity;
	Color _lightColor;
	bool _bombStarted = false;
	public Light RoomLight;
	public KMGameplayRoom Room;
	public KMAudio Sound;
	public KMBombInfo BombInfo;
	bool _redLightFlashing = false;
	Coroutine _redLightRoutine;

	// Use this for initialization
	void Start () {
		_lightIntensity = RoomLight.intensity;
		_lightColor = RoomLight.color;
		ChangeLights(false);
		Room.OnLightChange += ChangeLights;
	}

	void ChangeLights(bool on) {
		if (_bombStarted) {
			// pacing event: darkness
			StartCoroutine(HandlePacingLights(on));
		}
		else {
			if (on) {
				// start of round lights on
				Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Switch, RoomLight.transform);
				SetAmbient(ON_LEVEL);
				_bombStarted = true;
			}
			else {
				// start of round lights off
				SetAmbient(OFF_LEVEL);
			}
		}
	}

	IEnumerator HandlePacingLights(bool on) {
		if (on) {
			Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.LightBuzzShort, RoomLight.transform);
			SetAmbient(FADE_LEVEL);
			yield return new WaitForSeconds(0.611f);
			SetAmbient(ON_LEVEL);
		}
		else {
			Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.LightBuzz, RoomLight.transform);
			SetAmbient(FADE_LEVEL);
			yield return new WaitForSeconds(1.57f);
			SetAmbient(OFF_LEVEL);
		}
	}

	void Update() {
		if (!_redLightFlashing && BombInfo.IsBombPresent() && BombInfo.GetTime() < 60.0f) {
			// turn on the flashy red light
			if (_redLightRoutine != null) {
				StopCoroutine(_redLightRoutine);
				_redLightRoutine = null;
			}
			_redLightFlashing = true;
			_redLightRoutine = StartCoroutine(RedLight());
		}
	}

	void SetAmbient(float amount) {
		SetAmbient(amount, Color.white);
	}

	void SetAmbient(float amount, Color color) {
		SetAmbient(Color.Lerp(Color.black, Color.white, amount) * color);
		SetLightIntensity(amount);
	}

	void SetAmbient(Color color) {
		RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
		RenderSettings.ambientLight = color;
		RenderSettings.ambientIntensity = 1;

		DynamicGI.updateThreshold = 0;
		DynamicGI.UpdateEnvironment();
	}

	void SetLightIntensity(float intensity) {
		RoomLight.intensity = _lightIntensity * intensity;
	}


	public static Type FindType(string qualifiedTypeName) {
		Type t = Type.GetType(qualifiedTypeName);

		if (t != null) return t;
		else {
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
				t = asm.GetType(qualifiedTypeName);
				if (t != null) return t;
			}
			return null;
		}
	}

	public void OneMinuteEvent() {
		StartCoroutine(RedLight());
	}

	public IEnumerator RedLight() {
		while (BombInfo.GetTime() < 60.0f) {
			RoomLight.color = new Color(1f, .2f, .2f);
			Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.EmergencyAlarm, RoomLight.transform);
			yield return new WaitForSeconds(1);
			RoomLight.color = _lightColor;
			yield return new WaitForSeconds(1.25f);
		}
		RoomLight.color = _lightColor;
		_redLightFlashing = false;
		_redLightRoutine = null;
	}

}
