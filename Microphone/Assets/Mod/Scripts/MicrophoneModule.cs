using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;

public class MicrophoneModule : MonoBehaviour {

	[SerializeField] KMBombInfo _bombInfo;
	[SerializeField] KMBombModule _bombModule;
	[SerializeField] BombHelper _bombHelper;

	[Space]

	[SerializeField] KMSelectable _volumeSelectable;
	[SerializeField] KMSelectable _recordSelectable;

	[Space]

	[SerializeField] MovableObject _volumeKnob;
	[SerializeField] GameObject _popFilter;
	[SerializeField] GameObject[] _microphones;

	[Space]

	[Range(0,1)]
	[SerializeField] float _popFilterRatio;

	int _initialKnobPosition;
	int _currentKnobPosition;
	int _micType;
	bool _popFilterEnabled;

	bool _initialRecording;
	int _deafSpot;

	bool _solved = false;
	bool _alarmOn = false;
	bool _striking = false;

	AudioSource _alarm;
	AudioSource _strike;

	// Use this for initialization
	void Start () {
		StartModule();
		_bombModule.OnActivate += ActivateModule;
		_bombInfo.OnBombExploded += delegate { ExplodedSolve(); };

		_volumeSelectable.OnInteract += delegate { ChangeKnob(); _bombHelper.GenericButtonPress(_volumeSelectable, true, 0.15f); return false; };
		_recordSelectable.OnInteract += delegate { HitButton(); _bombHelper.GenericButtonPress(_recordSelectable, true, 0.25f); return false; };
	}

	void StartModule() {
	
		// todo: perform a check on this in case the player uses a room without an alarm, or the devs patch the game.
		_alarm = GameObject.Find("alarm_clock_beep").GetComponent<AudioSource>();
		_strike = GameObject.Find("strike").GetComponent<AudioSource>();

		_initialKnobPosition = UnityEngine.Random.Range(0, 6);
		_micType = UnityEngine.Random.Range(0, _microphones.Length);
		_popFilterEnabled = UnityEngine.Random.Range(0f, 1f) < _popFilterRatio;

		_popFilter.SetActive(_popFilterEnabled);
		_microphones[_micType].gameObject.SetActive(true);
		_volumeKnob.SetPosition(_initialKnobPosition);
		_currentKnobPosition = _initialKnobPosition;

		_initialRecording = true;

		_deafSpot = CalculateDeafSpot();
	}

	/// <summary>
	/// Calculates the deaf spot according to step one of the manual
	/// </summary>
	/// <returns></returns>
	int CalculateDeafSpot() {
		int deafSpot = _initialKnobPosition;
		Debug.LogFormat("[Microphone #{0}] Volume knob initial position: {1}.", _bombHelper.ModuleId, _initialKnobPosition);
		int portCount = _bombInfo.GetPortCount(Port.StereoRCA);
		deafSpot = _initialKnobPosition - portCount;
		Debug.LogFormat("[Microphone #{0}] Subtract {1} (Stereo RCA port count) gives {2}.", _bombHelper.ModuleId, portCount, deafSpot);
		if (_popFilterEnabled) {
			deafSpot += 1;
			Debug.LogFormat("[Microphone #{0}] Add {1} (Pop filter present) gives {2}.", _bombHelper.ModuleId, 1, deafSpot);
		}
		else {
			Debug.LogFormat("[Microphone #{0}] No pop filter is present.", _bombHelper.ModuleId);
		}
		if (deafSpot < 0) {
			deafSpot *= -2;
			Debug.LogFormat("[Microphone #{0}] Multiply by {1} (Value lower than 0) gives {2}.", _bombHelper.ModuleId, -2, deafSpot);
		}
		if (deafSpot > 5) {
			deafSpot = 0;
			Debug.LogFormat("[Microphone #{0}] Set to {1} (Value higher than 5) gives {2}.", _bombHelper.ModuleId, 0, deafSpot);
		}
		Debug.LogFormat("[Microphone #{0}] Step one complete: Deaf Spot value is {1}.", _bombHelper.ModuleId, deafSpot);
		return deafSpot;
	}

	void ActivateModule() {
		if (_initialRecording) {	// add a check for this in case people strike while the lights are off.
			// TODO: make the record light start flickering
		}
	}

	void ChangeKnob() {
		_currentKnobPosition++;
		if (_currentKnobPosition > 5) {
			if (_initialRecording) {
				Debug.LogFormat("[Microphone #{0}] Strike: Volume knob is being lowered (from 5 to 0), but step two was not completed.", _bombHelper.ModuleId);
				_bombModule.HandleStrike();
			}
			_currentKnobPosition = 0;
		}
		_volumeKnob.MoveTo(_currentKnobPosition);
	}

	void HitButton() {
		if (_initialRecording) {
			Debug.LogFormat("[Microphone #{0}] Strike: Record button was used to attempt to stop the recording.", _bombHelper.ModuleId);
			_bombModule.HandleStrike();
			if (_currentKnobPosition < _deafSpot) {
				Debug.LogFormat("[Microphone #{0}] Step two: Recording was stopped by pressing the button.", _bombHelper.ModuleId);
				_initialRecording = false;
			}
		}
	}

	#region audio listeners (heh)

	/// <summary>
	/// Solve module when explosion occurs (easter egg).
	/// </summary>
	void ExplodedSolve() {
		// todo: try this with a bunch of unsolved microphones. Will all but one of them solve?
		if (_solved) {
			return;
		}
		int modulesLeft = _bombInfo.GetSolvableModuleIDs().Count - _bombInfo.GetSolvedModuleIDs().Count;
		if (modulesLeft == 1) {
			Debug.LogFormat("[Microphone #{0}] This is one sturdy microphone. Despite the bomb having blown up, the diaphragm survived.", _bombHelper.ModuleId);
		}
		else {
			Debug.LogFormat("[Microphone #{0}] Step four complete: Module disarmed. Sound used to pop diaphragm: Bomb explosion.", _bombHelper.ModuleId);
			_solved = true;
		}
	}

	/// <summary>
	/// Checks for strike sound on stage two
	/// </summary>
	void StepTwoStrike() {
		// todo: Check if this works with Double-Oh
		if (!_initialRecording) {
			// step two is already completed
			return;
		}
		if (!_striking && _strike.isPlaying) {
			_striking = true;
			if (_currentKnobPosition >= _deafSpot) {
				if (_currentKnobPosition == _deafSpot) {
					Debug.LogFormat("[Microphone #{0}] Picked up a strike sound, but the microphone's volume is not set to higher than the deaf spot. It instead is set to equal it. However, because of internal vibrations caused by the strike sound, this is sufficient.", _bombHelper.ModuleId);
				}
				else {
					Debug.LogFormat("[Microphone #{0}] Picked up a strike sound.", _bombHelper.ModuleId);
				}
				_initialRecording = false;
				Debug.LogFormat("[Microphone #{0}] Step two: Security kicked in and stopped the recording (microphone volume: {1}). Sound used: Strike.", _bombHelper.ModuleId, _currentKnobPosition);
			}
			else {
				// volume too low
				Debug.LogFormat("[Microphone #{0}] Picked up a strike sound, but the volume was set too low ({1}).", _bombHelper.ModuleId, _currentKnobPosition);
			}
		}
		else if (_striking && _strike.isPlaying) {
			_striking = false;
		}
	}

	/// <summary>
	/// Checks for alarm sound on stage two
	/// </summary>
	void StepTwoAlarm() {
		// todo: Maybe make this work with the 'bomb about to explode' alarm too.
		if (!_initialRecording) {
			// step two is already completed.
			return;
		}
		if (!_alarmOn && _alarm.isPlaying) {
			_alarmOn = true;
			if (_deafSpot == 5 && _currentKnobPosition == 5) {
				// alarm turned on, but it must stay on.
				Debug.LogFormat("[Microphone #{0}] Picked up an alarm clock, waiting 10 seconds due to the deaf spot being 5.", _bombHelper.ModuleId);
			}
			if (_deafSpot == 5 && _currentKnobPosition == 5) {
				// Volume too low for stage 5
				Debug.LogFormat("[Microphone #{0}] Picked up an alarm clock, but the volume was set too low ({1}).", _bombHelper.ModuleId, _currentKnobPosition);
			}
			if (_currentKnobPosition <= _deafSpot) {
				// Volume too low
				Debug.LogFormat("[Microphone #{0}] Picked up an alarm clock, but the volume was set too low ({1}).", _bombHelper.ModuleId, _currentKnobPosition);
			}
			else {
				// Conditions met, finish stage 2.
				_initialRecording = false;
				_alarmOn = false;
				Debug.LogFormat("[Microphone #{0}] Picked up an alarm clock.", _bombHelper.ModuleId);
				Debug.LogFormat("[Microphone #{0}] Step two: Security kicked in and stopped the recording (microphone volume: {1}). Sound used: Alarm clock.", _bombHelper.ModuleId, _currentKnobPosition);
			}
		}
		else if (_alarmOn && !_alarm.isPlaying) {
			_alarmOn = false;
			if (_deafSpot == 5 && _currentKnobPosition == 5) {
				// alarm turned off, but it must stay on.
				Debug.LogFormat("[Microphone #{0}] Alarm clock sound disappeared too soon.", _bombHelper.ModuleId);
			}
			else {
				Debug.LogFormat("[Microphone #{0}] Alarm clock sound disappeared again.", _bombHelper.ModuleId);
			}
		}
	}

	#endregion

	// Update is called once per frame
	void Update () {
		StepTwoStrike();
		StepTwoAlarm();
		
		


		//AudioSource[] abc = GameObject.FindObjectsOfType<AudioSource>();
		//List<AudioSource> dcd = new List<AudioSource>();
		//foreach (AudioSource a in abc) {
		//	if (a.isPlaying && !dcd.Contains(a)) {
		//		Debug.Log("Start: " + a.gameObject.name);
		//		dcd.Add(a);
		//	}
		//	else if (!a.isPlaying && dcd.Contains(a)) {
		//		Debug.Log("Stop: " + a.gameObject.name);
		//		dcd.Remove(a);
		//	}
		//}
	}
}
