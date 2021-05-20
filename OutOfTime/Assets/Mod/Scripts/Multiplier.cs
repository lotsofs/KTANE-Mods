using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multiplier : MonoBehaviour {

	LightsHandler _lighting;
	KMBombInfo _bombInfo;

	int _baseMode;
	int _previousValue;

	bool _active;

	void Start() {
		_lighting = GetComponent<LightsHandler>();
		_bombInfo = GetComponent<KMBombInfo>();
	}

	public void Disable() {
		_lighting.TurnOn(false);
	}

	public void StartRandomEffect() {
		_baseMode = Random.Range(0, 7);
		int strikes = _bombInfo.GetStrikes();
		strikes %= 7;
		int colorToLightUp = (_baseMode + 7 - strikes) % 7;
		_lighting.SetColor(colorToLightUp);
		_lighting.TurnOn(true);
		_active = true;
	}

	public void CalculateSequenceEnd(int value) {
		// TODO: Stop the multiplier if we have enough points gathered, so as to not overdo it.
	}

	public int Multiply(int value) {
		if (!_active) {
			_previousValue = value;
			return value;
		}
		int returnVal = value;
		int actualMode = _baseMode + _bombInfo.GetStrikes();
		switch (actualMode % 7) {
			case 0:     // Red: Values are doubled.
				returnVal *= 2;
				break;
			case 1:     // Yellow: Value is multiplied by previous.
				returnVal *= _previousValue;
				break;
			case 2:     // Green: Difference with previous value is added.
				returnVal += Mathf.Abs(returnVal - _previousValue);
				break;
			case 3:     // Cyan: Every button adds 10.
				returnVal = 10;
				break;
			case 4:     // Blue: Values are squared.
				returnVal *= returnVal;
				break;
			case 5:     // Magenta: Buttons add minutes, not seconds.
				returnVal *= 60;
				break;
			case 6:     // White: Counter increases on its own.
				break;
		}
		_previousValue = value;
		return returnVal;
	}

}
