using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Emotion : MonoBehaviour {

	KMBossModule _bombBoss;
	KMBombModule _bombModule;
	KMBombInfo _bombInfo;
	KMGameInfo _gameInfo;
	BombHelper _bombHelper;

	bool _started = false;

	int _totalModules = 0;
	int _ignoredModules = 0;
	int _unignoredModules = 0;
	int _solvedModules = 0;

	float _totalTime = 0;
	float _relevantTime = 0;
	int _previousTime = 0;

	int _neediness = 0;
	int _accumulatedNeediness = 0;
	bool _initialNeedyMomentDone = false;

	int _satisfaction = 0;
	int _finalSatisfaction = 0;
	int _requiredSatisfaction = 0;

	public event Action OnLogDumpRequested;

	[SerializeField] int _initialSatisfaction = 10;
	[SerializeField] int _needinessPerMinute = 5;
	[SerializeField] int _needinessPerModule = 15;
	[SerializeField] int _satisfiedNeedinessPerPress = 3;
	[SerializeField] int _requiredSatisfactionToSatisfy = 33;

	[Space]

	[SerializeField] MovableObject _platformMover;
	[SerializeField] MovableObject _displayMover;
	[SerializeField] Multiplier _multiplier;
	[SerializeField] Screen _screen;

	Coroutine _coroutine;

	int _updateNeedinessTime = 60;
	int _activateRewardMomentTime = 60;

	void Update () {

		// TODO: When there are no other modules left, go into boss mode.

		int time = (int)_bombInfo.GetTime();
		if (_previousTime == time) {
			return;
		}
		_previousTime = time;
		if (time % 60 == 1) {
			// Neediness increases with every solved module
			int solves = _bombInfo.GetSolvedModuleIDs().Count;
			_neediness += (solves - _solvedModules) * _needinessPerModule;
			_accumulatedNeediness += (solves - _solvedModules) * _needinessPerModule;
			_solvedModules = solves;
			// Neediness increases with every minute
			_neediness += _needinessPerMinute;
			_accumulatedNeediness += _needinessPerMinute;

			// Use the neediness meter to trigger special events
			_updateNeedinessTime = UnityEngine.Random.Range(0, 60);
			_activateRewardMomentTime = UnityEngine.Random.Range(0, 60);
		}
		if (_coroutine != null) {
			return;
		}
		if (time % 60 == _updateNeedinessTime) {
			if (!_initialNeedyMomentDone) {
				InitialNeedyMoment();
			}
			else {
				if (UnityEngine.Random.Range(0, 2) == 1 && UnityEngine.Random.Range(0, 100) < _neediness) {
					_coroutine = StartCoroutine(ShowMaxNeediness());
				}
				else {
					_coroutine = StartCoroutine(ShowNeediness());
				}
			}
			_updateNeedinessTime = 60;
		}
		if (_coroutine != null) {
			return;
		}
		if (time % 60 == _activateRewardMomentTime) {
			int chance = UnityEngine.Random.Range(0, 1000);
			if (chance < _satisfaction) {
				_satisfaction -= chance;
				if (_satisfaction < 0) _satisfaction = 0;
				int t = Mathf.Min(_satisfaction/10, 100);
				OnLogDumpRequested.Invoke();
				_bombHelper.Log("Activating lights.");
				_multiplier.StartWeightedEffect(200, 100 + t, 100 + t, 100 + t, 50 + t, 0 + t, 100 + t);
				_activateRewardMomentTime = 60;
			}
		}
	}

	#region satisfy neediness

	public void Satisfy(int value) {
		_satisfaction += value;
		_neediness -= _satisfiedNeedinessPerPress;
		if (_satisfaction % _requiredSatisfactionToSatisfy == 0) {
			if (_coroutine == null) {
				_coroutine = StartCoroutine(SatisfactionCoroutine());
			}
			_finalSatisfaction++;
			OnLogDumpRequested.Invoke();
			_bombHelper.Log("Cravings satisfied :)");
			_screen.ShowSmiley();
		}
	}

	IEnumerator SatisfactionCoroutine() {
		_displayMover.MoveToInbetween(0.1f, duration: 5f);
		_platformMover.MoveToInbetween(0.4f, duration: 2f);
		yield return new WaitForSeconds(2.5f);
		_platformMover.MoveToInbetween(0.1f, duration: 2f);
		yield return new WaitForSeconds(3f);
		_coroutine = null;
	}

	#endregion

	#region display of neediness

	IEnumerator ShowNeediness() {
		float shownNeediness = _neediness / 100f;
		shownNeediness = Mathf.Max(Mathf.Min(shownNeediness, 0.9f),0.1f);
		_platformMover.MoveToInbetween(shownNeediness, speed: 0.01f);
		_displayMover.MoveToInbetween(shownNeediness, speed: 0.01f);
		yield return null;
		yield return new WaitForSeconds(3f);
		_coroutine = null;
	}

	IEnumerator ShowMaxNeediness() {
		_platformMover.MoveToInbetween(1, speed: 0.01f);
		_displayMover.MoveToInbetween(1, speed: 0.01f);
		yield return null;
		yield return new WaitForSeconds(3f);
		_coroutine = null;
	}

	#endregion

	#region initial neediness

	void InitialNeedyMoment() {
		if (!_started) return;
		if (_coroutine == null) {
			if (UnityEngine.Random.Range(0f,1f) > 0.25f) {
				_satisfaction += _initialSatisfaction;
				_coroutine = StartCoroutine(InitialNeedyMomentCoroutine());
				_initialNeedyMomentDone = true;
			}
		}
	}

	IEnumerator InitialNeedyMomentCoroutine() {
		OnLogDumpRequested.Invoke();
		_bombHelper.Log("Performing initial Cravings Routine");
		_displayMover.MoveToInbetween(0.9f, duration: 4f);
		_platformMover.MoveToInbetween(0.2f, duration: 2f);
		yield return new WaitForSeconds(2.5f);
		_multiplier.StartRandomEffect();
		_platformMover.MoveToInbetween(0.9f, duration: 5f);
		yield return new WaitForSeconds(5f);
		_coroutine = null;
	}

	#endregion

	#region initmood

	void Start () {
		_bombBoss = GetComponent<KMBossModule>();
		_bombModule = GetComponent<KMBombModule>();
		_bombInfo = GetComponent<KMBombInfo>();
		_gameInfo = GetComponent<KMGameInfo>();
		_bombHelper = GetComponent<BombHelper>();

		_gameInfo.OnLightsChange += AfraidOfTheDark;
		_bombModule.OnActivate += Activate;

		_platformMover.SetPosition(0);
		_displayMover.SetPosition(0);
		_multiplier.Disable();
	}

	void Activate() {
		string[] ignoredModules = _bombBoss.GetIgnoredModules(_bombModule);
		// dont really care what the modules are, just how many there are.
		List<string> presentModules = _bombInfo.GetSolvableModuleNames();
		_totalModules = presentModules.Count;
		foreach (string item in presentModules) {
			if (ignoredModules.Contains(item)) {
				_ignoredModules++;
			}
		}
		_unignoredModules = _totalModules - _ignoredModules;
		float percentageIgnored = _ignoredModules / _totalModules;

		// leave some time in for boss solves, so don't eat all the time.
		// TODO: A more complicated calculation to deal with longer boss modules.
		// TODO: This is probably an obsolete todo. Just go into boss mode when there's nothing but bosses left.
		_totalTime = _bombInfo.GetTime();
		_relevantTime = percentageIgnored * _totalTime;

		_multiplier.Disable();
		_started = true;

		if (_coroutine == null) {
			_coroutine = StartCoroutine(ComeAlive());
		}
	}

	#endregion

	#region come alive

	IEnumerator ComeAlive() {
		float randomStart = UnityEngine.Random.Range(0.3f, 0.7f);
		_platformMover.MoveToInbetween(randomStart, duration: 4f);
		_displayMover.MoveToInbetween(randomStart, duration: 3f);
		yield return new WaitForSeconds(4f);
		_coroutine = null;
	}

	#endregion

	#region afraid of the dark

	[ContextMenu("Test 'Afraid Of The Dark'")]
	void AfraidOfTheDark() {
		AfraidOfTheDark(false);
	}

	void AfraidOfTheDark(bool lightsOn) {
		if (lightsOn == false) {
			if (_coroutine == null) {
				_multiplier.StartRandomEffect();
				OnLogDumpRequested.Invoke();
				_bombHelper.Log("The dark is scary. Turning on my lights.");
				_coroutine = StartCoroutine(AfraidOfTheDarkCoroutine());
			}
		}
	}

	IEnumerator AfraidOfTheDarkCoroutine() {
		_platformMover.MoveToInbetween(0.55f, duration: 1f);
		_displayMover.MoveToInbetween(0.85f, duration: 3.5f);
		yield return new WaitForSeconds(2f);
		_platformMover.MoveToInbetween(0.85f, duration: 2f);
		yield return new WaitForSeconds(2f + UnityEngine.Random.Range(0f, 1f));
		_multiplier.StartRandomEffect();
		yield return new WaitForSeconds(1f);
		_displayMover.MoveToInbetween(0.75f, duration: 1.2f);
		_platformMover.MoveToInbetween(0.75f, duration: 1f);
		yield return new WaitForSeconds(1.2f);
		_coroutine = null;
	}

	#endregion
}
