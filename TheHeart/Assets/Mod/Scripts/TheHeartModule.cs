using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheHeartModule : MonoBehaviour {

	[SerializeField] KMBombInfo _bombInfo;
	[SerializeField] KMBombModule _bombModule;
	[SerializeField] BombHelper _bombHelper;
	[SerializeField] KMAudio _bombAudio;

	[Space]

	[SerializeField] HeartBeat _heartBeat;
	[SerializeField] KMSelectable _theHeartSelectable;

	float _activationTime;
	bool _zeroSecondsHit;
	int _strikes;
	int _bombTick;

	int _solvableModulesCount;
	int _modulesRemaining = -1;

	int _resets = 0;
	bool _solving = false;

	[SerializeField] float _aedChargeTime = 5f;
	[SerializeField] float _aedPlaySoundAt = 4f;
	bool _aedSoundPlaying = true;
	float _aedCharge;

	// Use this for initialization
	void Start () {
		_solvableModulesCount = _bombInfo.GetSolvableModuleIDs().Count;
		_activationTime = (int)_bombInfo.GetTime();
		StartModule();
		_bombModule.OnActivate += ActivateModule;
	}

	void ActivateModule() {
		//_aedSoundPlaying = false;
		_solvableModulesCount = _bombInfo.GetSolvableModuleIDs().Count;
		StartHeart();
	}

	void StartModule() {
		_theHeartSelectable.OnInteract += delegate { Defibrilate(); return false; };
	}
	
	void Defibrilate() {
		if (_aedCharge > 0) {
			return;
		}
		_aedCharge = _aedChargeTime;
		_aedSoundPlaying = false;
		_bombHelper.GenericButtonPress(_theHeartSelectable, false, 3);
		_bombAudio.PlaySoundAtTransform("DefibrillatorThump", this.transform);
		if (_heartBeat.Beating) {
			Debug.LogFormat("[The Heart #{0}] STRIKE!: Attempt to defibrilate a beating heart. Oof.", _bombHelper.ModuleId);
			_bombModule.HandleStrike();
			return;
		}
		_resets++;
		Debug.LogFormat("[The Heart #{0}] DEFIBRILATED at {1} seconds for the {2}th time.", _bombHelper.ModuleId, (int)_bombInfo.GetTime(), _resets);
		StartHeart();
	}

	void StartHeart() {
		_activationTime = (int)_bombInfo.GetTime();
		_heartBeat.Beating = true;
	}

	void StopHeart(string reason) {
		Debug.LogFormat("[The Heart #{0}] STOPPING at {1} seconds because {2}", _bombHelper.ModuleId, (int)_bombInfo.GetTime(), reason);
		_heartBeat.Beating = false;
		if (_solving) {
			_bombModule.HandlePass();
			Debug.LogFormat("[The Heart #{0}] SOLVED!", _bombHelper.ModuleId);
		}
	}

	// Update is called once per frame
	void Update () {
		RechargeAED();
		EvaluateStoppingConditions();
		EvaluateSolves();
	}

	void EvaluateSolves() {
		int unsolvedModules = _solvableModulesCount - _bombInfo.GetSolvedModuleIDs().Count;
		
		if (unsolvedModules != _modulesRemaining) {
			if (_modulesRemaining != -1) {
				Debug.LogFormat("[The Heart #{0}] A module was solved. {1} unsolved modules remain.", _bombHelper.ModuleId, _modulesRemaining);
			}
			_modulesRemaining = unsolvedModules;
		}
		if (!_solving && unsolvedModules <= _resets) {
			Debug.LogFormat("[The Heart #{0}] The amount of times the heart has been defibrilated equals or exceeds the amount of remaining unsolved modules. Module will be solved the next time the heart stops.", _bombHelper.ModuleId);
			_solving = true;
		}
		else if (_solving && unsolvedModules > _resets) {
			// set solving back to false because apparently something got miscalculated
			_solving = false;
		}
		// todo: Evaluate solved hearts.
	}

	void RechargeAED() {
		if (_aedCharge > 0) {
			_aedCharge -= Time.deltaTime;
		}
		if (!_aedSoundPlaying && _aedCharge < _aedPlaySoundAt) {
			_bombAudio.PlaySoundAtTransform("DefibrillatorCharging", this.transform);
			_aedSoundPlaying = true;
		}
	}

	void EvaluateStoppingConditions() {
		int timeRemaining = (int)_bombInfo.GetTime();
		if (!_heartBeat.Beating) {
			// heart is already stopped. Do nothing.
			_bombTick = (int)_bombInfo.GetTime();
			return;
		}
		
		// check for < 1 second remaining
		if (timeRemaining == 0) {
			_zeroSecondsHit = true;
			StopHeart("the timer has reached fewer than 1 seconds remaining.");
		}
		else if (_zeroSecondsHit && timeRemaining >= 1f) {
			_zeroSecondsHit = false;
		}
		// check if it's been a minute since the last defib
		if ((Mathf.Abs(_activationTime - timeRemaining)) >= 60) {
			StopHeart("the timer differs by a minute or more from when the heart last started.");
		}
		// check for strikes
		if (_bombInfo.GetStrikes() > _strikes) {
			StopHeart("a strike happened on the bomb.");
			_strikes = _bombInfo.GetStrikes();
		}
		// check for large jumps in the timer (time mode)
		if (_bombTick - timeRemaining >= 2) {
			StopHeart("The timer jumped down by more than one second, implying a strike happened somewhere");
		}
		//else if ((int)_bombInfo.GetTime() - _bombTick >= 2) {
		//	StopHeart("The timer jumped up by more than one second, implying a solved module somewhere");
		//}
		_bombTick = (int)_bombInfo.GetTime();
	}
}
