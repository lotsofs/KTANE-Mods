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

	// Use this for initialization
	void Start () {
		SetWallColors();
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

		Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		tex.SetPixel(0, 0, new Color(r, g, b));
		tex.Apply();

		Texture2D texDark = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		texDark.SetPixel(0, 0, new Color(rD, gD, bD));
		texDark.Apply();

		foreach (Renderer renderer in wallRenderers) {
			renderer.material.mainTexture = tex;
			renderer.material.SetTexture("_SecondTex", texDark);
		}
	}


}
