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
	int _trueFinalSatisfaction = 0;
	int _requiredSatisfaction = 0;

	public event Action OnLogDumpRequested;

	int _initialNeediness = 70;
	int _needinessPerMinute = 4;
	int _needinessPerModule = 20;
	int _satisfiedNeedinessPerPress = 8;
	int _requiredSatisfactionToSatisfy = 11;

	[Space]

	[SerializeField] MovableObject _platformMover;
	[SerializeField] MovableObject _displayMover;
	[SerializeField] Multiplier _multiplier;
	[SerializeField] Screen _screen;

	Coroutine _coroutine;

	bool _endState = false;
	bool _moduleSolved = false;

	int _updateNeedinessTime = 60;
	int _activateRewardMomentTime = 60;

	#region module solved

	public void SolveModule() {
		_moduleSolved = true;
		_multiplier.StartRandomEffect();
	}

	IEnumerator SolvedBombAnimation() {
		_platformMover.MoveToInbetween(1, duration: 2f);
		_displayMover.MoveToInbetween(1, duration: 2f);
		yield return new WaitForSeconds(2.5f);
		_platformMover.MoveToInbetween(0, speed: 0.01f);
		_displayMover.MoveToInbetween(0, speed: 0.01f);
	}

	#endregion

	void Update () {
		if (_endState || _moduleSolved) {
			if (_coroutine == null) {
				_coroutine = StartCoroutine(SolvedBombAnimation());
			}
		}

		int time = (int)_bombInfo.GetTime();
		if (_previousTime == time) {
			return;
		}
		_previousTime = time;

		// Neediness increases with every solved module
		int solves = _bombInfo.GetSolvedModuleIDs().Count;
		_neediness += (solves - _solvedModules) * _needinessPerModule;
		_accumulatedNeediness += (solves - _solvedModules) * _needinessPerModule;
		_solvedModules = solves;

		if (time % 60 == 1) {
			// Neediness increases with every minute
			_neediness += _needinessPerMinute;
			_accumulatedNeediness += _needinessPerMinute;

			// Slowly trickle satisfaction to encourage more lightups even without interaction
			_satisfaction += 6;

			// Use the neediness meter to trigger special events
			_updateNeedinessTime = UnityEngine.Random.Range(0, 60);
			_activateRewardMomentTime = UnityEngine.Random.Range(0, 60);
		}
		if (_coroutine != null) {
			return;
		}

		float solveRatio = (float)_unignoredModules != 0 ? (float)_solvedModules / (float)_unignoredModules : 1f;
		if (_initialNeedyMomentDone && solveRatio >= 0.95f) {

			int requiredSatisfaction = _accumulatedNeediness / (_satisfiedNeedinessPerPress * _requiredSatisfactionToSatisfy);
			
			float ratioSatisfied;
			if (requiredSatisfaction == 0) { ratioSatisfied = (float)_trueFinalSatisfaction / 10f; }
			else { ratioSatisfied = (float)_trueFinalSatisfaction / (float)requiredSatisfaction; }
			int percentageSatisfied = (int)(ratioSatisfied * 100f);
			OnLogDumpRequested.Invoke();
			_bombHelper.Log("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
			_bombHelper.Log("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
			_bombHelper.Log("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
			_bombHelper.Log(string.Format("The bomb is nearing the end. Over the course of the bomb, this module accumulated {0} cravings. {1} of these were satisfied ({2}%). Now going into end of bomb state.", requiredSatisfaction, _trueFinalSatisfaction, percentageSatisfied));
			// Go into final state.
			StartCoroutine(EndOfBomb(percentageSatisfied));
			_coroutine = StartCoroutine(EndOfBombPanelMovements());
			_endState = true;
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
				_bombHelper.Log(string.Format("Activating lights with a Chance over Satisfaction value of {0}/{1} (New satisfaction {2})", chance, chance + _satisfaction, _satisfaction));
				_multiplier.StartWeightedEffect(200, 100 + t, 100 + t, 100 + t, 50 + t, 0 + t, 100 + t);
				_activateRewardMomentTime = 60;
			}
		}
	}

	#region end of the bomb

	IEnumerator EndOfBomb(int percentage) {
		int bonus = Mathf.Max(0, percentage - 100);
		while (!_moduleSolved) {
			int random = UnityEngine.Random.Range(0, 100) + 15;
			if (random < percentage) {
				int r = 200 + (bonus / 4);
				int y = 150 + (bonus / 2);
				int g = 150 + (bonus / 2);
				int c = 150 + (bonus / 2);
				int b = 100 + bonus;
				int m = 50 + bonus;
				int w = 150 + bonus;
				int t = r + y + g + c + b + m + w;
				int rand = UnityEngine.Random.Range(0, t);
				float duration;
				if (rand < r) {
					// double
					duration = 15f;
					_multiplier.StartSpecific(0, int.MaxValue, duration);
				}
				else if (rand < r + y) {
					// multiply by prev
					duration = 10f;
					_multiplier.StartSpecific(1, int.MaxValue, duration);
				}
				else if (rand < r + y + g) {
					// difference with prev
					duration = 15f;
					_multiplier.StartSpecific(2, int.MaxValue, duration);
				}
				else if (rand < r + y + g + c) {
					// fixed 10
					duration = 15f;
					_multiplier.StartSpecific(3, int.MaxValue, duration);
				}
				else if (rand < r + y + g + c + b) {
					// squared
					duration = 8f;
					_multiplier.StartSpecific(4, int.MaxValue, duration);
				}
				else if (rand < r + y + g + c + b + m) {
					// minutes
					duration = 2f;
					_multiplier.StartSpecific(5, int.MaxValue, duration);
				}
				else {
					// one more than previous
					duration = 20f;
					_multiplier.StartSpecific(6, int.MaxValue, duration);
				}
				yield return new WaitForSeconds(duration);
			}
			float waitingTime = 21;
			waitingTime -= (bonus / 5);
			waitingTime = Mathf.Max(1, waitingTime);
			yield return new WaitForSeconds(waitingTime);
		}
	}

	IEnumerator EndOfBombPanelMovements() {
		while (!_moduleSolved) {
			float ratio = UnityEngine.Random.Range(0.1f,1f);
			_platformMover.MoveToInbetween(ratio, speed: 0.01f);
			_displayMover.MoveToInbetween(ratio, speed: 0.01f);
			for (int i = 0; i < 17; i++) {
				if (_moduleSolved) {
					_coroutine = null;
					yield break;
				}
				yield return new WaitForSeconds(1f);
			}
		}
		_coroutine = null;
	}

	#endregion

	#region satisfy neediness

	public void Satisfy(int value) {
		if (_endState) return;
		_satisfaction += 1;
		_finalSatisfaction++;
		_neediness -= _satisfiedNeedinessPerPress;
		if (_finalSatisfaction % _requiredSatisfactionToSatisfy == 0) {
			if (_coroutine == null && _neediness < 100 && _neediness > 10) {
				_coroutine = StartCoroutine(SatisfactionCoroutine());
			}
			_trueFinalSatisfaction++;
			//OnLogDumpRequested.Invoke();
			_bombHelper.Log("Craving satisfied :)");
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
				_neediness += _initialNeediness;
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
	}

	void Activate() {
		string[] ignoredModules = _bombBoss.GetIgnoredModules(_bombModule);
		if (ignoredModules.Length == 0) {
			ignoredModules = new string[]{ "Out of Time" };
		}
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

		_bombHelper.Log(string.Format("Ignoring {0} modules.", _ignoredModules));

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
				//_multiplier.StartRandomEffect();
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
