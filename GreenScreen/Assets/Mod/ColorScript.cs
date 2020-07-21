using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GreenScreenSettings {
	public int red;
	public int green;
	public int blue;
	public int redDark;
	public int greenDark;
	public int blueDark;
}

public class ColorScript : MonoBehaviour {

	GreenScreenSettings settings;
	public KMModSettings modSettings;

	public Renderer[] renderers;

	public KMAudio audio;

	bool _initialLightChange = false;

	// Use this for initialization
	void Start () {

		settings = JsonConvert.DeserializeObject<GreenScreenSettings>(modSettings.Settings);

		int r = settings.red;
		int g = settings.green;
		int b = settings.blue;
		int rD = settings.redDark;
		int gD = settings.greenDark;
		int bD = settings.blueDark;
		
		Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		tex.SetPixel(0, 0, new Color(r, g, b));
		tex.Apply();

		Texture2D texDark = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		texDark.SetPixel(0, 0, new Color(rD, gD, bD));
		texDark.Apply();

		foreach (Renderer renderer in renderers) {
			renderer.material.mainTexture = tex;
			renderer.material.SetTexture("_SecondTex", texDark);
		}
	}


	void ChangeLight(bool on) {
		if (_initialLightChange) {
			if (on) {
				// turn the lights on at the start of the round
				_initialLightChange = true;
			}
			else {
				// turn shit to dark at the start of the round
			}
		}
		else {
			if (on) {
				// turn the lights on after the event
			}
			else {
				// turn the lights off for the event
			}
		}
	}

	IEnumerator ChangeLightIntensity(KMSoundOverride.SoundEffect sound, float waitingTime, bool on) {
		if (sound != null) {
			audio.PlayGameSoundAtTransform(sound, transform);
		}

		yield return new WaitForSeconds(waitingTime);
	}

}
