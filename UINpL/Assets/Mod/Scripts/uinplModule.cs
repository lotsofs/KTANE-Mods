using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using System.Linq;

public class uinplModule : MonoBehaviour {

	public UinplSettings settings;

	public float onTime = 1f;
	public float offTime = 0.3f;
	public float letterPercentage = 0.3f;
	public float resumeShuffleRatio = 0.2f;

	[SerializeField] Slogans _textDisplay;

	TextMesh[] _buttonLabels = new TextMesh[24];
	Coroutine _coroutine;
	string _serialNumber;

	private bool _halted = false;
	private bool _solved = false;
	private int _serialUinIndex;
	private bool _serialBuilt = false;

	private char _oldChar = '-';
	private char _newChar = '-';
	private int _changedCharIndex;
	private int _solution;

	private KMSelectable[] _buttons;

	KMBombInfo _bombInfo;
	KMBombModule _bombModule;
	BombHelper _bombHelper;

	static readonly string Letters = "abcdefghijklmnopqrstuvwxyz";
	static readonly string Numbers = "0123456789";

	static readonly string Conversions = "0123456789abcdefghijklmnopqrstuvwxyz";

	static readonly int[,] Solutions = new int[36, 36] {
		{ 0,  3, 17, 12,  6,  2, 24, 22,  2, 15,  6, 15, 11, 15,  3, 23,  7,  5, 10,  2, 11, 21, 19, 18,  6,  4, 10, 16, 14, 13,  4,  2,  5, 24, 22,  8},
		{11,  0, 10, 20, 18,  7,  1, 12, 11,  5, 11,  4, 20,  2,  9,  3,  1, 13, 24, 21, 12,  1, 14,  9,  7, 20,  3, 13, 16, 24, 18, 21,  9, 21, 10,  9},
		{16, 21,  0,  4,  5,  8, 10, 22, 13, 23,  7,  1,  7, 15,  3,  5, 12, 11,  6, 19, 15,  3, 13, 17,  9,  4, 17, 20, 13,  6, 22, 10, 13, 20,  9,  6},
		{ 4, 12, 16,  0, 19, 24,  6,  8, 16,  4,  8, 23, 13,  9, 18, 24,  5,  7, 22,  4,  4,  9,  8, 16, 20,  5,  8,  9, 16,  6, 19,  9,  2,  4,  1, 11},
		{ 6, 12, 20, 15,  0,  2, 21, 10,  8, 21, 19,  8, 12, 21, 23,  9,  1, 20,  1, 14, 16, 18, 12,  8,  8, 11, 15, 19, 19, 22, 10, 11, 11,  8,  3,  2},
		{17, 13,  5,  6, 12,  0,  6,  3, 23, 19,  5, 12, 17, 10, 22,  8,  6, 12,  7,  2, 17, 14, 17,  6, 17,  4, 16, 24, 17,  5,  9,  5,  7,  3, 13, 12},
		{19, 11,  4,  2,  9,  9,  0, 14, 15,  1, 23,  2, 16,  2,  8, 20, 20,  4, 15, 14, 12,  3,  4,  4,  5, 14, 13, 18, 14, 19,  2, 17, 18,  3, 13, 24},
		{10,  9, 16, 14, 16, 14, 13,  0, 17,  8, 23, 23, 21, 23, 15, 16,  9,  7, 10,  8,  3, 21,  3, 12, 14,  2,  1,  1,  7, 10, 23, 10,  4,  9, 21, 18},
		{ 2,  4,  5,  8, 14, 13, 20, 24,  0, 12, 20, 17, 10,  5,  9,  5, 17, 13, 23, 13,  7, 10,  6,  1, 13, 13, 13, 21, 17,  1, 22,  8,  9,  6, 14, 23},
		{16, 18, 13, 18, 22, 18,  5,  7, 20,  0,  9, 14, 11, 18,  5, 22, 23,  1, 22, 23,  1,  1,  8, 20, 12, 11, 20,  9, 13, 20, 14,  6, 11, 22,  2, 11},
		{16, 18,  4,  6,  4, 19,  4, 11, 24,  5,  0,  1, 12,  1,  5, 18,  3, 10,  7, 23, 13,  2, 18, 10, 13, 17, 10, 21, 13, 15,  7,  9, 16,  7, 20, 18},
		{18, 19, 17,  5, 23,  9, 17, 24,  6, 15, 22,  0,  4,  3, 24, 11, 15, 10, 10, 20,  6, 24, 19, 13, 21,  7, 24, 10,  2,  5, 16,  3,  9, 22,  3, 23},
		{12, 10,  4, 23, 17, 14,  3, 18,  3, 11, 16,  4,  0,  3, 19, 12, 19,  4,  2, 11, 11, 15,  7,  4, 12, 17, 21, 14, 13,  9,  1, 13, 12,  7, 18, 24},
		{14, 11,  7, 18, 18,  9, 24, 16, 19,  3, 20,  5,  1,  0,  4,  6,  4,  9,  7,  3, 13,  3,  1, 24, 18,  5, 21,  6,  3,  1,  9,  6,  6, 23, 15, 16},
		{12, 12,  6,  9, 19, 12,  5, 19,  8,  1,  7, 22,  8, 10,  0, 22, 11,  8,  8, 17,  4,  5, 13,  8, 17, 14, 22, 19, 20,  2, 11, 19,  2, 12, 20, 14},
		{ 1, 24, 11,  1, 10,  9,  1,  1, 24, 15,  8, 15,  4,  9, 18,  0, 15, 12, 23, 12, 22,  9, 21,  7, 13,  2, 20,  1, 18, 13,  8, 19, 18, 22,  5,  3},
		{ 9, 20, 16,  1,  6, 18, 22,  6, 17, 19,  3,  4, 10, 23, 12,  9,  0,  5,  1, 22, 17, 19, 17,  5,  3,  8, 11,  3,  7,  2, 19, 21,  8,  3, 14,  6},
		{23,  5,  9, 11,  4, 18,  2, 17,  1, 13, 18, 14,  5, 11, 24,  6, 14,  0,  9,  4, 15, 18, 20,  5, 23,  1, 23, 16,  6, 21, 21, 17, 21, 13, 20,  6},
		{24,  3,  3,  5, 12,  4, 13, 24, 18,  6, 17, 13,  2,  7, 19, 11, 16, 19,  0, 17, 10,  5, 24, 18,  1, 19,  5, 18,  4, 17, 10, 16, 24,  1, 14, 17},
		{ 6, 15, 24,  6, 10, 16,  3, 10, 18,  3, 10,  4, 18,  2, 13, 16, 13, 20, 16,  0,  7, 22, 17, 15, 24, 11, 12,  6, 10,  8, 18,  7,  2, 15,  8, 10},
		{11, 19, 10,  1, 15,  3,  7, 14,  9,  9,  4,  5, 22, 16, 23,  5, 12,  8,  4,  8,  0,  9, 19, 17,  2,  9, 14,  2,  8, 24, 18, 22, 24,  9, 20, 16},
		{ 7,  9,  6, 13,  1, 15, 16, 20,  8,  3, 20, 14,  3, 22,  2, 13,  9,  2,  7,  2,  5,  0, 10, 18, 20, 21, 15, 12, 21,  9, 16, 14,  9, 15, 24,  4},
		{ 5, 19, 12,  7, 24,  1, 24, 24,  5, 11, 20, 19, 18, 17,  6, 21,  3, 10,  9, 12, 14, 12,  0,  3,  6,  7,  5,  1, 13,  2, 19, 22,  2, 19, 18, 18},
		{ 6,  4, 10,  4, 17,  7,  4, 18, 23, 20,  7, 23, 16, 16, 24,  8,  9, 13, 19,  9, 20, 20,  4,  0,  8, 17, 24,  2, 24, 11, 13, 21, 13, 13, 15, 14},
		{ 9, 21, 14, 18,  7, 17, 20, 15,  2, 20, 12, 20, 23,  2,  9, 15,  1, 21, 16, 20, 14,  4,  7, 21,  0,  7,  6,  7,  1,  1,  9,  1, 11, 20,  9,  4},
		{ 6, 10,  2,  9,  5, 10,  5, 17,  6, 14,  8, 20, 24, 14, 22, 18, 17, 13, 23,  4, 20, 22,  2,  3, 17,  0, 15, 23, 20, 18,  7,  5,  2,  8,  5,  1},
		{18,  9, 13, 10, 24, 14,  8, 19,  7, 15,  8,  1, 17, 14,  3, 16, 17, 14,  6, 10, 11, 11,  3,  2, 16, 10,  0, 14, 15,  7,  3, 15, 20, 23, 22,  3},
		{ 7, 12, 23,  9, 24, 23,  7, 22,  1, 23,  3, 11, 19, 12, 15, 18,  8,  4, 11, 15,  8,  4, 12, 15,  8, 18, 17,  0, 14, 16, 10, 21, 17,  8, 14,  8},
		{ 7,  3, 18, 17, 12, 18, 14, 10,  5, 11, 16, 11,  2, 11, 24, 20, 24, 19, 12, 18, 16, 14, 14,  8, 12, 19, 24, 24,  0,  2,  4, 17, 20, 11, 21, 24},
		{ 9, 22, 23,  5, 14, 17, 16, 20,  1, 18,  5,  5, 15,  2, 18,  5,  6,  8,  6,  6, 20, 11,  4,  2, 12, 13,  8, 24, 13,  0,  6,  5, 15, 12,  3, 13},
		{19,  9,  3, 23, 23, 24, 13,  9, 12, 24, 22, 13,  2,  4,  7,  9, 22, 21,  1,  4, 24, 15, 23, 18,  1,  5,  1, 22,  8, 18,  0, 15, 10, 14, 19,  6},
		{15, 12, 23, 22, 21, 22, 20, 21,  7, 15, 20, 10,  4, 15,  4, 17, 23,  8,  6, 18, 22, 17,  6, 13, 15, 22, 19,  7, 11, 22,  4,  0, 13, 17, 18,  5},
		{17, 10, 17, 18, 14,  4,  7,  9,  1,  1,  8, 22, 11, 19, 12, 19, 16, 11, 11,  4,  5,  4, 21,  2, 22, 15,  6, 11, 11,  2,  9, 12,  0, 24, 16,  1},
		{ 2, 17, 23, 11,  9,  8, 20, 16, 10, 12,  7, 12, 24, 23, 11, 13, 18, 17,  1,  6, 22,  7, 21,  4, 15, 11, 17, 15,  2,  7, 20, 21, 19,  0, 14, 24},
		{ 4, 22, 20, 14,  7,  2, 23,  4,  5, 17,  1, 14, 13, 18,  7, 10,  9, 23, 14, 16,  2, 15,  7,  7,  6, 13, 10, 21, 22,  5,  1, 10, 17, 11,  0, 20},
		{ 2, 12,  7, 14, 20, 22, 13, 18, 23,  2, 23, 15,  7, 10,  5, 16, 16, 10,  1, 21, 10,  1,  3,  9,  3,  8,  4, 23,  6,  1, 20, 24, 20, 22,  8,  0},
	};

	void Start() {
		_bombInfo = GetComponent<KMBombInfo>();
		_bombHelper = GetComponent<BombHelper>();
		_bombModule = GetComponent<KMBombModule>();
		_serialNumber = _bombInfo.GetSerialNumber().ToLower();

		KMSelectable selectable = GetComponent<KMSelectable>();
		_buttons = selectable.Children;

		_serialUinIndex = Random.Range(0, 19);  // Determine where in the uinpl the serial should go
		_serialBuilt = Random.Range(0f, 1f) < 0.5f; // Determine whether we spawn the module with the serial already present

		ModConfig<UinplSettings> config = new ModConfig<UinplSettings>("UinplSettings");
		settings = config.Read();
		if (settings.onTime != onTime || settings.offTime != offTime) {
			_textDisplay.SetColor(new Color(255, 153, 0));
			_bombHelper.Log(string.Format("Accessibility settings changed: On Time {0}, Off Time {1}. Turning the display orange.", settings.onTime, settings.offTime));
		}
		onTime = settings.onTime;
		offTime = settings.offTime;

		_buttons[0].OnInteract += () => {
			// continue button
			_buttons[0].AddInteractionPunch(0.5f);
			_bombHelper.PlayGameSound(KMSoundOverride.SoundEffect.ButtonPress, _buttons[0].transform);
			HaltToggle();
			return false;
		};
		for (int i = 0; i < 24; i++) {
			// display buttons
			KMSelectable subSelectable = _buttons[i + 1];
			_buttonLabels[i] = subSelectable.GetComponentInChildren<TextMesh>();
			TextMesh label = _buttonLabels[i];
			label.gameObject.SetActive(false);

			subSelectable.OnHighlight += () => {
				// give an orange highlight on hover
				if (_halted && !_solved) label.color = new Color(1f, 69f / 255f, 0f);
			};
			subSelectable.OnHighlightEnded += () => {
				// remove orange highlight
				label.color = new Color(102f / 255f, 1f, 0f);
			};
			int i2 = i;
			subSelectable.OnInteract += () => {
				subSelectable.AddInteractionPunch(0.1f);
				// change the color slightly to simulate pressing on a screen and it leaving one of those weird LCD finger marks
				label.color = new Color(102f / 255f, 0.8f, 0f);
				if (_halted && !_solved) CheckAnswer(i2);
				return false;
			};
			GenerateCharacter(i);
		}

		_bombModule.OnActivate += () => {
			_coroutine = StartCoroutine(Flash());
			_textDisplay.SetStage(0);
		};
	}

	void CalculateSolution() {
		string twentyfour = "";
		for (int i = 0; i < 24; i++) {
			twentyfour += _buttonLabels[i].text;
		}
		_bombHelper.Log(string.Format("The current UIN(+L) is {0}", twentyfour));
		_bombHelper.Log(string.Format("Last changed at position {0}: From {1} to {2}.", _changedCharIndex + 1, _oldChar, _newChar));

		bool serialNumberPresent = twentyfour.Contains(_serialNumber);
		int serIndex = twentyfour.IndexOf(_serialNumber);
		_bombHelper.Log(serialNumberPresent ?
			string.Format("Serial number is present on position {0}", serIndex + 1) :
			"Serial number not present."
		);
		
		// convert the chars to an index from the table
		int newCharTableIndex = Conversions.IndexOf(_newChar);
		int oldCharTableIndex = Conversions.IndexOf(_oldChar);

		// get two positions from the table
		int intermediateStepA = Solutions[newCharTableIndex, oldCharTableIndex] - 1;
		int intermediateStepB = Solutions[oldCharTableIndex, newCharTableIndex] - 1;
		if (intermediateStepA == intermediateStepB) {
			// If both numbers acquired from the tables are the same, press the digit (or letter) in that position, 
			// unless the bomb's serial number is present in your UIN(+L), in which case press the most recently changed digit (or letter) instead.
			_solution = serialNumberPresent ? _changedCharIndex : intermediateStepB;
			_bombHelper.Log(string.Format("Table results: {0} and {1}, which are the same.", intermediateStepA + 1, intermediateStepB + 1));
			_bombHelper.Log(string.Format("Solution: position {0} which is a {1}", _solution + 1, twentyfour[_solution]));
			return;
		}

		// get the chars in these positions
		char intermediateCharA = twentyfour[intermediateStepA];
		char intermediateCharB = twentyfour[intermediateStepB];
		_bombHelper.Log(string.Format("Table results: {0} and {1} which are {2} and {3}.", intermediateStepA + 1, intermediateStepB + 1, intermediateCharA, intermediateCharB));

		if (intermediateCharA == intermediateCharB) {
			// If both these digits (or letters) are the same, press the digit (or letter) in the position denoted by the first number, 
			// or the second number if the bomb's serial number is present in your UIN(+L).
			_solution = serialNumberPresent ? intermediateStepB : intermediateStepA;
			_bombHelper.Log(string.Format("These two characters are the same.", intermediateStepA + 1, intermediateStepB + 1));
			_bombHelper.Log(string.Format("Solution: position {0} which is a {1}", _solution + 1, twentyfour[_solution]));
			return;
		}

		// convert these chars back into table index
		int intACharTableIndex = Conversions.IndexOf(intermediateCharA);
		int intBCharTableIndex = Conversions.IndexOf(intermediateCharB);

		// get the final solution from this table
		_solution = serialNumberPresent ? 
			Solutions[intACharTableIndex, intBCharTableIndex] - 1 :
			Solutions[intBCharTableIndex, intACharTableIndex] - 1;
		_bombHelper.Log(string.Format("Solution: position {0} which is a {1}", _solution + 1, twentyfour[_solution]));
	}

	void HaltToggle() {
		if (_oldChar == '-' || _solved) return;
		if (_halted) {
			_bombHelper.Log("Continue pressed again, resuming");
			_bombHelper.Log("--------------------------------");
			// resume
			_textDisplay.SetStage(0);
			ResumeFlashing();
		}
		else {
			_bombHelper.Log("Continue pressed, halting");
			_textDisplay.SetStage(1);
			CalculateSolution();
		}
		_halted = !_halted;
	}

	void ResumeFlashing() {
		for (int i = 0; i < 24; i++) {
			if (Random.Range(0f, 1f) < resumeShuffleRatio) {
				bool changeToLetter = Random.Range(0f, 1f) < letterPercentage;
				int newIndex = changeToLetter ? Random.Range(0, Letters.Length) : Random.Range(0, Numbers.Length);
				char newChar = changeToLetter ? Letters[newIndex] : Numbers[newIndex];
				_buttonLabels[i].text = newChar.ToString();
			}
			// blank the screen upon resuming
			_buttonLabels[i].gameObject.SetActive(false);
		}
	}

	bool ChangeCharacter(int index) {
		char oldChar = _buttonLabels[index].text[0];
		char newChar = oldChar;
		int changedCharIndex = index;

		// Check for serial number
		int serialIndex = index - _serialUinIndex;
		if (serialIndex >= 0 && serialIndex < 6) {
			// If this is in the assigned serial number space
			if (_serialBuilt) {
				// we are currently removing the serial number
				if (oldChar != _serialNumber[serialIndex]) {
					// this current character is already not set. change a different one instead.
					int incorrectSerialDigits = 0;
					for (int j = 0; j < 6; j++) {
						// loop through the serial until we find a character that matches
						if (_buttonLabels[_serialUinIndex + j].text[0] == _serialNumber[j]) {
							oldChar = _buttonLabels[_serialUinIndex + j].text[0];
							newChar = oldChar;
							while (newChar == oldChar) {
								bool changeToLetter = Random.Range(0f, 1f) < letterPercentage;
								int newIndex = changeToLetter ? Random.Range(0, Letters.Length) : Random.Range(0, Numbers.Length);
								newChar = changeToLetter ? Letters[newIndex] : Numbers[newIndex];
							}
							_buttonLabels[_serialUinIndex + j].text = newChar.ToString();
							changedCharIndex = _serialUinIndex + j;
							break;
						}
						else {
							incorrectSerialDigits++;
						}
					}
					if (incorrectSerialDigits == 6) {
						// all the digits are set. serial is entirely gone, mark it as such.
						// choose a new spot for the serial, then let the random change take care of the char to change.
						_serialBuilt = false;
						_serialUinIndex = Random.Range(0, 19);
					}
				}
				// else: the character is set to a serial character. Just leave it to change to random.
			}
			else {
				// we are currently building the serial number
				if (oldChar == _serialNumber[serialIndex]) {
					// this current character is already set. change a different one instead.
					int correctSerialDigits = 0;
					for (int j = 0; j < 6; j++) {
						// loop through the serial until we find a character that doesn't match
						if (_buttonLabels[_serialUinIndex + j].text[0] != _serialNumber[j]) {
							oldChar = _buttonLabels[_serialUinIndex + j].text[0];
							newChar = _serialNumber[j];
							_buttonLabels[_serialUinIndex + j].text = newChar.ToString();
							changedCharIndex = _serialUinIndex + j;
							break;
						}
						else {
							correctSerialDigits++;
						}
					}
					if (correctSerialDigits == 6) {
						// all the digits are set. serial is present, mark it as such.
						_serialBuilt = true;
						// we want to keep the serial number present in the number a bit longer, reroll the character to change.
						return false;
					}
				}
				else {
					// this current character is not set. Set it.
					newChar = _serialNumber[serialIndex];
					_buttonLabels[index].text = newChar.ToString();
				}
			}
		}
		if (newChar != oldChar) {
			// skip the random change if a serial change has already happened.
			_changedCharIndex = changedCharIndex;
			_oldChar = oldChar;
			_newChar = newChar;
			return true;
		}

		// Change to random
		while (newChar == oldChar) {
			bool changeToLetter = Random.Range(0f, 1f) < letterPercentage;
			int newIndex = changeToLetter ? Random.Range(0, Letters.Length) : Random.Range(0, Numbers.Length);
			newChar = changeToLetter ? Letters[newIndex] : Numbers[newIndex];
		}

		_buttonLabels[index].text = newChar.ToString();
		_changedCharIndex = changedCharIndex;
		_oldChar = oldChar;
		_newChar = newChar;
		return true;
	}

	void GenerateCharacter(int index) {
		char newChar;
		if (_serialBuilt && index >= _serialUinIndex && index < _serialUinIndex + 6) {
			// if this character should be a serial number char
			int serialIndex = index - _serialUinIndex;
			newChar = _serialNumber[serialIndex];
		}
		else {
			bool changeToLetter = Random.Range(0f, 1f) < letterPercentage;
			int newIndex = changeToLetter ? Random.Range(0, Letters.Length) : Random.Range(0, Numbers.Length);
			newChar = changeToLetter ? Letters[newIndex] : Numbers[newIndex];
		}
		_buttonLabels[index].text = newChar.ToString();
	}

	void CheckAnswer(int pressed) {
		if (pressed == _solution) {
			_bombHelper.Log(string.Format("Pressed position {0} ('{1}'), that is correct. Module solved!", pressed, _buttonLabels[pressed].text));
			_solved = true;
			_textDisplay.SetStage(3);
			_bombModule.HandlePass();
			StopCoroutine(_coroutine);
			_coroutine = StartCoroutine(FlashSolved());
		}
		else {
			_bombHelper.Log(pressed.ToString());
			_bombHelper.Log(string.Format("STRIKE!: Pressed position {0} ('{1}')!", pressed, _buttonLabels[pressed].text));
			_textDisplay.SetStage(2);
			_bombModule.HandleStrike();
		}
	}

	IEnumerator Flash() {
		while (!_solved) {
			yield return new WaitForSeconds(onTime);
			while (_halted) {
				yield return null;
			}

			bool changed = false;
			while (!changed) {
				int randomInt = Random.Range(0, 24);
				for (int i = 0; i < 24; i++) {
					_buttonLabels[i].gameObject.SetActive(false);

					if (i == randomInt) {
						changed = ChangeCharacter(i);
					}
				}
			}
			yield return new WaitForSeconds(offTime);
			for (int i = 0; i < 24; i++) {
				_buttonLabels[i].gameObject.SetActive(true);
			}
		}
	}

	IEnumerator FlashSolved() {
		for (int i = 0; i < 24; i++) {
			switch (i) {
				case 16:
					_buttonLabels[i].text = ">";
					_buttonLabels[i].gameObject.SetActive(true);
					break;
				case 17:
					_buttonLabels[i].text = "█";
					_buttonLabels[i].fontSize = (int)(_buttonLabels[i].fontSize * 0.8f);
					_buttonLabels[i].gameObject.SetActive(true);
					break;
				default:
					_buttonLabels[i].text = " ";
					_buttonLabels[i].gameObject.SetActive(false);
					break;
			}
		}
		while (_solved) {
			yield return new WaitForSeconds(onTime);
			_buttonLabels[17].gameObject.SetActive(false);
			yield return new WaitForSeconds(offTime);
			_buttonLabels[17].gameObject.SetActive(true);
		}
	}

	public readonly string TwitchHelpMessage = "'!{0} Continue' to press the Continue button. '!{0} Press <1-24>' press button in reading order. '!{0} Streamdelay <seconds>' to make the module compensate for the specified stream delay in seconds (min/default = 0).";

	public IEnumerator ProcessTwitchCommand(string command) {
		command = command.ToUpperInvariant().Trim();
		List<string> split = command.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
		if (split[0] == "CONTINUE" || split[0] == "CONT" || split[0] == "C") {
			_buttons[0].OnInteract();
			yield return null;
		}
		else if (split[0] == "PRESS" || split[0] == "P") {
			if (split.Count == 1) {
				yield break;
			}
			int result;
			if (!int.TryParse(split[1], out result)) {
				yield break;
			}
			if (result < 1 || result > 24) {
				yield break;
			}
			_buttons[result].OnInteract();
			yield return null;
		}
		else if (split[0] == "STREAMDELAY" || split[0] == "DELAY" || split[0] == "SD" || split[0] == "D") {
			if (split.Count == 1) {
				yield break;
			}
			float result;
			if (!float.TryParse(split[1], out result)) {
				yield break;
			}
			if (result < 0) {
				yield break;
			}
			onTime = settings.onTime + result;
			StopCoroutine(_coroutine);
			_coroutine = StartCoroutine(Flash());
			yield return null;
		}
	}

	IEnumerator TwitchHandleForcedSolve() {
		while (!_solved) {
			if (!_halted) {
				_buttons[0].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
			_buttons[_solution + 1].OnInteract();
			yield return null;
		}
	}
}
