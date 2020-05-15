using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
	int _solvedHearts = 0;
	int _heartCounts = 0;

	int _resets = 0;
	bool _solving = false;
	bool _solved = false;
	bool _justStruck = false;

	[SerializeField] float _aedChargeTime = 5f;
	[SerializeField] float _aedPlaySoundAt = 4f;
	bool _aedSoundPlaying = true;
	float _aedCharge;

	Coroutine _heartColor;

	// Use this for initialization
	void Start() {
		// set some variables
		_activationTime = (int)_bombInfo.GetTime();
		GetModules();

		// set buttons etc
		_theHeartSelectable.OnInteract += delegate { Defibrillate(); return false; };
		_bombModule.OnActivate += ActivateModule;
		StartCoroutine(HeartSpeedRegulator());
		_heartBeat.OnColorGone += StrikeOut;
	}

	void StrikeOut(object obj, EventArgs e) {
		_bombModule.HandleStrike();
	}

	void ActivateModule() {
		//_aedSoundPlaying = false;
		GetModules();
		StartHeart();
		if (DateTime.Today.Month == 4 && DateTime.Today.Day == 1) {
			_heartBeat.RandomizeColor();
			// april fools
		}
	}

	void GetModules() {
		List<string> solvableModules = _bombInfo.GetSolvableModuleIDs();
		_solvableModulesCount = solvableModules.Count;

		int hearts = 0;
		foreach (string module in solvableModules) {
			if (module == _bombModule.ModuleType) {
				hearts++;
			}
		}
		_heartCounts = hearts;
	}

	void Defibrillate() {
		if (_aedCharge > 0) {
			return;
		}
		_aedCharge = _aedChargeTime;
		_aedSoundPlaying = false;
		_bombHelper.GenericButtonPress(_theHeartSelectable, false, 3);
		_heartBeat.WhatIsOopQuestionMark(1);
		//_bombAudio.PlaySoundAtTransform("WhyDoesThisNotWork", this.transform);
		// heart is still beating. strike.
		if (_heartBeat.Beating) {
			Debug.LogFormat("[The Heart #{0}] STRIKE!: Attempt to defibrilate a beating heart. Oof.", _bombHelper.ModuleId);
			_bombModule.HandleStrike();
			_justStruck = true;
			return;
		}
		// start heart if the moduel isnt solved yet
		if (!_solved) {
			_resets++;
			Debug.LogFormat("[The Heart #{0}] DEFIBRILATED at {1} seconds for the {2}th time.", _bombHelper.ModuleId, (int)_bombInfo.GetTime(), _resets);
			StartHeart();
		}
		else {
			_heartBeat.RandomizeColor();
		}
	}

	void StartHeart() {
		_activationTime = (int)_bombInfo.GetTime();
		_heartBeat.Beating = true;

		_justStruck = false;
		_heartBeat.ResetInterval();
		if (_heartColor != null) {
			StopCoroutine(_heartColor);
		}
		_heartColor = StartCoroutine(_heartBeat.ReplenishColor());
	}

	void StopHeart(string reason) {
		if (!_heartBeat.Beating) {
			// already stopped
			return;
		}

		Debug.LogFormat("[The Heart #{0}] STOPPING at {1} seconds because {2}", _bombHelper.ModuleId, (int)_bombInfo.GetTime(), reason);
		_heartBeat.Beating = false;
		// solve the heart if conditions apply
		if (_solving) {

			StartCoroutine(SolveDelay());
		}
		else {
			if (_heartColor != null) {
				StopCoroutine(_heartColor);
			}
			_heartColor = StartCoroutine(_heartBeat.DepleteColor());
		}
	}

	IEnumerator SolveDelay() {
		for (int i = 0; i <= _bombHelper.ModuleId % _heartCounts; i++) {
			yield return null;
		}
		if (_justStruck) {
			yield return new WaitForSeconds(1);
		}
		if (_solving) {
			_bombModule.HandlePass();
			Debug.LogFormat("[The Heart #{0}] SOLVED!", _bombHelper.ModuleId);
			_solved = true;
		}
		else {
			if (_heartColor != null) {
				StopCoroutine(_heartColor);
			}
			_heartColor = StartCoroutine(_heartBeat.DepleteColor());
		}
	}

	// Update is called once per frame
	void Update() {
		RechargeAED();
		EvaluateStoppingConditions();
		EvaluateSolves();
	}

	void EvaluateSolves() {
		if (_solved) {
			return;
		}
		if (_solvableModulesCount == 0) {
			// deal with some bug that can happen at the start where it doesn't detect modules straight away.
			_solving = false;
			return;
		}

		List<string> solvedModules = _bombInfo.GetSolvedModuleIDs();
		int unsolvedModules = _solvableModulesCount - solvedModules.Count;

		// the amount of solves has changed
		if (unsolvedModules != _modulesRemaining) {
			// check if this is module initialization
			if (_modulesRemaining != -1) {
				Debug.LogFormat("[The Heart #{0}] A module was solved. {1} unsolved modules remain.", _bombHelper.ModuleId, unsolvedModules);
			}
			// update solve amount
			_modulesRemaining = unsolvedModules;
			// check for other hearts
			int solvedHearts = 0;
			foreach (string module in solvedModules) {
				if (module == _bombModule.ModuleType) {
					solvedHearts++;
				}
			}
			if (_solvedHearts != solvedHearts) {
				_solvedHearts = solvedHearts;
				Debug.LogFormat("[The Heart #{0}] A The Heart module was solved. Now at {1} solved hearts.", _bombHelper.ModuleId, _solvedHearts);
			}
		}
		// calculate minimum reset requirement
		int minimumResetCount = _modulesRemaining + _solvedHearts * 2;
		// check if our requirement is met
		if (!_solving && minimumResetCount <= _resets) {
			Debug.LogFormat("[The Heart #{0}] Defib count >= Unsolved Modules + Solved Hearts * 2 ({1} >= {2} + {3} * 2). Module will be solved the next time the heart stops.", _bombHelper.ModuleId, _resets, _modulesRemaining, _solvedHearts);
			_solving = true;
		}
		else if (_solving && minimumResetCount > _resets) {
			Debug.LogFormat("[The Heart #{0}] Will no longer solve on the next stop; Defib count < Unsolved Modules + Solved Hearts * 2 ({1} < {2} + {3} * 2).", _bombHelper.ModuleId, _resets, _modulesRemaining, _solvedHearts);
			_solving = false;
		}
	}

	void RechargeAED() {
		if (_aedCharge > 0) {
			_aedCharge -= Time.deltaTime;
		}
		if (!_aedSoundPlaying && _aedCharge < _aedPlaySoundAt) {
			_heartBeat.WhatIsOopQuestionMark(2);
			//_bombAudio.PlaySoundAtTransform("Uuuuugh", this.transform);
			_aedSoundPlaying = true;
		}
	}

	void EvaluateStoppingConditions() {
		int timeRemaining = (int)_bombInfo.GetTime();
		if (!_heartBeat.Beating) {
			// heart is already stopped. Do nothing, but do update stuff.
			_bombTick = (int)_bombInfo.GetTime();
			_strikes = _bombInfo.GetStrikes();
			return;
		}

		// check for < 1 second remaining
		if (timeRemaining == 0 && !_zeroSecondsHit) {
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
			StopHeart("the timer jumped down by more than one second, implying a strike happened somewhere");
		}
		//else if ((int)_bombInfo.GetTime() - _bombTick >= 2) {
		//	StopHeart("The timer jumped up by more than one second, implying a solved module somewhere");
		//}
		_bombTick = (int)_bombInfo.GetTime();
	}

	IEnumerator HeartSpeedRegulator() {
		yield return new WaitForSeconds(1);
		_modulesRemaining = _solvableModulesCount;
		int interimRemaining = _modulesRemaining;
		float chance = 0;
		while (!_solved) {
			yield return null;
			if (!_heartBeat.Beating) {
				interimRemaining = _modulesRemaining;
				chance = UnityEngine.Random.Range(0f, 1f);
				continue;
			}
			if (interimRemaining != _modulesRemaining) {
				//float wait = UnityEngine.Random.Range(0, 3);
				interimRemaining = _modulesRemaining;
				//yield return new WaitForSeconds(wait);
				_heartBeat.TargetInterval *= 0.8f;
				continue;
			}
			if (interimRemaining == _modulesRemaining) {
				if (chance > 0.5) {
					yield return new WaitForSeconds(20);
					_heartBeat.TargetInterval *= 1.2f;
					chance = 0;
				}
			}
		}
	}


#pragma warning disable 414
	public readonly string TwitchHelpMessage = "'!{0} defibrillate' to defibrillate the heart. Append with seconds (00-59) to defibrillate the heart at that specific amount of seconds, eg.: '!{0} defibrillate 00' to defibrillate on the whole minute exactly.";
#pragma warning restore 414

	public IEnumerator ProcessTwitchCommand(string command) {
		command = command.ToLowerInvariant().Trim();
		List<string> split = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
		if (split[0] != "defibrilate" && split[0] != "defibrillate" && split[0] != "defib") {
			yield break;
		}
		if (split.Count == 1) {
			_theHeartSelectable.OnInteract();
		}
		else {
			for (int i = 0; i < split[1].Length; i++) {
				if (!char.IsDigit(split[1][i])) {
					yield break;
				}
			}
			int time = int.Parse(split[1]);
			if (time < 00 || time > 59) {
				yield break;
			}
			bool done = false;
			while (!done) {
				yield return null;
				int seconds = (int)_bombInfo.GetTime() % 60;
				if (seconds == time) {
					_theHeartSelectable.OnInteract();
					done = true;
				}
			}
		}
	}

	public void TwitchHandleForcedSolve() {
		StartCoroutine(ForceSolve());
	}

	IEnumerator ForceSolve() {
		yield return new WaitForSeconds(((_bombHelper.ModuleId % _heartCounts) % 539) / 9f);
		while (!_solved) {
			if (_heartBeat.Beating == true) {
				yield return new WaitForSeconds(10f);
				StopHeart("the module is being automatically solved so we will just stop the heart every 10 real-time seconds.");
			}
			if (_heartBeat.Beating == false) {
				yield return null;
				for (int i = 0; i < _bombHelper.ModuleId % _heartCounts; i++) {
					yield return null;
				}
				if (!_solved) {
					_theHeartSelectable.OnInteract();
				}
			}
			yield return null;
		}
	}
}
