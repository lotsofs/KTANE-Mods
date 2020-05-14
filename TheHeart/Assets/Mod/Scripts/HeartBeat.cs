using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartBeat : MonoBehaviour {

	[SerializeField] KMAudio _bombAudio;
	[NonSerialized] public bool Beating = false;
	public float Interval = 1.0f;
	float _defaultInterval;
	[SerializeField] float _doubleBeatTime = 0.3f;
	[SerializeField] float _shrinkRate = 0.03f;
	[SerializeField] float _firstBeatSize = 1.03f;
	[SerializeField] float _secondBeatSize = 1.025f;

	Vector3 _baseSize;
	
	float _elapsedTime = int.MaxValue;
	bool _doubleBeatHappened = false;

	public float TargetInterval;

	Renderer _renderer;

	public event EventHandler OnColorGone;

	/// <summary>
	/// This function plays sounds. It doesn't work if I play these sounds from where I should in the code. For some godforsaken reason.
	/// Also these audio files need to be named this because if I name them something sensible, it doesn't work either.
	/// </summary>
	/// <param name="ffs"></param>
	public void WhatIsOopQuestionMark(int ffs) {
		if (ffs == 1) {
			_bombAudio.PlaySoundAtTransform("KR-Convoy", this.transform);	// Defibrillator Thump
		}
		if (ffs == 2) {
			_bombAudio.PlaySoundAtTransform("KR-City", this.transform);		// Defribillator Charging
		}
	}

	// Use this for initialization
	void Start () {
		_renderer = GetComponent<Renderer>();

		_baseSize = transform.localScale;
		_defaultInterval = Interval;
		TargetInterval = Interval;

		_renderer.material.color = new Color(0.8f, 0, 0);
	}

	// Update is called once per frame
	void Update() {
		Beat();
		ChangeInterval();
	}

	void Beat() {
		_elapsedTime += Time.deltaTime;
		// Ba
		if (Beating && _elapsedTime > Interval) {
			_doubleBeatHappened = false;
			_elapsedTime = 0;
			transform.localScale = _baseSize * _firstBeatSize;
			_bombAudio.PlaySoundAtTransform("HeartIn", this.transform);
		}
		// Dum
		else if (Beating && !_doubleBeatHappened && _elapsedTime > _doubleBeatTime * Interval) {
			_doubleBeatHappened = true;
			transform.localScale = _baseSize * _secondBeatSize;
			_bombAudio.PlaySoundAtTransform("HeartOut", this.transform);
		}
		// <shrink>
		else if (transform.localScale.x > _baseSize.x) {
			transform.localScale *= (1f - (Time.deltaTime * _shrinkRate * Interval));
		}
	}

	void ChangeInterval() {
		if (Interval < 0.4f) {
			// heart beats too fast. We can go no faster. Calm down.
			TargetInterval = _defaultInterval;
		}
		if (Interval > TargetInterval) {
			Interval -= Time.deltaTime * 0.01f;
		}
		else if (Interval < TargetInterval) {
			Interval += Time.deltaTime * 0.01f;
		}
	}

	public void ResetInterval() {
		Interval = _defaultInterval;
		TargetInterval = _defaultInterval;
	}

	public IEnumerator DepleteColor() {
		Color col = _renderer.material.color;
		while (col.r > 0.2f) {
			col.b = Mathf.Min(col.b + Time.deltaTime * 0.01333f, 0.4f);
			col.r = Mathf.Max(col.r - Time.deltaTime * 0.02f, 0.2f);
			_renderer.material.color = col;
			yield return null;
		}
		while (col.g < 0.4f) {
			col.g = Mathf.Min(col.g + Time.deltaTime * 0.02f, 0.4f);
			col.r = Mathf.Min(col.r + Time.deltaTime * 0.01f, 0.4f);
			_renderer.material.color = col;
			yield return null;
		}
		while (col.g > 0.2f) {
			col.g = Mathf.Max(col.g - Time.deltaTime * 0.02f, 0.2f);
			col.r = Mathf.Max(col.r - Time.deltaTime * 0.02f, 0.2f);
			col.b = Mathf.Max(col.b - Time.deltaTime * 0.02f, 0.2f);
			_renderer.material.color = col;
			yield return null;
		}
		_renderer.material.color = new Color(0.2f, 0.2f, 0.2f);
		if (OnColorGone != null) {
			OnColorGone.Invoke(this, new EventArgs());
		}
		//yield return null;
	}

	public IEnumerator ReplenishColor() {
		Color col = _renderer.material.color;

		//while (col.b < 0.4f) {
		//	col.g = Mathf.Max(col.g - Time.deltaTime * 0.1f, 0.0f);
		//	col.r = Mathf.Min(col.r + Time.deltaTime * 0.2f, 0.8f);
		//	col.b = Mathf.Min(col.b + Time.deltaTime * 0.2f, 0.4f);
		//	_renderer.material.color = col;
		//	yield return null;
		//}

		while (col.r < 0.8f) {
			col.b = Mathf.Max(col.b - Time.deltaTime * 0.2f, 0.0f);
			col.g = Mathf.Max(col.g - Time.deltaTime * 0.2f, 0.0f);
			col.r = Mathf.Min(col.r + Time.deltaTime * 0.2f, 0.8f);
			_renderer.material.color = col;
			yield return null;
		}
		Debug.Log(col.r);
		_renderer.material.color = new Color(0.8f, 0, 0);
	}
}
