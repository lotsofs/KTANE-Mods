using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;

public class uinplModule : MonoBehaviour {

	static string Letters = "abcdefghijklmnopqrstuvwxyz";
	static string Numbers = "0123456789";

	public float onTime = 1f;
	public float offTime = 0.3f;
	public float letterPercentage = 0.3f;

	TextMesh[] _buttonLabels = new TextMesh[24];
	Coroutine _coroutine;
	string _serialNumber;

	private bool _halted = false;
	private bool _solved = false;
	private int _serialUinIndex;
	private bool _serialBuilt = false;

	private char _oldChar;
	private char _newChar;
	private int _changedCharIndex;

	KMBombInfo _bombInfo;

	void Start () {
		_bombInfo = GetComponent<KMBombInfo>();
		_serialNumber = _bombInfo.GetSerialNumber().ToLower();

		KMSelectable selectable = GetComponent<KMSelectable>();

		_serialUinIndex = Random.Range(0, 19);
		_serialBuilt = Random.Range(0f, 1f) < 0.5f;

		for (int i = 0; i < 24; i++) {
			_buttonLabels[i] = selectable.Children[i + 1].GetComponentInChildren<TextMesh>();
			char newChar;
			if (_serialBuilt && i >= _serialUinIndex && i < _serialUinIndex + 6) {
				int serialIndex = i - _serialUinIndex;
				newChar = _serialNumber[serialIndex];
			}
			else {
				bool changeToLetter = Random.Range(0f, 1f) < letterPercentage;
				int newIndex = changeToLetter ? Random.Range(0, Letters.Length) : Random.Range(0, Numbers.Length);
				newChar = changeToLetter ? Letters[newIndex] : Numbers[newIndex];
			}
			_buttonLabels[i].text = newChar.ToString();

		}

		_coroutine = StartCoroutine(Flash());
		
		//StopCoroutine(_coroutine);
		//_coroutine = StartCoroutine(FlashSolved());
	}
	
	void Update () {
		// todo: upon pressing the continue button when it's already halted, change some numbers
	}

	IEnumerator Flash() {
		while (!_solved) {
			yield return new WaitForSeconds(onTime);
			if (_halted) {
				continue;
			}
			
			int randomInt = Random.Range(0, 24);
			for (int i = 0; i < 24; i++) {
				_buttonLabels[i].gameObject.SetActive(false);

				if (i == randomInt) {
					char oldChar = _buttonLabels[i].text[0];
					char newChar = oldChar;
					int changedCharIndex = i;
					
					// Check for serial number
					int serialIndex = i - _serialUinIndex;
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
									// do not do anything else and let the random change take care of it.
									_serialBuilt = true;
								}
							}
							else {
								// this current character is not set. Set it.
								newChar = _serialNumber[serialIndex];
								_buttonLabels[i].text = newChar.ToString();
							}
						}
					}
					if (newChar != oldChar) {
						// skip the random change if a serial change has already happened.
						_changedCharIndex = changedCharIndex;
						_oldChar = oldChar;
						_newChar = newChar;
						continue;
					}

					// Change to random
					while (newChar == oldChar) {
						bool changeToLetter = Random.Range(0f, 1f) < letterPercentage;
						int newIndex = changeToLetter ? Random.Range(0, Letters.Length) : Random.Range(0, Numbers.Length);
						newChar = changeToLetter ? Letters[newIndex] : Numbers[newIndex];
					}

					_buttonLabels[i].text = newChar.ToString();
					_changedCharIndex = changedCharIndex;
					_oldChar = oldChar;
					_newChar = newChar;
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
}
