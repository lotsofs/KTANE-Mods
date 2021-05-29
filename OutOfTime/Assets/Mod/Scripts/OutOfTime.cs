using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OutOfTime : MonoBehaviour {

    KMBombInfo _bombInfo;
	KMBombModule _bombModule;
	KMSelectable _bombSelectable;
	BombHelper _bombHelper;
	Multiplier _multiplier;
	Emotion _emotion;

	[SerializeField] Button[] _keypadButtons;
	[SerializeField] KMSelectable _displayButton;
	[SerializeField] Screen _screen;

	[SerializeField] TextMesh _screenText;
	[SerializeField] int _deadlineOffset = 5;

	[SerializeField] Color _buttonPressColor;
	[SerializeField] Color _buttonUnpressColor;

	int _currentIndex;
	int _score = 0;
	bool _started = false;
	bool _solved = false;

	string _pressed;
	int _logged;

	List<char> _buttonLabels;
	Dictionary<int, char> _transitionQueue = new Dictionary<int, char>();

	#region setup

	// Use this for initialization
	void Awake () {
		_bombHelper = GetComponent<BombHelper>();
		_bombInfo = GetComponent<KMBombInfo>();
		_bombModule = GetComponent<KMBombModule>();
		_bombSelectable = GetComponent<KMSelectable>();
		_multiplier = GetComponent<Multiplier>();
		_emotion = GetComponent<Emotion>();
		_bombModule.OnActivate += Activate;
		_emotion.OnLogDumpRequested += LogDump;
		_multiplier.OnLogDumpRequested += LogDump;
	}

	void OnDestroy() {
		LogDump();
	}

	void Start() {
		ConfigureSelectables();
	}

	void Activate() {
		ChooseStartingPoint();
		_started = true;
	}
	
	void ChooseStartingPoint() {
		// Choose starting index
		int max = GridSequence.Sequence.Length;
		_currentIndex = UnityEngine.Random.Range(0, max);
		_bombHelper.Log(string.Format("Starting at index {0}", _currentIndex));

		// Figure out what letters came before it
		string initialSequence = "";
		for (int i = _currentIndex - 5; i < _currentIndex; i++) {
			initialSequence += GridSequence.Sequence[(i + max) % max];	// add first because modulo with negative numbers is weird
		}
		_bombHelper.Log(string.Format("Starting Sequence: {0}.", initialSequence));
		_bombHelper.Log(string.Format("Expecting {0} for the first button press.", GridSequence.Sequence[_currentIndex]));
		_pressed = initialSequence;
		_logged = initialSequence.Length;

		// Figure out what letters to display on the buttons

		_buttonLabels = new List<char>();
		int futureIndex = _currentIndex;
		while (_buttonLabels.Count < 9) {
			char letter = GridSequence.Sequence[futureIndex % max];
			if (_buttonLabels.Contains(GridSequence.Previous[letter])) {
				futureIndex++;
				continue;
			}
			if (!_buttonLabels.Contains(letter)) {
				_buttonLabels.Add(letter);
			}
			futureIndex++;
		}
		_buttonLabels = _buttonLabels.Shuffle();

		// Update displays
		string logKeypad = "Keypad buttons are, in order 1-9: ";
		for (int i = 0; i < _keypadButtons.Length; i++) {
			_keypadButtons[i].TextMesh.text = _buttonLabels[i].ToString();
			logKeypad += _keypadButtons[i].TextMesh.text;
		}
		_screen.UpdateCounter(_score);
		_bombHelper.Log(logKeypad);
	}

	void ConfigureSelectables() {
		foreach (Button button in _keypadButtons) {
			KMSelectable sel = button.Selectable;
			sel.OnInteract += () => {
				_bombHelper.GenericButtonPress(sel, true, 0.05f);
				button.GetComponent<MovableObject>().SetPosition(1);
				button.TextMesh.color = _buttonPressColor;
				ButtonPress(button);
				return false;
			};
			sel.OnInteractEnded += () => {
				button.GetComponent<MovableObject>().SetPosition(0);
				button.TextMesh.color = _buttonUnpressColor;
			};
		}
		_displayButton.OnInteract += () => {
			_bombHelper.GenericButtonPress(_displayButton, false, 0.1f);
			if (_started) _screen.ShowPresses(_pressed);
			return false;
		};
		_displayButton.OnInteractEnded += () => {
			_bombHelper.GenericButtonPress(_displayButton, false, 0.1f);
			if (_started) _screen.UpdateCounter(_score);
		};
	}

	#endregion

	void LogDump() {
		if (_pressed.Length > _logged) {
			_bombHelper.Log("============================================================");
			_bombHelper.Log("Pressed correctly: " + _pressed.Substring(_logged));
			_logged = _pressed.Length;
			_bombHelper.Log("Now at index " + _currentIndex);
			_bombHelper.Log("Current score: " + _score);
			_bombHelper.Log("Expected next press: " + GridSequence.Sequence[_currentIndex]);
			string logKeypad = "Keypad buttons are, in order 1-9: ";
			for (int i = 0; i < 9; i++) {
				logKeypad += _keypadButtons[i].TextMesh.text;
			}
			_bombHelper.Log(logKeypad);
			_bombHelper.Log("------------------------------------------------------------");
		}
	}

	void ButtonPress(Button button) {
		if (_solved || !_started) {
			return;
		}
		char letter = button.TextMesh.text[0];
		if (letter == GridSequence.Sequence[_currentIndex]) {
			ValidButtonPress(button);
		}
		else {
			LogDump();
			_bombHelper.Log("STRIKE!!: Pressed " + letter);
			_bombModule.HandleStrike();
		}
	}

	void ValidButtonPress(Button button) {
		char letter = button.TextMesh.text[0];
		_pressed += letter;
		CheckLetterTimeliness(letter);
		AddValue(button);
		CheckTransitionQueue();
		CheckSolveCondition();
		_currentIndex++;
		_currentIndex %= GridSequence.Sequence.Length;
	}

	void AddValue(Button button) {
		int value = _multiplier.Multiply(button.BaseValue);

		_score += value;
		_screen.UpdateCounter(_score);
		_multiplier.CalculateSequenceEnd(value);
		_emotion.Satisfy(button.BaseValue);
	}

	void CheckSolveCondition() {
		if (_score > _bombInfo.GetTime()) {
			LogDump();
			_bombHelper.Log("MODULE SOLVED!");
			_solved = true;
			_emotion.SolveModule();
			_bombModule.HandlePass();
		}
	}

	void ChangeLetter(char letter) {
		int index = _buttonLabels.IndexOf(letter);
		_buttonLabels[index] = GridSequence.Next[letter];
		_keypadButtons[index].TextMesh.text = _buttonLabels[index].ToString();
		//LogDump();
		//_bombHelper.Log(string.Format("Changed letter {0} to {1} on the keypad (button with value {2}).", letter, _keypadButtons[index].TextMesh.text, index + 1));
		////string logKeypad = "Keypad buttons are now, in order 1-9: ";
		//for (int i = 0; i < 9; i++) {
		//	logKeypad += _keypadButtons[i].TextMesh.text;
		//}
		//_bombHelper.Log(logKeypad);
	}

	/// <summary>
	/// Checks whether we are currently on the last occurence of a letter and changes it to the next if so.
	/// </summary>
	/// <param name="letter"></param>
	void CheckLetterTimeliness(char letter) {
		if (GridSequence.Lasts[letter] == _currentIndex) {
			// find next letter
			char letterNew = GridSequence.Next[letter];
			int deadline = GridSequence.Firsts[letterNew];
			// get the distance to the next letter
			if (deadline - _currentIndex < _deadlineOffset) {
				ChangeLetter(letter);
			}
			else {
				// queue up the transition
				int random = UnityEngine.Random.Range(_currentIndex, deadline - _deadlineOffset);
				if (!_transitionQueue.ContainsKey(random)) {
					_transitionQueue.Add(random, letter);
				}
				else {
					// if we happen to pick a random number that already has something queued, just change it now.
					ChangeLetter(letter);
				}
			}
		}
	}
	
	/// <summary>
	/// Check if we have any letters queued up to be changed.
	/// </summary>
	void CheckTransitionQueue() {
		if (_transitionQueue.ContainsKey(_currentIndex)) {
			char letter = _transitionQueue[_currentIndex];
			ChangeLetter(letter);
			_transitionQueue.Remove(_currentIndex);
		}
	}

	#region twitch plays

	public readonly string TwitchHelpMessage = "'!{0} Press ABCDFEDC' to press those letters in order. " +
		"'!{0} Display 2' to press on the display for 2 seconds (duration optional, default = 2).";

	public IEnumerator ProcessTwitchCommand(string command) {
		command = command.ToUpperInvariant().Trim();
		List<string> split = command.Split(new char[] { ' ','	' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
		if (split[0] == "SCREEN" || split[0] == "DISPLAY") {
			int duration = 2;
			if (split.Count > 1) {
				if (!int.TryParse(split[1], out duration)) {
					yield break;
				}
			}
			_displayButton.OnInteract();
			float time = duration;
			while (time > 0) {
				time -= Time.deltaTime;
				yield return "trycancel";
			}
			_displayButton.OnInteractEnded();
		}
		else if (split[0] == "PRESS") {
			if (split.Count == 1) {
				yield break;
			}
			split.RemoveAt(0);
			string buttons = string.Join("", split.ToArray());
			foreach (char button in buttons) {
				int index = _buttonLabels.IndexOf(button);
				if (index == -1) {
					yield return "sendtochat I can not find that letter on the Out of Time keypad.";
					yield break;
				}
				_keypadButtons[index].Selectable.OnInteract();
				yield return new WaitForSeconds(0.1f);
				_keypadButtons[index].Selectable.OnInteractEnded();
				yield return "trycancel";
			}
		}
	}


	IEnumerator TwitchHandleForcedSolve() {
		while (!_solved) {
			char button = GridSequence.Sequence[_currentIndex % 1000];
			int index = _buttonLabels.IndexOf(button);
			_keypadButtons[index].Selectable.OnInteract();
			yield return new WaitForSeconds(0.1f);
			_keypadButtons[index].Selectable.OnInteractEnded();
			yield return "trycancel";
			if (_currentIndex % 100 == 0) {
				// Pause to see if any other modules need anything
				yield return true;
			}
		}
	}

	#endregion

}
