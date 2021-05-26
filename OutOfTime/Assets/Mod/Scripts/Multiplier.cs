using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multiplier : MonoBehaviour {

	LightsHandler _lighting;
	KMBombInfo _bombInfo;
	BombHelper _bombHelper;

	int _rootMode;
	int _previousValue;
	int _previousBigValue;

	bool _active;
	
	public event Action OnLogDumpRequested;

	int _earnedDuringLighting = 0;
	int _maxPoints = 0;

	float _timer = 0;
	float _maxTime = 0;

	string[] _effectNames = { "Double", "Multiply By Previous", "Difference With Previous", "Fixed Ten", "Square", "Minutes", "Plus One" };
	string[] _colorNames = { "Red", "Yellow", "Green", "Cyan", "Blue", "Magenta", "White" };

	void Start() {
		_lighting = GetComponent<LightsHandler>();
		_bombInfo = GetComponent<KMBombInfo>();
		_bombHelper = GetComponent<BombHelper>();
	}

	public void Disable() {
		_lighting.TurnOn(false);
		_active = false;
		_maxPoints = int.MaxValue;
		_maxTime = float.MaxValue;
		_earnedDuringLighting = 0;
		_timer = 0;
	}

	public void StartRandomEffect() {
		_rootMode = UnityEngine.Random.Range(0, 7);
		int strikes = _bombInfo.GetStrikes();
		strikes %= 7;
		int effect = (_rootMode + strikes) % 7;
		_lighting.SetColor(_rootMode);
		_lighting.TurnOn(true);
		_active = true;
		_maxPoints = 59;
		_maxTime = 120f;

		OnLogDumpRequested.Invoke();
		_bombHelper.Log(string.Format("Lighting up random color: {0}. Strike count: {1}, so this is {2};", _colorNames[_rootMode], _bombInfo.GetStrikes(), _effectNames[effect]));
	}

	public void CalculateSequenceEnd(int value) {
		_earnedDuringLighting += value;
		if (_earnedDuringLighting > _maxPoints) {
			OnLogDumpRequested.Invoke();
			_bombHelper.Log("Disabling lights");
			Disable();
		}
	}

	public void StartWeightedEffect(int r, int y, int g, int c, int b, int m, int w) {
		int t = r + y + g + c + b + m + w;
		int rand = UnityEngine.Random.Range(0, t);
		rand -= r; // double
		if (rand < 0) { StartSpecific(0, int.MaxValue, 60f); return; }
		rand -= y;	// multiply by prev
		if (rand < 0) { StartSpecific(1, 600, 30f); return; }
		rand -= g;	// difference with prev
		if (rand < 0) { StartSpecific(2, int.MaxValue, 60f); return; }
		rand -= c;	// fixed 10
		if (rand < 0) { StartSpecific(3, int.MaxValue, 60f); return; }
		rand -= b;	// squared
		if (rand < 0) { StartSpecific(4, 600, 60f); return; }
		rand -= m;	// minutes
		if (rand < 0) { StartSpecific(5, 300, 30f); return; }
		rand -= w;	// one more than previous
		if (rand < 0) { StartSpecific(6, 600, 60f); return; }
	}

	void StartSpecific(int effect, int maxPoints, float duration) {
		int strikes = _bombInfo.GetStrikes();
		strikes %= 7;
		int colorToLightUp = (effect + 7 - strikes) % 7;
		_rootMode = colorToLightUp;

		OnLogDumpRequested.Invoke();
		_bombHelper.Log(string.Format("Starting effect: {0}. Strike count: {1}, so lighting up {2};", _effectNames[effect], _bombInfo.GetStrikes(), _colorNames[colorToLightUp]));

		_lighting.SetColor(colorToLightUp);
		_lighting.TurnOn(true);
		_active = true;
		_earnedDuringLighting = 0;
		_maxPoints = maxPoints;
		_maxTime = duration;
	}

	void Update() {
		if (!_active) return;

		_timer += Time.deltaTime;
		if (_timer > _maxTime) {
			OnLogDumpRequested.Invoke();
			_bombHelper.Log("Disabling lights");
			Disable();
		}
	}

	public int Multiply(int value) {
		if (!_active) {
			_previousValue = value;
			return value;
		}
		int returnVal = value;
		int actualMode = _rootMode + _bombInfo.GetStrikes();
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
			case 6:     // White: Each press adds one more than the previous.
				returnVal = _previousBigValue + 1;
				break;
		}
		_bombHelper.Log(string.Format("Pressed button {0} in mode {1}, which turns it into {2}", value, _effectNames[actualMode % 7] , returnVal));
		_previousValue = value;
		_previousBigValue = returnVal;
		return returnVal;
	}

}
