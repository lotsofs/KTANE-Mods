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
	bool _initialNeedyMomentDone = false;

	[SerializeField] int _initialNeediness = 90;
	[SerializeField] int _needinessPerMinute = 5;
	[SerializeField] int _needinessPerModule = 15;

	[Space]

	[SerializeField] MovableObject _platformMover;
	[SerializeField] MovableObject _displayMover;
	[SerializeField] Multiplier _multiplier;

	Coroutine _coroutine;

	int _updateNeedinessTime = 60;
	int _activateRewardMomentTime = 60;

	public void Satisfy(int value) {
		// TODO: Make it so neediness can be satisfied.
	}

	void Update () {
		int time = (int)_bombInfo.GetTime();
		if (_previousTime == time) {
			return;
		}
		_previousTime = time;
		if (time % 60 == 0) {
			if (!_initialNeedyMomentDone && _coroutine == null) InitialNeedyMoment();
			// Neediness increases with every solved module
			int solves = _bombInfo.GetSolvedModuleIDs().Count;
			_neediness += (solves - _solvedModules) * _needinessPerModule;
			_solvedModules = solves;
			// Neediness increases with every minute
			_neediness += _needinessPerMinute;

			// TODO: Use the neediness meter to trigger special events
			_updateNeedinessTime = UnityEngine.Random.Range(0, 60);
			_activateRewardMomentTime = UnityEngine.Random.Range(0, 60);
		}
		if (time % 60 == _updateNeedinessTime) {
			if (_coroutine == null) {
				_coroutine = StartCoroutine(ShowNeediness());
				_updateNeedinessTime = 60;
			}
		}
		if (time % 60 == _activateRewardMomentTime) {
			if (UnityEngine.Random.Range(0, 100) < _neediness) {

				_activateRewardMomentTime = 60;
			}
		}
	}

	IEnumerator ShowNeediness() {
		float shownNeediness = _neediness / 100f;
		shownNeediness = Mathf.Min(shownNeediness, 1);
		_platformMover.MoveToInbetween(shownNeediness, speed: 0.01f);
		_displayMover.MoveToInbetween(shownNeediness, speed: 0.01f);
		yield return null;
		yield return new WaitForSeconds(10f);
		_coroutine = null;
	}

	void InitialNeedyMoment() {
		if (!_started) return;
		if (_coroutine == null) {
			if (UnityEngine.Random.Range(0f,1f) > 0.25f) {
				_neediness += _initialNeediness;
				_coroutine = StartCoroutine(InitialNeedyMomentCoroutine());
				_initialNeedyMomentDone = true;
			}
		}
	}

	IEnumerator InitialNeedyMomentCoroutine() {
		_bombHelper.Log("Performing initial Neediness Routine");
		_displayMover.MoveToInbetween(0.9f, duration: 4f);
		_platformMover.MoveToInbetween(0.2f, duration: 2f);
		yield return new WaitForSeconds(2.5f);
		_multiplier.StartRandomEffect();
		_platformMover.MoveToInbetween(0.9f, duration: 5f);
		yield return new WaitForSeconds(5f);
		_coroutine = null;
	}

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
				_bombHelper.Log("The dark is scary");
				_coroutine = StartCoroutine(AfraidOfTheDarkCoroutine());
			}
		}
	}

	IEnumerator AfraidOfTheDarkCoroutine() {
		_platformMover.MoveToInbetween(0.55f, duration: 1f);
		_displayMover.MoveToInbetween(0.85f, duration: 3.5f);
		yield return new WaitForSeconds(2f);
		_platformMover.MoveToInbetween(0.85f, duration: 2f);
		yield return new WaitForSeconds(2f + Random.Range(0f, 1f));
		_multiplier.StartRandomEffect();
		yield return new WaitForSeconds(1f);
		_displayMover.MoveToInbetween(0.75f, duration: 1.2f);
		_platformMover.MoveToInbetween(0.75f, duration: 1f);
		yield return new WaitForSeconds(1.2f);
		_coroutine = null;
	}

	#endregion
}
