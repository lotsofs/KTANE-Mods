using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartBeat : MonoBehaviour {

	[SerializeField] KMAudio _bombAudio;
	[NonSerialized] public bool Beating = false;
	public float Interval = 1.0f;
	[SerializeField] float _doubleBeatTime = 0.3f;
	[SerializeField] float _shrinkRate = 0.03f;
	[SerializeField] float _firstBeatSize = 1.03f;
	[SerializeField] float _secondBeatSize = 1.025f;

	Vector3 _baseSize;
	
	float _elapsedTime = int.MaxValue;
	bool _doubleBeatHappened = false;


	// Use this for initialization
	void Start () {
		_bombAudio.PlaySoundAtTransform("PinkNoise", this.transform);
		_baseSize = transform.localScale;
	}

	// Update is called once per frame
	void Update() {
		_elapsedTime += Time.deltaTime;
		if (Beating && _elapsedTime > Interval) {
			_doubleBeatHappened = false;
			_elapsedTime = 0;
			transform.localScale = _baseSize * _firstBeatSize;
			//_bombAudio.PlaySoundAtTransformWithRef("HeartA", this.transform);
		}
		else if (Beating && !_doubleBeatHappened && _elapsedTime > _doubleBeatTime * Interval) {
			_doubleBeatHappened = true;
			transform.localScale = _baseSize * _secondBeatSize;
			//string areyoufuckingjOking = "heartB";
			//_bombAudio.PlaySoundAtTransform(areyoufuckingjOking, this.transform);
			Debug.Log("FFS");
		}
		else if (transform.localScale.x > _baseSize.x) {
			transform.localScale *= (1f - (Time.deltaTime * _shrinkRate * Interval));
		}
		
	}
}
