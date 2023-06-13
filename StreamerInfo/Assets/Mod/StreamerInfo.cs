using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using System.Linq;
using System.IO;
using System;

public class StreamerInfo : MonoBehaviour {

	KMBombModule[] _allMods;
	List<KMBombModule> _interactedMods;
	List<KMBombModule> _uninteractedMods;

	List<string> _allModules;
	List<string> _solvedModules;
	List<string> _unsolvedModules;

	int _previousSolvesCount = 0;
	int _previousStrikes = 0;

	int _vanillaCount = 0;
	int _interactedVanillae = 0;

	KMBombInfo _bombInfo;
	KMGameInfo _gameInfo;
	string _serial;

	string _recentlyFinishedSerial;

	string _settingsPath;

	// Use this for initialization
	void Start () {
		_bombInfo = GetComponent<KMBombInfo>();
		_gameInfo = GetComponent<KMGameInfo>();
		_gameInfo.OnStateChange += StateChange;
		MakeDirectory();
	}
	
	void StateChange(KMGameInfo.State state) {
		if (state == KMGameInfo.State.Setup || state == KMGameInfo.State.PostGame) {
			ResetValues();
		}
	} 

	void MakeDirectory() {
		_settingsPath = Path.Combine(Application.persistentDataPath, "StreamInfo");
		if (!Directory.Exists(_settingsPath)) {
			Directory.CreateDirectory(_settingsPath);
		}
		MakeFile("uninteracted.txt", "");
		MakeFile("interacted.txt", "");
		MakeFile("unsolved.txt", "");
		MakeFile("solved.txt", "");
		MakeFile("strikes.txt", "");
		MakeFile("interactcount.txt", "");
		MakeFile("solvecount.txt", "");
		MakeFile("strikecount.txt", "");
		MakeFile("lastevent.txt", "");
	}

	void MakeFile(string fileName, string items) {
		string path = Path.Combine(_settingsPath, fileName);
		File.WriteAllText(path, items);
	}

	void MakeFile(string fileName, List<string> items, string separator =  "") {
		string text = string.Join(separator, items.ToArray());
		MakeFile(fileName, text);
	}

	void MakeFile(string fileName, List<KMBombModule> items, string separator = "") {
		List<string> mods = new List<string>();
		foreach (KMBombModule mod in items) {
			mods.Add(mod.ModuleDisplayName);
		}
		string text = string.Join(separator, mods.ToArray());
		MakeFile(fileName, text);
	}

	void AppendFile(string fileName, string line) {
		string path = Path.Combine(_settingsPath, fileName);
		if (File.Exists(path)) {
			File.AppendAllText(path, Environment.NewLine + line);
		}
		else {
			Debug.LogErrorFormat("[Stream Info] File {0} does not exist...", path);
		}
	}

	void GetModules() {
		Debug.Log("[Stream Info] Grabbing Module List...");
		_allMods = FindObjectsOfType<KMBombModule>();
		_interactedMods = new List<KMBombModule>();
		_uninteractedMods = _allMods.ToList();

		foreach (KMBombModule mod in _allMods) {
			KMSelectable selectable = mod.GetComponent<KMSelectable>();

			if (selectable != null) {
				selectable.OnFocus += () => Selected(mod);
			}
			else {
				Debug.Log("[Stream Info] No KMSelectable was found on " + mod.ModuleType);
			}
		}
		_allModules = _bombInfo.GetSolvableModuleNames();
		_unsolvedModules = _bombInfo.GetSolvableModuleNames();
		_solvedModules = new List<string>();

		_vanillaCount = _allModules.Count - _allMods.Length;
		if (_vanillaCount > 0) {
			Debug.LogFormat("[Stream Info] {0} Vanillas are present", _vanillaCount);
		}

		_previousSolvesCount = _bombInfo.GetSolvedModuleIDs().Count;
		_previousStrikes = _bombInfo.GetStrikes();

		MakeFile("uninteracted.txt", _uninteractedMods, "; ");
		MakeFile("interacted.txt", _interactedMods);
		MakeFile("unsolved.txt", _unsolvedModules, "; ");
		MakeFile("solved.txt", _solvedModules);
		MakeFile("strikes.txt", "");
		MakeFile("interactcount.txt", string.Format("Interacted with: {0} out of {1}", 0, _allModules.Count));
		MakeFile("solvecount.txt", string.Format("Solved: {0} out of {1}", 0, _allModules.Count));
		MakeFile("strikecount.txt", "Strikes: 0");
		MakeFile("lastevent.txt", "Bomb Started");
	}

	void Selected(KMBombModule mod) {
		if (_uninteractedMods.Contains(mod)) {
			_interactedMods.Add(mod);
			_uninteractedMods.Remove(mod);
			MakeFile("interactcount.txt", string.Format("Interacted with: {0} out of {1}", _interactedMods.Count + _interactedVanillae, _allModules.Count));
			MakeFile("uninteracted.txt", _uninteractedMods, "; ");
		}
		string line = string.Format("{0} - {1}", _bombInfo.GetFormattedTime(), mod.ModuleDisplayName);
		AppendFile("interacted.txt", line);
	}

	void Solved(string module) {
		string line = string.Format("{0} - {1}", _bombInfo.GetFormattedTime(), module);
		AppendFile("solved.txt", line);
		MakeFile("lastevent.txt", "Module Solved");

		_unsolvedModules.Remove(module);
		MakeFile("unsolved.txt", _unsolvedModules, "; ");
		MakeFile("solvecount.txt", string.Format("Solved: {0} out of {1}", _bombInfo.GetSolvedModuleIDs().Count, _allModules.Count));

		switch(module) {
			case "Wires":
			case "The Button":
			case "Complicated Wires":
			case "Keypad":
			case "Maze":
			case "Memory":
			case "Morse Code":
			case "Password":
			case "Simon Says":
			case "Who's on First":
			case "Wire Sequence":
				_interactedVanillae++;
				MakeFile("interactcount.txt", string.Format("Interacted with: {0} out of {1}", _interactedMods.Count + _interactedVanillae, _allModules.Count));
				Debug.Log("[Stream Info] Solved a vanilla " + module);
				break;
			default:
				break;
		}
	}

	void Strike(int num) {
		string line = string.Format("{0} - {1}", _bombInfo.GetFormattedTime(), num);
		AppendFile("strikes.txt", line);
		MakeFile("strikecount.txt", "Strikes: " + num);
		MakeFile("lastevent.txt", "Strike");
	}

	// Update is called once per frame
	void Update () {
		if (_serial == null && _bombInfo.IsBombPresent()) {
			string newSerial = _bombInfo.GetSerialNumber();
			if (_recentlyFinishedSerial == newSerial) {
				// Bomb is still present even after it has exploded. Ignore it.
				return;
			}
			_serial = _bombInfo.GetSerialNumber();
			//_bombInfo.OnBombExploded += ResetValues;

			_previousSolvesCount = _bombInfo.GetSolvedModuleIDs().Count;
			GetModules();
			return;
		}
		else if (_serial == null) {
			return;
		}
		int currentSolvesCount = _bombInfo.GetSolvedModuleIDs().Count;
		if (currentSolvesCount > _previousSolvesCount) {
			_previousSolvesCount = currentSolvesCount;
			List<string> bombSolves = _bombInfo.GetSolvedModuleNames();
			foreach (string module in _solvedModules) {
				bombSolves.Remove(module);
			}
			_solvedModules.Add(bombSolves[0]);
			Solved(bombSolves[0]);
		}
		int currentStrikes = _bombInfo.GetStrikes();
		if (currentStrikes > _previousStrikes) {
			_previousStrikes = currentStrikes;
			Strike(currentStrikes);
		}

	}

	void ResetValues () {
		_recentlyFinishedSerial = _serial;
		
		_serial = null;
		_previousSolvesCount = 0;
		_previousStrikes = 0;

		_allMods = null;
		_interactedMods = null;
		_uninteractedMods = null;

		_allModules = null;
		_solvedModules = null;
		_unsolvedModules = null;

		_vanillaCount = 0;
		_interactedVanillae = 0;
	}
}
