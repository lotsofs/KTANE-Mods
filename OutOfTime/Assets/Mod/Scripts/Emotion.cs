using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Emotion : MonoBehaviour {

	KMBossModule _bombBoss;
	KMBombModule _bombModule;
	KMBombInfo _bombInfo;
	KMGameInfo _gameInfo;

	int _totalModules = 0;
	int _ignoredModules = 0;
	int _unignoredModules = 0;
	int _solvedModules = 0;

	float _totalTime = 0;
	float _relevantTime = 0;

	void Start () {
		_bombBoss = GetComponent<KMBossModule>();
		_bombModule = GetComponent<KMBombModule>();
		_bombInfo = GetComponent<KMBombInfo>();
		_gameInfo = GetComponent<KMGameInfo>();

		_gameInfo.OnLightsChange += AfraidOfTheDark;

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
		_totalTime = _bombInfo.GetTime();
		_relevantTime = percentageIgnored * _totalTime;
	}
	
	void AfraidOfTheDark(bool lightsOn) {
		if (lightsOn == false) {
			// TODO: The dark is scary. Turn on lights after a bit. 
		}
	}

	void Update () {
		
	}
}
