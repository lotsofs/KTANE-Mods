using System.Collections;
using System.Collections.Generic;
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
				ButtonPress(button);
				return false;
			};
			sel.OnInteractEnded += () => {
				button.GetComponent<MovableObject>().SetPosition(0);
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
			_bombHelper.Log("Pressed correctly:");
			_bombHelper.Log(_pressed.Substring(_logged));
			_logged = _pressed.Length;
			_bombHelper.Log("Now at index " + _currentIndex);
			_bombHelper.Log("Expected next press: " + GridSequence.Sequence[_currentIndex]);
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
	}

	void CheckSolveCondition() {
		if (_score > _bombInfo.GetTime()) {
			_bombModule.HandlePass();
			LogDump();
			_bombHelper.Log("Next press not needed. Module got solved.");
			_solved = true;
		}
	}

	void ChangeLetter(char letter) {
		int index = _buttonLabels.IndexOf(letter);
		_buttonLabels[index] = GridSequence.Next[letter];
		_keypadButtons[index].TextMesh.text = _buttonLabels[index].ToString();
		LogDump();
		_bombHelper.Log(string.Format("Changed letter {0} to {1} on the keypad (button with value {2}).", letter, _keypadButtons[index].TextMesh.text, index + 1));
		string logKeypad = "Keypad buttons are now, in order 1-9: ";
		for (int i = 0; i < 9; i++) {
			logKeypad += _keypadButtons[i].TextMesh.text;
		}
		_bombHelper.Log(logKeypad);
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

}
