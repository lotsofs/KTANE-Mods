using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using System;
using System.Linq;

public class MicrophoneModule : MonoBehaviour {

	[SerializeField] KMBombInfo _bombInfo;
	[SerializeField] KMBombModule _bombModule;
	[SerializeField] BombHelper _bombHelper;
	[SerializeField] KMAudio _bombAudio;

	[Space]

	[SerializeField] KMSelectable _volumeSelectable;
	[SerializeField] KMSelectable _recordSelectable;

	[Space]

	[SerializeField] MovableObject _circle;
	[SerializeField] MovableObject _volumeKnob;
	[SerializeField] GameObject _popFilter;
	[SerializeField] GameObject[] _microphones;
	[SerializeField] BlinkenLight _led;
	[SerializeField] Shake _microphonePadShaker;

	[Space]

	[Range(0, 1)]
	[SerializeField] float _popFilterRatio;

	int _initialKnobPosition;
	int _currentKnobPosition;
	int _micType;
	bool _popFilterEnabled;

	int _step;
	bool _recording;
	int _deafSpot;
	int _timerCount;
	int _timerTicks;

	bool _alarmOn = false;
	bool _striking = false;
	int _stepFourSubstep = 0;
	int _stepFourSubSubstep = 0;
	int _stepFourVolumeShouldEndAt = 0;

	List<AudioSource> _alarms = new List<AudioSource>();
	List<AudioSource> _strike = new List<AudioSource>();

	#region start of the game

	/// <summary>
	/// Start
	/// </summary>
	void Start() {
		StartModule();
		_bombModule.OnActivate += ActivateModule;
	}

	/// <summary>
	/// Find the alarm clock, configure the module, etc
	/// </summary>
	void StartModule() {
		AudioSource[] strikes = (AudioSource[])Resources.FindObjectsOfTypeAll<AudioSource>();
		AudioSource[] alarms = (AudioSource[])Resources.FindObjectsOfTypeAll<AudioSource>();
		if (alarms.Length == 0) {
			Debug.LogWarningFormat("[Microphone #{0}] ERROR: Could not locate fair audio sources in room. Auto-solving.", _bombHelper.ModuleId);
			_step = 6;
		}
		else if (strikes.Length == 0) {
			Debug.LogWarningFormat("[Microphone #{0}] ERROR: Could not locate strike source on bomb. Auto-solving.", _bombHelper.ModuleId);
			_step = 6;
		}
		else {
			foreach (AudioSource alarm in alarms) {
				if (alarm.gameObject.name == "alarm_clock_beep") {
					_alarms.Add(alarm);
				}
			}
			foreach (AudioSource strike in strikes) {
				if (strike.gameObject.name == "strike") {
					_strike.Add(strike);
				}
			}
			_step = 2;
			_recording = true;
		}

		_volumeSelectable.OnInteract += delegate { ChangeKnob(); _bombHelper.GenericButtonPress(_volumeSelectable, true, 0.15f); return false; };
		_recordSelectable.OnInteract += delegate { HitButton(); _bombHelper.GenericButtonPress(_recordSelectable, true, 0.25f); return false; };
		_bombInfo.OnBombExploded += delegate { ExplodedSolve(); };

		_initialKnobPosition = UnityEngine.Random.Range(0, 6);
		_micType = UnityEngine.Random.Range(0, _microphones.Length);
		_popFilterEnabled = UnityEngine.Random.Range(0f, 1f) < _popFilterRatio;

		_popFilter.SetActive(_popFilterEnabled);
		_microphones[_micType].gameObject.SetActive(true);
		_volumeKnob.SetPosition(_initialKnobPosition);
		_currentKnobPosition = _initialKnobPosition;

		_deafSpot = CalculateDeafSpot();

		_circle.SetPosition(_initialKnobPosition);
	}

	/// <summary>
	/// Calculates the deaf spot according to step one of the manual
	/// </summary>
	/// <returns></returns>
	int CalculateDeafSpot() {
		int deafSpot = _initialKnobPosition;
		Debug.LogFormat("[Microphone #{0}] Initial position of volume knob: {1}.", _bombHelper.ModuleId, _initialKnobPosition);
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
		Debug.LogFormat("[Microphone #{0}] ---- ", _bombHelper.ModuleId);
		return deafSpot;
	}

	/// <summary>
	/// Lights come on
	/// </summary>
	void ActivateModule() {
		if (!_recording) {
			_led.TurnOff();
		}
		else if (_recording && _alarmOn && _currentKnobPosition > _deafSpot) {
			_led.TurnOn();
		}
		else if (_step == 4) {
			_led.TurnOn();
		}
		else {
			_led.TurnBlinky();
		}
		if (_step == 6) {
			_recordSelectable.OnInteract += delegate { _bombModule.HandlePass(); return false; };
			_volumeSelectable.OnInteract += delegate { _bombModule.HandlePass(); return false; };
		}
	}

	// Update is called once per frame
	void Update() {
		//DebugTest();
		//return;
		TPUpdate();

		switch (_step) {
			case 2:
				StepTwoStrikeSound();
				StepTwoAlarmSound();
				StepTwoAlarmTimer();
				break;
			case 3:
				StepThreeStrikeSound();
				StepThreeAlarmSound();
				break;
			case 4:
				StepFourStrikeSound();
				StepFourAlarmTimer();
				break;
		}

	}

	void Strike() {
		_bombModule.HandleStrike();
	}

	#endregion

	bool AlarmIsPlaying() {
		foreach (AudioSource alarm in _alarms) {
			if (alarm.isPlaying) {
				return true;
			}
		}
		if (_tpNoise != null) {
			return true;
		}
		return false;
	}

	bool StrikeIsPlaying() {
		foreach (AudioSource strike in _strike) {
			if (strike.isPlaying) {
				return true;
			}
		}
		return false;
	}

	#region button handling

	/// <summary>
	/// Volume knob (bottom right) is pressed.
	/// </summary>
	void ChangeKnob() {
		_currentKnobPosition++;
		if (_currentKnobPosition > 5) {
			if (_step == 2 && _recording) {
				Debug.LogFormat("[Microphone #{0}] Strike: Volume knob is being lowered (from 5 to 0), but step two was not completed.", _bombHelper.ModuleId);
				Strike();
			}
			_currentKnobPosition = 0;
		}
		else if (_step == 2 && _recording && AlarmIsPlaying()) {
			StartStepThree();
			Debug.LogFormat("[Microphone #{0}] Raised volume while alarm clock is playing.", _bombHelper.ModuleId);
			Debug.LogFormat("[Microphone #{0}] Step two complete: Security kicked in and stopped the recording (microphone volume: {1}). Sound used: Alarm clock.", _bombHelper.ModuleId, _currentKnobPosition);
			Debug.LogFormat("[Microphone #{0}] ---- ", _bombHelper.ModuleId);
		}
		_volumeKnob.MoveTo(_currentKnobPosition);
	}

	/// <summary>
	/// Record button (top right) is pressed.
	/// </summary>
	void HitButton() {
		if (_step >= 5) {
			return;
		}
		// on step 2
		if (_step == 2 && _recording) {
			Debug.LogFormat("[Microphone #{0}] Strike: Record button was used to attempt to stop the recording.", _bombHelper.ModuleId);
			Strike();
			if (_currentKnobPosition < _deafSpot) {
				Debug.LogFormat("[Microphone #{0}] Step two complete: Recording was stopped by pressing the button.", _bombHelper.ModuleId);
				Debug.LogFormat("[Microphone #{0}] ---- ", _bombHelper.ModuleId);
				StartStepThree();
			}
		}
		// on step 3 first half
		else if (_step == 3 && !_recording) {
			_recording = true;
			_led.TurnBlinky();
			Debug.LogFormat("[Microphone #{0}] Recording button pressed, recording started.", _bombHelper.ModuleId);
		}
		// on step 4 or second half of step 3
		else if (_step >= 3 && _recording) {
			StartStepThree();
			Debug.LogFormat("[Microphone #{0}] Recording button pressed; microphone disabled. Returning to start of step three.", _bombHelper.ModuleId, _recording ? "on" : "off");
		}
	}

	#endregion

	#region solving

	/// <summary>
	/// The bomb exploding is a loud enough sound. Solve module :p
	/// </summary>
	void ExplodedSolve() {
		if (_step >= 5) {
			return;
		}
		int modulesLeft = _bombInfo.GetSolvableModuleIDs().Count - _bombInfo.GetSolvedModuleIDs().Count;
		if (modulesLeft == 1) {
			Debug.LogFormat("[Microphone #{0}] This is one sturdy microphone. Despite the bomb having blown up, the diaphragm survived.", _bombHelper.ModuleId);
		}
		else {
			Debug.LogFormat("[Microphone #{0}] Step four complete: Module disarmed. Sound used: Bomb explosion.", _bombHelper.ModuleId);
			_step = 6;
		}
		TPCleanup();
	}

	/// <summary>
	/// Solves the module
	/// </summary>
	void NormalSolve() {
		if (_step >= 5) {
			return;
		}
		_step = 5;
		StartCoroutine(Solving());
	}

	IEnumerator Solving() {
		yield return new WaitForSeconds(0.25f);
		_bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CapacitorPop, this.transform);
		_led.TurnOff();
		_microphonePadShaker.TurnOn(false);
		yield return new WaitForSeconds(0.5f);
		_bombModule.HandlePass();
		_step = 6;
	}

	#endregion

	#region Step Three

	/// <summary>
	/// Resets all the necessary variables when starting/returning to step three.
	/// </summary>
	void StartStepThree() {
		_striking = false;
		_recording = false;
		_alarmOn = false;
		_led.TurnOff();
		_step = 3;
		_timerTicks = -1;
		_timerCount = -1;
		_stepFourSubstep = 0;
		_stepFourSubSubstep = 0;
		_stepFourVolumeShouldEndAt = _deafSpot;
		_microphonePadShaker.TurnOn(false);
		_microphonePadShaker.SetShake(0);
	}

	/// <summary>
	/// Checks for a strike sound on step 3
	/// </summary>
	void StepThreeStrikeSound() {
		if (!_recording) {
			return;
		}
		if (_step < 3 || _step == 5) {
			// not at this step
			return;
		}
		if (!_striking && StrikeIsPlaying()) {
			_striking = true;
			if (_currentKnobPosition > _deafSpot) {
				Debug.LogFormat("[Microphone #{0}] Picked up a strike, but the recording volume is set too high ({1}). Security kicked in and stopped the recording.", _bombHelper.ModuleId, _currentKnobPosition);
				StartStepThree();
				StartCoroutine(DelayedStrike());
				Debug.LogFormat("[Microphone #{0}] Strike: Security kicked in beyond step two. Returning to start of step three.", _bombHelper.ModuleId);
			}
			else if (_currentKnobPosition < _deafSpot) {
				Debug.LogFormat("[Microphone #{0}] Picked up a strike, but the recording volume is set too low ({1}). Stopping the recording since it can't hear anything anyway.", _bombHelper.ModuleId, _currentKnobPosition);
				StartStepThree();
				StartCoroutine(DelayedStrike());
				Debug.LogFormat("[Microphone #{0}] Strike: Recording was forcefully stopped. Returning to start of step three.", _bombHelper.ModuleId);
			}
			else {
				Debug.LogFormat("[Microphone #{0}] Step three complete: Picked up sound: Strike.", _bombHelper.ModuleId);
				Debug.LogFormat("[Microphone #{0}] ---- ", _bombHelper.ModuleId);
				Debug.LogFormat("[Microphone #{0}] No special instructions required since the bomb's internal vibrations caused by the strike are sufficient.", _bombHelper.ModuleId);
				Debug.LogFormat("[Microphone #{0}] Step four complete: Module disarmed. Sound used: Strike.", _bombHelper.ModuleId);
				NormalSolve();
			}
		}
	}

	IEnumerator DelayedStrike() {
		yield return new WaitForSeconds(0.5f);
		Strike();
	}

	/// <summary>
	/// Checks if the alarm is played during step three, and starts step four if so
	/// </summary>
	void StepThreeAlarmSound() {
		if (!_recording) {
			return;
		}
		if (_step != 3) {
			// not at this step
			return;
		}
		if (!_alarmOn && AlarmIsPlaying()) {
			_alarmOn = true;
			Debug.LogFormat("[Microphone #{0}] Picked up an alarm clock.", _bombHelper.ModuleId);
			if (_currentKnobPosition > _deafSpot) {
				Debug.LogFormat("[Microphone #{0}] Picked up an alarm clock, but the recording volume is set too high ({1}). Security kicked in and stopped the recording.", _bombHelper.ModuleId, _currentKnobPosition);
				StartStepThree();
				Strike();
				Debug.LogFormat("[Microphone #{0}] Strike: Security kicked in beyond step two. Returning to start of step three.", _bombHelper.ModuleId);
			}
			else if (_currentKnobPosition < _deafSpot) {
				Debug.LogFormat("[Microphone #{0}] Picked up an alarm clock, but the recording volume is set too low ({1}). Stopping the recording since it can't hear anything anyway.", _bombHelper.ModuleId, _currentKnobPosition);
				StartStepThree();
				Strike();
				Debug.LogFormat("[Microphone #{0}] Strike: Recording was forcefully stopped. Returning to start of step three.", _bombHelper.ModuleId);
			}
			else {
				Debug.LogFormat("[Microphone #{0}] Step three complete: Picked up sound: Alarm clock.", _bombHelper.ModuleId);
				Debug.LogFormat("[Microphone #{0}] ---- ", _bombHelper.ModuleId);
				_led.TurnOn();
				_step = 4;
			}
		}
	}

	#endregion

	#region Step Two

	/// <summary>
	/// Checks for alarm sound on stage two
	/// </summary>
	void StepTwoAlarmSound() {
		if (_step != 2) {
			// step two is already completed.
			return;
		}
		if (!_alarmOn && AlarmIsPlaying()) {
			_alarmOn = true;
			if (_deafSpot == 5 && _currentKnobPosition == 5) {
				// alarm turned on, but it must stay on.
				_timerCount = (int)_bombInfo.GetTime();
				_timerTicks = 0;
				_led.TurnOn();
				Debug.LogFormat("[Microphone #{0}] Picked up an alarm clock, waiting 10 seconds due to the deaf spot being 5.", _bombHelper.ModuleId);
			}
			else if (_currentKnobPosition <= _deafSpot) {
				// Volume too low
				Debug.LogFormat("[Microphone #{0}] Picked up an alarm clock, but the volume was set too low ({1}).", _bombHelper.ModuleId, _currentKnobPosition);
			}
			else {
				// Conditions met, finish stage 2.
				StartStepThree();
				Debug.LogFormat("[Microphone #{0}] Picked up an alarm clock.", _bombHelper.ModuleId);
				Debug.LogFormat("[Microphone #{0}] Step two complete: Security kicked in and stopped the recording (microphone volume: {1}). Sound used: Alarm clock.", _bombHelper.ModuleId, _currentKnobPosition);
				Debug.LogFormat("[Microphone #{0}] ---- ", _bombHelper.ModuleId);
			}
		}
		else if (_alarmOn && !AlarmIsPlaying()) {
			_alarmOn = false;
			_led.TurnBlinky();
			if (_deafSpot == 5 && _currentKnobPosition == 5) {
				// alarm turned off, but it must stay on.
				Debug.LogFormat("[Microphone #{0}] Alarm clock sound disappeared too soon.", _bombHelper.ModuleId);
				_timerTicks = -1;
				_timerCount = -1;
			}
			else {
				Debug.LogFormat("[Microphone #{0}] Alarm clock sound disappeared again.", _bombHelper.ModuleId);
			}
		}
	}

	/// <summary>
	/// Checks if the alarm is running for long enough on step 2 if the deaf spot is 5.
	/// </summary>
	void StepTwoAlarmTimer() {
		if (_alarmOn && _deafSpot == 5 && _currentKnobPosition == 5) {
			int currentTime = (int)_bombInfo.GetTime();
			if (_timerCount != currentTime) {
				_timerTicks++;
				_timerCount = currentTime;
				if (_timerTicks >= 10) {
					StartStepThree();
					Debug.LogFormat("[Microphone #{0}] 10 seconds have elapsed.", _bombHelper.ModuleId, _currentKnobPosition);
					Debug.LogFormat("[Microphone #{0}] Step two complete: Security kicked in and stopped the recording (microphone volume: {1}). Sound used: Alarm clock.", _bombHelper.ModuleId, _currentKnobPosition);
					Debug.LogFormat("[Microphone #{0}] ---- ", _bombHelper.ModuleId);
				}
			}
		}
	}

	/// <summary>
	/// Checks for strike sound on stage two
	/// </summary>
	void StepTwoStrikeSound() {
		if (_step != 2) {
			// step two is already completed
			return;
		}
		if (!_striking && StrikeIsPlaying()) {
			_striking = true;
			if (_currentKnobPosition >= _deafSpot) {
				if (_currentKnobPosition == _deafSpot) {
					Debug.LogFormat("[Microphone #{0}] Picked up a strike, but the microphone's volume is not set to higher than the deaf spot. It instead is set to equal it. However, because of internal vibrations caused by the strike's sound, this is sufficient.", _bombHelper.ModuleId);
				}
				else {
					Debug.LogFormat("[Microphone #{0}] Picked up a strike.", _bombHelper.ModuleId);
				}
				_led.TurnOffDelay(0.7f);
				StartStepThree();
				Debug.LogFormat("[Microphone #{0}] Step two complete: Security kicked in and stopped the recording (microphone volume: {1}). Sound used: Strike.", _bombHelper.ModuleId, _currentKnobPosition);
				Debug.LogFormat("[Microphone #{0}] ---- ", _bombHelper.ModuleId);
			}
			else {
				// volume too low
				Debug.LogFormat("[Microphone #{0}] Picked up a strike, but the volume was set too low ({1}).", _bombHelper.ModuleId, _currentKnobPosition);
			}
		}
		else if (_striking && !StrikeIsPlaying()) {
			_striking = false;
		}
	}

	#endregion

	#region Step Four

	/// <summary>
	/// Goes through the special instructions (step four) if the alarm is running
	/// </summary>
	void StepFourAlarmTimer() {
		int currentTime = (int)_bombInfo.GetTime();
		if (_timerCount != currentTime) {
			_timerTicks++;
			_timerCount = currentTime;
		}
		_microphonePadShaker.TurnOn(true);
		switch (_stepFourSubstep) {
			case 0:
				_stepFourSubstep += StepFourPointOne() ? 1 : 0;
				_microphonePadShaker.SetShake(0.1f);
				break;
			case 1:
				_stepFourSubstep += StepFourPointTwo() ? 1 : 0;
				break;
			case 2:
				_stepFourSubstep += StepFourPointThree() ? 1 : 0;
				break;
			case 3:
				_stepFourSubstep += StepFourPointFour() ? 1 : 0;
				break;
			case 4:
				_stepFourSubstep += StepFourPointFive() ? 1 : 0;
				_microphonePadShaker.SetShake(_stepFourSubSubstep == 0 ? 0.2f : 0);
				break;
			case 5:
				_stepFourSubstep += StepFourPointSix() ? 1 : 0;
				_microphonePadShaker.SetShake(0.2f + Mathf.Min(0.8f, (float)_timerTicks / 10f));
				break;
			case 6:
				Debug.LogFormat("[Microphone #{0}] Step four complete: Module disarmed. Sound used: Alarm clock.", _bombHelper.ModuleId);
				NormalSolve();
				break;
		}
		if (_step == 3) {
			Debug.LogFormat("[Microphone #{0}] Returning to step three. ", _bombHelper.ModuleId);
			StartStepThree();
		}
	}

	/// <summary>
	/// checks if the alarm is still running
	/// </summary>
	/// <returns></returns>
	bool StepFourIsAlarmStillRunning() {
		if (_alarmOn && !AlarmIsPlaying()) {
			if (_tpTotalSolving.Contains(_bombHelper.ModuleId)) {
				Debug.LogFormat("[Microphone #{0}] Alarm clock sound disappeared, then immediately reappeared again.", _bombHelper.ModuleId);
				_alarmOn = false;
				return true;
			}
			_alarmOn = false;
			Debug.LogFormat("[Microphone #{0}] Alarm clock sound disappeared again.", _bombHelper.ModuleId);
			StartStepThree();
			Strike();
			Debug.LogFormat("[Microphone #{0}] Strike: Alarm clock sound disappeared too early. Returning to start of step three.", _bombHelper.ModuleId);
			return false;
		}
		return true;
	}

	/// <summary>
	/// 6) The round microphone types will break at this point. Otherwise, the sound must be played for up to 10 more seconds.
	/// </summary>
	/// <returns></returns>
	bool StepFourPointSix() {
		if (_micType == 0) {
			Debug.LogFormat("[Microphone #{0}] Microphone is round. Popping...", _bombHelper.ModuleId, _currentKnobPosition, _stepFourVolumeShouldEndAt);
			return true;
		}
		if (!StepFourIsAlarmStillRunning()) {
			return false;
		}
		if (_stepFourVolumeShouldEndAt != _currentKnobPosition) {
			Debug.LogFormat("[Microphone #{0}] Strike: Recording volume was changed to {1}, but should have remained at {2}!", _bombHelper.ModuleId, _currentKnobPosition, _stepFourVolumeShouldEndAt);
			StartStepThree();
			Strike();
			return false;
		}
		if (_timerTicks >= 10) {
			Debug.LogFormat("[Microphone #{0}] Alarm clock succesfully played into the microphone for an extended duration, as the microphone wasn't round. Popping...", _bombHelper.ModuleId, _currentKnobPosition, _stepFourVolumeShouldEndAt);
			return true;
		}
		_microphonePadShaker.SetShake(_timerTicks / 10);
		return false;
	}

	/// <summary>
	/// 5) If there is an SND indicator on the bomb, stop the sound and start it again.
	/// </summary>
	/// <returns></returns>
	bool StepFourPointFive() {
		if (!_bombInfo.IsIndicatorPresent(Indicator.SND)) {
			Debug.LogFormat("[Microphone #{0}] Skipping step four point five because there is no SND indicator.", _bombHelper.ModuleId);
			return true;
		}
		if (_stepFourVolumeShouldEndAt != _currentKnobPosition) {
			Debug.LogFormat("[Microphone #{0}] Strike: Recording volume was changed to {1}, but should have remained at {2}.", _bombHelper.ModuleId, _currentKnobPosition, _stepFourVolumeShouldEndAt);
			StartStepThree();
			Strike();
			return false;
		}
		if (_stepFourSubSubstep == 0 && !AlarmIsPlaying()) {
			_stepFourSubSubstep = 1;
			Debug.LogFormat("[Microphone #{0}] Alarm clock sound disappeared again.", _bombHelper.ModuleId);
			Debug.LogFormat("[Microphone #{0}] An SND indicator is present. Waiting for the sound to come on again.", _bombHelper.ModuleId);
			return false;
		}
		if (_stepFourSubSubstep == 1 && AlarmIsPlaying()) {
			Debug.LogFormat("[Microphone #{0}] Picked up an alarm clock again.", _bombHelper.ModuleId);
			_stepFourSubSubstep = 0;
			_timerTicks = 0;
			return true;
		}
		return false;
	}

	/// <summary>
	/// 4) If the deaf spot is 0, the volume knob must be increased by 1 until it reaches the maximum volume of 5. Do this with a speed of at most one increase per tick of the bomb's timer.
	/// </summary>
	/// <returns></returns>
	bool StepFourPointFour() {
		if (_deafSpot != 0) {
			Debug.LogFormat("[Microphone #{0}] Skipping step four point four because the deaf spot is not 0.", _bombHelper.ModuleId);
			return true;
		}
		if (!StepFourIsAlarmStillRunning()) {
			return false;
		}
		if (_stepFourSubSubstep == 0 && _currentKnobPosition == 1) {
			_stepFourSubSubstep = 1;
			Debug.LogFormat("[Microphone #{0}] Recording volume succesfully set to 1.", _bombHelper.ModuleId);
			_timerTicks = 0;
			return false;
		}
		if (_timerTicks < 1 && _currentKnobPosition != _stepFourSubSubstep) {
			Debug.LogFormat("[Microphone #{0}] Strike: Recording volume was set to {1} too early. It was meant to be set to {2} for at least one tick of the bomb's timer.", _bombHelper.ModuleId, _stepFourSubSubstep + 1, _stepFourSubSubstep);
			StartStepThree();
			Strike();
			return false;
		}
		if (_timerTicks >= 1 && _currentKnobPosition == 5 && _stepFourSubSubstep == 4) {
			Debug.LogFormat("[Microphone #{0}] Recording volume succesfully set to 5 after a tick of the bomb's timer.", _bombHelper.ModuleId);
			_stepFourSubSubstep = 0;
			_timerTicks = 0;
			_stepFourVolumeShouldEndAt = 5;
			return true;
		}
		if (_timerTicks >= 1 && _currentKnobPosition == _stepFourSubSubstep + 1) {
			_stepFourSubSubstep += 1;
			Debug.LogFormat("[Microphone #{0}] Recording volume succesfully set to {1} after a tick of the bomb's timer.", _bombHelper.ModuleId, _currentKnobPosition);
			_timerTicks = 0;
			return false;
		}
		return false;
	}

	/// <summary>
	/// 3) If the deaf spot is 1, change the volume to 5 at any point. Leave it there for at least one tick of the bomb's timer and at most three, then set it back to 1.
	/// </summary>
	/// <returns></returns>
	bool StepFourPointThree() {
		if (_deafSpot != 1) {
			Debug.LogFormat("[Microphone #{0}] Skipping step four point three because the deaf spot is not 1.", _bombHelper.ModuleId);
			return true;
		}
		if (!StepFourIsAlarmStillRunning()) {
			return false;
		}
		if (_stepFourSubSubstep == 0 && _currentKnobPosition == 5) {
			Debug.LogFormat("[Microphone #{0}] Recording volume succesfully set to 5.", _bombHelper.ModuleId);
			_stepFourSubSubstep = 1;
			_timerTicks = 0;
			return false;
		}
		if (_stepFourSubSubstep == 1 && _timerTicks < 1 && _currentKnobPosition != 5) {
			Debug.LogFormat("[Microphone #{0}] Strike: Recording volume was left on 1 for too short. It was meant to be set to 5 for at least one tick of the bomb's timer.", _bombHelper.ModuleId);
			StartStepThree();
			Strike();
			return false;
		}
		if (_stepFourSubSubstep == 1 && _timerTicks > 3 && _currentKnobPosition == 5) {
			Debug.LogFormat("[Microphone #{0}] Strike: Recording volume was left on 5 for too long. It was meant to be set to 5 for at most three ticks of the bomb's timer.", _bombHelper.ModuleId);
			StartStepThree();
			Strike();
			return false;
		}
		if (_stepFourSubSubstep == 1 && _currentKnobPosition == 1) {
			Debug.LogFormat("[Microphone #{0}] Recording volume succesfully set back to 1 after one but before four ticks of the bomb's timer.", _bombHelper.ModuleId);
			_stepFourSubSubstep = 0;
			_timerTicks = 0;
			_stepFourVolumeShouldEndAt = 1;
			return true;
		}
		return false;
	}

	/// <summary>
	/// 2) If the deaf spot is 2, change the volume to 3 after at least five ticks of the bomb's timer.
	/// </summary>
	/// <returns></returns>
	bool StepFourPointTwo() {
		if (_deafSpot != 2) {
			Debug.LogFormat("[Microphone #{0}] Skipping step four point two because the deaf spot is not 2.", _bombHelper.ModuleId);
			return true;
		}
		if (!StepFourIsAlarmStillRunning()) {
			return false;
		}
		if (_timerTicks < 5 && _currentKnobPosition != 2) {
			Debug.LogFormat("[Microphone #{0}] Strike: Recording volume was set to 3 too early. It was meant to be set to 2 for at least five ticks of the bomb's timer.", _bombHelper.ModuleId);
			StartStepThree();
			Strike();
			return false;
		}
		if (_currentKnobPosition == 3 && _timerTicks >= 5) {
			Debug.LogFormat("[Microphone #{0}] Recording volume succesfully set to 3 after five ticks of the bomb's timer.", _bombHelper.ModuleId);
			_stepFourSubSubstep = 0;
			_timerTicks = 0;
			_stepFourVolumeShouldEndAt = 3;
			return true;
		}
		return false;
	}

	/// <summary>
	/// 1) If the deaf spot is 5, change it to 4 before ten ticks of the bomb's timer have passed. Leave it there for at least one tick, then change it back to 5.
	/// </summary>
	/// <returns></returns>
	bool StepFourPointOne() {
		if (_deafSpot != 5) {
			Debug.LogFormat("[Microphone #{0}] Skipping step four point one because the deaf spot is not 5.", _bombHelper.ModuleId);
			return true;
		}
		if (!StepFourIsAlarmStillRunning()) {
			return false;
		}
		if (_stepFourSubSubstep == 0 && _timerTicks >= 10 && _currentKnobPosition != 4) {
			Debug.LogFormat("[Microphone #{0}] Strike: Recording volume was set to 5 for too long. It was meant to be set to 4 before ten ticks of the bomb's timer.", _bombHelper.ModuleId);
			StartStepThree();
			Strike();
			return false;
		}
		if (_currentKnobPosition == 4 && _stepFourSubSubstep == 0) {
			Debug.LogFormat("[Microphone #{0}] Recording volume succesfully set to 4 before ten ticks of the bomb's timer.", _bombHelper.ModuleId);
			_stepFourSubSubstep = 1;
			_timerTicks = 0;
			return false;
		}
		if (_stepFourSubSubstep == 1 && _timerTicks == 0 && _currentKnobPosition == 5) {
			Debug.LogFormat("[Microphone #{0}] Strike: Recording volume was set back to 5 too early. It was meant to be set to 4 for at least one tick of the bomb's timer.", _bombHelper.ModuleId);
			StartStepThree();
			Strike();
			return false;
		}
		if (_stepFourSubSubstep == 1 && _timerTicks > 0 && _currentKnobPosition == 5) {
			Debug.LogFormat("[Microphone #{0}] Recording volume succesfully set back to 5 after one tick of the bomb's timer.", _bombHelper.ModuleId);
			_stepFourSubSubstep = 0;
			_timerTicks = 0;
			_stepFourVolumeShouldEndAt = 5;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Striking during step 4
	/// </summary>
	void StepFourStrikeSound() {
		if (_step != 4) {
			return;
		}
		if (!_striking && StrikeIsPlaying()) {
			_striking = true;
			Debug.LogFormat("[Microphone #{0}] Picked up a strike during step four.", _bombHelper.ModuleId);
			Debug.LogFormat("[Microphone #{0}] Cancelling the special instructions currently being executed since the bomb's internal vibrations caused by the strike are sufficient.", _bombHelper.ModuleId);
			Debug.LogFormat("[Microphone #{0}] Step four complete: Module disarmed. Sound used: Strike.", _bombHelper.ModuleId);
			NormalSolve();
		}
	}

	#endregion

	#region twitch plays

#pragma warning disable 414
	public readonly string TwitchHelpMessage = "Hit the record button: !{0} record/r. Set the volume knob to 3: !{0} volume 3/v3. "
		+ "Submitting multiple commands is possible by listing them in sequence and using wait 2/w2 to wait 2 timer ticks in between. Example: \"!{0} wait 2 volume 4 w1 v5\". "
		+ "!alarm snooze not working? Use \"!{0} request twitch plays to please play a loud sound.\" (punctuation sensitive), and the module will see what it can do for you.";
	#pragma warning restore 414

	static List<int> _tpTotalSolving = new List<int>();
	static List<int> _tpReadyForSilence = new List<int>();
	static KMAudio.KMAudioRef _tpNoise = null;
	static int _tpLastSolved = -1;
	bool _tpAbort = false;

	public IEnumerator TpPlayPinkNoise() {
		Debug.LogFormat("<Microphone #{0}> Attempting to play pink noise.", _bombHelper.ModuleId);
		if (_tpNoise != null && _tpNoise.StopSound != null) {
			Debug.LogFormat("<Microphone #{0}> Stopping existing pink noise.", _bombHelper.ModuleId);
			_tpNoise.StopSound();
		}
		Debug.LogFormat("<Microphone #{0}> Attempting to play pink noise...", _bombHelper.ModuleId);
		_tpNoise = null;
		Debug.LogFormat("[Microphone #{0}] A pink noise sound is being played. This will be registered as an alarm clock.", _bombHelper.ModuleId);
		_tpNoise = _bombAudio.PlaySoundAtTransformWithRef("PinkNoise", this.transform);
		yield return new WaitForSeconds(20f);
		if (_tpNoise != null && _tpNoise.StopSound != null) {
			_tpNoise.StopSound();
		}
		_tpNoise = null;
	}

	public IEnumerator ProcessTwitchCommand(string command) {
		command = command.ToLowerInvariant().Trim();
		if (command == "request twitch plays to please play a loud sound.") {
			Debug.LogFormat("<Microphone #{0}> Received request for pink noise.", _bombHelper.ModuleId);
			StartCoroutine(TpPlayPinkNoise());
			Debug.LogFormat("<Microphone #{0}> Request granted, playing a loud sound for twenty seconds.", _bombHelper.ModuleId);
			yield return "sendtochat Request granted, playing a loud sound for twenty seconds.";
		}
		command = command.Replace("volume ", "v");
		command = command.Replace("wait ", "w");
		command = command.Replace("v ", "v");
		command = command.Replace("w ", "w");
		command = command.Replace("record", "r");
		command = command.Replace("r", "r0");
		List<string> split = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
		// validate input
		foreach (string c in split) {
			if ((c[0] != 'v' && c[0] != 'w' && c[0] != 'r') || c.Length == 1) {
				yield break;
			}
			for (int i = 1; i < c.Length; i++) {
				if (!char.IsDigit(c[i])) {
					yield break;
				}
			}
			if (c[0] == 'v') {
				string level = c.Substring(1);
				int l = int.Parse(level);
				if (l > 5) {
					yield break;
				}
			}
			if (c[0] == 'r' && c.Length > 2) {
				yield break;
			}
		}
		
		foreach (string c in split) {
			if (c[0] == 'r') {
				_recordSelectable.OnInteract();
				yield return new WaitForSeconds(0.3f);
			}
			if (c[0] == 'v') {
				string level = c.Substring(1);
				int l = int.Parse(level); 
				while (_currentKnobPosition != l) {
					yield return new WaitForSeconds(0.1f);
					_volumeSelectable.OnInteract();
				}
			}
			if (c[0] == 'w') {
				string time = c.Substring(1);
				int t = int.Parse(time);
				int previousTime = (int)_bombInfo.GetTime();
				int currentTime = (int)_bombInfo.GetTime();
				int timerTicks = 0;
				while (timerTicks != t) {
					currentTime = (int)_bombInfo.GetTime();
					if (previousTime != currentTime) {
						previousTime = currentTime;
						timerTicks++;
					}
					yield return "trycancel";
				}
			}
		}
		if (_step == 4 && _stepFourVolumeShouldEndAt == _currentKnobPosition) {
			yield return "solve";
		}
	}

	public IEnumerator TwitchHandleForcedSolve() {
		// add myself to the solving list
		_tpTotalSolving.Add(_bombHelper.ModuleId);

		// Wait for the alarm to come on
		while (!AlarmIsPlaying()) {
			yield return null;
		}

		// complete step 2
		if (_step == 2) {
			while (_currentKnobPosition != 5) {
				_volumeSelectable.OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
		}
		// check if we're still on step 2 after the previous stuff
		if (_step == 2) {
			while (_step != 3) {
				yield return true;
				yield return null;
			}
		}

		// complete step 3
		if (_step == 3) {
			while (_currentKnobPosition != _deafSpot) {
				_volumeSelectable.OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
			_recordSelectable.OnInteract();
			yield return new WaitForSeconds(0.1f);
		}

		// complete step 4
		if (_step == 4) {
			if (_tpAbort) goto cleanup;
			// 4.1
			if (_deafSpot == 5) {
				while (_currentKnobPosition != 4) {
					_volumeSelectable.OnInteract();
					yield return new WaitForSeconds(0.1f);
					if (_tpAbort) goto cleanup;
				}
				while (_timerTicks != 1) {
					yield return null;
					if (_tpAbort) goto cleanup;
				}
				while (_currentKnobPosition != 5) {
					_volumeSelectable.OnInteract();
					yield return new WaitForSeconds(0.1f);
					if (_tpAbort) goto cleanup;
				}
			}
			// 4.2
			if (_deafSpot == 2) {
				while (_currentKnobPosition != 3) {
					while (_timerTicks != 5) {
						yield return null;
						if (_tpAbort) goto cleanup;
					}
					_volumeSelectable.OnInteract();
					yield return new WaitForSeconds(0.1f);
					if (_tpAbort) goto cleanup;
				}
			}
			// 4.3
			if (_deafSpot == 1) {
				while (_currentKnobPosition != 5) {
					_volumeSelectable.OnInteract();
					yield return new WaitForSeconds(0.1f);
					if (_tpAbort) goto cleanup;
				}
				while (_timerTicks != 1) {
					yield return null;
					if (_tpAbort) goto cleanup;
				}
				while (_currentKnobPosition != 1) {
					_volumeSelectable.OnInteract();
					yield return new WaitForSeconds(0.1f);
					if (_tpAbort) goto cleanup;
				}
			}
			// 4.4
			if (_deafSpot == 0) {
				_volumeSelectable.OnInteract();
				yield return new WaitForSeconds(0.1f);
				if (_tpAbort) goto cleanup;
				while (_currentKnobPosition != 5) {
					yield return null;
					if (_timerTicks != 0) {
						_volumeSelectable.OnInteract();
						yield return new WaitForSeconds(0.1f);
					}
					if (_tpAbort) goto cleanup;
				}
			}
			if (_tpAbort) goto cleanup;
			// the module solos from here, it no longer requires active input
			StartCoroutine(ContinueForceSolve());
			while (_step != 6) {
				yield return true;
			}
			// wait for the module to actually solve
			yield break;
		}
	cleanup:
		TpModuleDone();
		// wait otherwise tp will force solve it before it's solved.
	}

	IEnumerator ContinueForceSolve() {
		// 4.5
		if (_bombInfo.IsIndicatorPresent(Indicator.SND)) {
			_tpReadyForSilence.Add(_bombHelper.ModuleId);
			while (AlarmIsPlaying()) {
				yield return WaitForAlarmState(false);
			}
			_tpReadyForSilence.Remove(_bombHelper.ModuleId);
			while (!AlarmIsPlaying()) {
				yield return WaitForAlarmState(true);
			}
		}
		// 4.6
		if (_micType == 1) {
			while (_step < 5) {
				if (_tpAbort) goto cleanup;
				yield return null;
			}
		}
	cleanup:
		TpModuleDone();
	}

	void TpModuleDone() {
		if (_tpReadyForSilence.Contains(_bombHelper.ModuleId)) {
			_tpReadyForSilence.Remove(_bombHelper.ModuleId);
		}
		if (_tpTotalSolving.Contains(_bombHelper.ModuleId)) {
			_tpTotalSolving.Remove(_bombHelper.ModuleId);
			_tpLastSolved = _bombHelper.ModuleId;
		}
	}

	IEnumerator WaitForAlarmState(bool desiredState) {
		while (AlarmIsPlaying() != desiredState) {
			yield return null;
		}

	}

	/// <summary>
	/// Checks each frame whether the alarm needs to be turned on/off for twitch plays autosolve
	/// </summary>
	public void TPUpdate() {
		// check if we need to do this at all
		if (_tpTotalSolving.Count == 0 && _tpLastSolved != _bombHelper.ModuleId) {
			// there's no microphoens in the autosolve queue, and this one is also not the last one to be removed from it.
			// this means we don't need to do anything else.
			return;
		}
		
		// check if im the leader and the alarm is playing, to shut it off
		if (_tpTotalSolving.Count == 0 && _tpLastSolved == _bombHelper.ModuleId) {
			// no autosolve queue, but we were the last one to be removed from it.
			// it is our duty to turn off the alarm if it's running.
			if (_tpNoise != null && _tpNoise.StopSound != null) {
				_tpNoise.StopSound();
			}
			_tpNoise = null;
			_tpLastSolved = -1;
			return;
		}

		// check if i'm detecting a strike
		// is the bomb striking or did the module somehow pass elsewise?
		if (!_tpAbort && _striking) {
			_tpAbort = true;
			return;
		}

		// check if im the leader and silence is needed
		// do we need silence?
		if (_tpReadyForSilence.Count == _tpTotalSolving.Count) {
			if (AlarmIsPlaying()) {
				if (_tpNoise != null && _tpNoise.StopSound != null) {
					_tpNoise.StopSound();
				}
				_tpNoise = null;
			}
			return;
		}

		// check if im the leader but the alarm needs to play
		// only one script needs to restart the alarm. Am I it?
		if (_tpTotalSolving[0] != _bombHelper.ModuleId) {
			return;
		}
		TurnOnTpAlarm();
	}

	/// <summary>
	/// Turns the tp autosolve alarm on
	/// </summary>
	/// <param name="on"></param>
	public void TurnOnTpAlarm() {
		if (!AlarmIsPlaying()) {
			if (_tpNoise != null && _tpNoise.StopSound != null) {
				_tpNoise.StopSound();
			}
			_tpNoise = null;
			_tpNoise = _bombAudio.PlaySoundAtTransformWithRef("PinkNoise", this.transform);
		}
	}

	/// <summary>
	/// Checks if a strike occured, or the actual alarm went off, while TP autosolve was active, and fixes the mess it caused.
	/// </summary>
	public void TPCleanup() {
		_tpTotalSolving.Clear();
		StopAllCoroutines();
		if (_tpNoise != null && _tpNoise.StopSound != null) {
			_tpNoise.StopSound();
		}
		_tpNoise = null;
	}

	#endregion

	#region test stuff, do not run

	AudioSource[] testA;
	List<AudioSource> testB = new List<AudioSource>();

	/// <summary>
	/// debug code, do not run.
	/// </summary>
	void DebugTest() {
		if (testA == null) {
			testA = GameObject.FindObjectsOfType<AudioSource>();
		}

		foreach (AudioSource a in testA) {
			if (a.isPlaying && !testB.Contains(a)) {
				Debug.Log("Start: " + a.gameObject.name + " @ " + string.Format("{0} {1} {2}", a.transform.position.x, a.transform.position.y, a.transform.position.z));
				testB.Add(a);
			}
			else if (!a.isPlaying && testB.Contains(a)) {
				Debug.Log("Stop: " + a.gameObject.name + " @ " + string.Format("{0} {1} {2}", a.transform.position.x, a.transform.position.y, a.transform.position.z));
				testB.Remove(a);
			}
		}
	}

	#endregion
}