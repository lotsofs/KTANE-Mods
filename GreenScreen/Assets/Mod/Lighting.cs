using System.Collections;
using System;
using UnityEngine;
using System.Reflection;

// ----- Code courtesy of HexiCube ----- //

public class Lighting : MonoBehaviour {


	public const float OFF_LEVEL = 0.025f;
	public const float FADE_LEVEL = 0.1f;
	public const float ON_LEVEL = 0.5f;

	float _lightIntensity;
	bool _bombStarted = false;
	public Light RoomLight;
	public KMGameplayRoom Room;
	public KMAudio Sound;

	// Use this for initialization
	void Start () {
		_lightIntensity = RoomLight.intensity;
		ChangeLights(false);
		Room.OnLightChange += ChangeLights;
	}

	void ChangeLights(bool on) {
		if (_bombStarted) {
			if (on) {
				// pacing event: darkness
				StartCoroutine(HandlePacingLights(on));
			}
		}
		else {
			if (on) {
				// start of round lights on
				RegisterOneMinPacing();
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



	public void RegisterOneMinPacing() {
		try {
			Type room = FindType("Room");
			IList arr = (IList)room.GetField("PacingActions").GetValue(UnityEngine.Object.FindObjectsOfType(room));

			Type action = FindType("Assets.Scripts.Pacing.PacingAction");
			PropertyInfo eventField = action.GetProperty("EventType");
			object field = FindType("Assets.Scripts.Pacing.PaceEvents").GetField("OneMinuteLeft").GetValue(null);

			bool removed = false;
			foreach (object o in arr) {
				if (eventField.GetValue(o, null).Equals(field)) {
					arr.Remove(o);
					removed = true;
					break;
				}
			}
			if (removed) {
				// A removed pacing event indicates that pacing events are enabled. Only add our event below if they indeed are enabled.
				object pEventObj = Activator.CreateInstance(action, new object[] { "CustomOneMin", field });
				action.GetField("Action").SetValue(pEventObj, new Action(OneMinuteEvent));
				arr.Add(pEventObj);
			}
		}
		catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}


	public void OneMinuteEvent() {
		StartCoroutine(RedLight());
	}

	public IEnumerator RedLight() {
		while (true) {
			RoomLight.color = new Color(1f, .2f, .2f);
			Sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.EmergencyAlarm, RoomLight.transform);
			yield return new WaitForSeconds(1);
			RoomLight.color = new Color(.5f, .5f, .5f);
			yield return new WaitForSeconds(1.25f);
		}
	}

}
