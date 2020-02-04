using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkenLight : MonoBehaviour {

	[SerializeField] Renderer _renderer;

	[SerializeField] Material _off;
	[SerializeField] Material _on;

	[SerializeField] float _onTime;
	[SerializeField] float _offTime;

	float _elapsedTime = 0;
	int _status = 0;
	Coroutine _coroutine;

	public void TurnOffDelay(float delay) {
		_coroutine = StartCoroutine(TurnOffDelayCoroutine(delay));
	}

	IEnumerator TurnOffDelayCoroutine(float delay) {
		_status = 2;
		_renderer.material = _on;
		yield return new WaitForSeconds(delay);
		_status = 0;
		_renderer.material = _off;
		_coroutine = null;
	}

	public void TurnOff() {
		if (_coroutine != null) {
			return;
		}
		_status = 0;
		_renderer.material = _off;
	}

	public void TurnOn() {
		StopAllCoroutines();
		_status = 2;
		_renderer.material = _on;
	}

	public void TurnBlinky() {
		_status = 1;
	}

	void Update() {
		if (_status != 1) {
			return;
		}
		_elapsedTime += Time.deltaTime;
		if (_elapsedTime > _onTime + _offTime) {
			_elapsedTime = 0;
		}
		if (_elapsedTime > _onTime) {
			_renderer.material = _off;
		}
		else {
			_renderer.material = _on;
		}
	}
}
