using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.AccessControl;
using System.Threading;
using UnityEngine;

public class Walls : MonoBehaviour {

	GreenScreenSettings _settings;
	public KMModSettings modSettings;
	public Renderer[] wallRenderers;
	public KMGameplayRoom Room;

	Texture2D _lightTexture;
	Texture2D _darkTexture;

	// Use this for initialization
	void Start() {
		SetWallColors();
		Room.OnLightChange += ChangeLights;
	}

	void ChangeLights(bool on) {
		StartCoroutine(HandlePacingLights(on));
	}

	IEnumerator HandlePacingLights(bool on) {
		if (on) {
			yield return new WaitForSeconds(0.611f);
			foreach (Renderer renderer in wallRenderers) {
				renderer.material.mainTexture = _lightTexture;
				renderer.material.SetTexture("_SecondTex", _lightTexture);
			}
		}
		else {
			yield return new WaitForSeconds(1.57f);
			foreach (Renderer renderer in wallRenderers) {
				renderer.material.mainTexture = _darkTexture;
				renderer.material.SetTexture("_SecondTex", _darkTexture);
			}
		}
	}

	/// <summary>
	/// Reads the config file and colors the walls accordingly.
	/// </summary>
	void SetWallColors() {
		_settings = JsonConvert.DeserializeObject<GreenScreenSettings>(modSettings.Settings);

		int r = _settings.red;
		int g = _settings.green;
		int b = _settings.blue;
		int rD = _settings.redDark;
		int gD = _settings.greenDark;
		int bD = _settings.blueDark;

		_lightTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		_lightTexture.SetPixel(0, 0, new Color(r, g, b));
		_lightTexture.Apply();

		_darkTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		_darkTexture.SetPixel(0, 0, new Color(rD, gD, bD));
		_darkTexture.Apply();

		foreach (Renderer renderer in wallRenderers) {
			renderer.material.mainTexture = _lightTexture;
			renderer.material.SetTexture("_SecondTex", _lightTexture);
		}
	}


}