using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour {

	public Vector3 MaxShake;
	Vector3 _originalPosition;
	float _currentShake;
	bool _shaking;

	// Use this for initialization
	void Start () {
		_originalPosition = transform.localPosition;
		_shaking = false;
	}

	// Update is called once per frame
	void Update() {
		if (_shaking) {
			Vector3 newPos = UnityEngine.Random.insideUnitSphere;
			newPos.x *= MaxShake.x * _currentShake;
			newPos.y *= MaxShake.y * _currentShake;
			newPos.z *= MaxShake.z * _currentShake;
			newPos += _originalPosition;
			transform.localPosition = newPos;
		}
	}

	/// <summary>
	/// Set shake at rate, with 1 being the maximum and 0 being off.
	/// </summary>
	/// <param name="rate"></param>
	public void SetShake(float rate) {
		_currentShake = rate;
	}

	public void TurnOn(bool on) {
		_shaking = on;
	}
}
