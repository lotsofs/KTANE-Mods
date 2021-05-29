using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightsHandler : MonoBehaviour {

	[SerializeField] GameObject _lightContainer;
	[SerializeField] Light[] _lightSources;
	[SerializeField] TextMesh _display;
	[SerializeField] Screen _screen;
	//[Space]
	//[SerializeField] GameObject _redBackdrop;
	//[SerializeField] GameObject _yellowBackdrop;
	//[SerializeField] GameObject _greenBackdrop;
	//[SerializeField] GameObject _cyanBackdrop;
	//[SerializeField] GameObject _blueBackdrop;
	//[SerializeField] GameObject _magentaBackdrop;
	//[SerializeField] GameObject _whiteBackdrop;
	[Space]
	[SerializeField] Material[] _backdrops;
	[SerializeField] Material[] _backPlanes;
	[SerializeField] Color[] _colors;
	[Space]
	[SerializeField] MovableObject _platformMover;
	[SerializeField] TextMesh _displayText;
	[Space]
	[SerializeField] Renderer _backdropModel;
	[SerializeField] Renderer _backdropPlane;
	Color _currentCol;

	public enum Colors {
		Red,
		Yellow,
		Green,
		Cyan,
		Blue,
		Magenta,
		White
	}

	void Awake() {
	}

	public void TurnOn(bool yes) {
		float scalar = transform.lossyScale.x;
		foreach (Light light in _lightSources) {
			//light.range *= scalar;
			light.range = 1;	// this doesn't do jack since a range of over 1 doesn't seem to do anything.
		}
		_lightContainer.SetActive(yes);
		_backdropModel.gameObject.SetActive(yes);
		_backdropPlane.gameObject.SetActive(yes);
		_displayText.color = yes ? _currentCol : Color.gray;
		_screen.UpdateCounter();
	}
	
	public void SetColor(int index) {
		index %= 7;
		SetColor((Colors)index);
	}

	public void SetColor(Colors color) {
		bool active = _backdropModel.gameObject.activeInHierarchy;

		_backdropModel.material = _backdrops[(int)color];
		_backdropPlane.material = _backPlanes[(int)color];
		_currentCol = _colors[(int)color];

		foreach (Light light in _lightSources) {
			light.color = _currentCol;
		}

		if (active) {
			_display.color = _currentCol;
		}
	}

	public void Update() {
		foreach (Light light in _lightSources) {
			light.intensity = _platformMover.ExtendedRate; // * 0.5f;
		}
	}
}
