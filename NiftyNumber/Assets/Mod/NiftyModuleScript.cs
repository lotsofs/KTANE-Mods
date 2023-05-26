using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;

public class NiftyModuleScript : MonoBehaviour {

	[SerializeField] TextMesh _topDisplay;
	[SerializeField] TextMesh _bottomDisplay;
	[SerializeField] KMSelectable[] _topUp;
	[SerializeField] KMSelectable[] _topDown;
	[SerializeField] KMSelectable[] _bottomUp;
	[SerializeField] KMSelectable[] _bottomDown;
	[SerializeField] KMSelectable _submit;
	[SerializeField] TextMesh _submitPlusLabel;
	[SerializeField] TextMesh _submitMinusLabel;

	const string Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	const int Biexian = 1679616;
	const int NifUnexian = 46656;
	const int Unexian = 1296;
	const int Nif = 36;
	const int One = 1;

	bool _add;
	string _solutionNiftimal;
	int _solutionDecimal;
	bool _solved = false;
	bool _activated = false;

	BombHelper _bombHelper;
	KMBombInfo _bombInfo;
	KMBombModule _bombModule;

	// Use this for initialization
	void Start () {
		_bombHelper = GetComponent<BombHelper>();
		_bombInfo = GetComponent<KMBombInfo>();
		_bombModule = GetComponent<KMBombModule>();

		_bombModule.OnActivate += () => {
			_topDisplay.gameObject.SetActive(true);
			_bottomDisplay.gameObject.SetActive(true);
			_activated = true;
		};

		for (int i = 0; i < _topUp.Length; i++) {
			KMSelectable butT = _topUp[i];
			int j = i;
			butT.OnInteract += () => {
				butT.AddInteractionPunch(0.1f);
				_bombHelper.PlayGameSound(KMSoundOverride.SoundEffect.ButtonPress, butT.transform);
				UpdateDisplay(true, j, true);
				return false;
			};
			KMSelectable butB = _topDown[i];
			butB.OnInteract += () => {
				butB.AddInteractionPunch(0.1f);
				_bombHelper.PlayGameSound(KMSoundOverride.SoundEffect.ButtonPress, butB.transform);
				UpdateDisplay(true, j, false);
				return false;
			};
		}
		for (int i = 0; i < _bottomUp.Length; i++) {
			KMSelectable butT = _bottomUp[i];
			int j = i;
			butT.OnInteract += () => {
				butT.AddInteractionPunch(0.1f);
				_bombHelper.PlayGameSound(KMSoundOverride.SoundEffect.ButtonPress, butT.transform);
				UpdateDisplay(false, j, true);
				return false;
			};
			KMSelectable butB = _bottomDown[i];
			butB.OnInteract += () => {
				butB.AddInteractionPunch(0.1f);
				_bombHelper.PlayGameSound(KMSoundOverride.SoundEffect.ButtonPress, butB.transform);
				UpdateDisplay(false, j, false);
				return false;
			};
		}
		_submit.OnInteract += () => {
			_submit.AddInteractionPunch(1f);
			_bombHelper.PlayGameSound(KMSoundOverride.SoundEffect.ButtonPress, _submit.transform);
			Submit();
			return false;
		};

		if (UnityEngine.Random.Range(0f,1f)>0.5f) {
			_add = true;
			_submitPlusLabel.gameObject.SetActive(true);
		}
		else {
			_add = false;
			_submitMinusLabel.gameObject.SetActive(true);
		}

		CalculateSolution();
	}

	void Submit() {
		if (_solved || !_activated) return;
		_bombHelper.Log("Submitting");

		string top = _topDisplay.text;
		int val = 0;
		val += Chars.IndexOf(top[0]) * NifUnexian;
		val += Chars.IndexOf(top[1]) * Unexian;
		val += Chars.IndexOf(top[2]) * Nif;
		val += Chars.IndexOf(top[3]);
		_bombHelper.Log(string.Format("Top Display = {0} (niftimal) = {1} (decimal)", _topDisplay.text, val));

		int bottom = int.Parse(_bottomDisplay.text);
		int pos1 = bottom;
		int pos4 = pos1 / NifUnexian;
		pos1 %= NifUnexian;
		int pos3 = pos1 / Unexian;
		pos1 %= Unexian;
		int pos2 = pos1 / Nif;
		pos1 %= Nif;
		string bottomNiftimal = string.Format("{0}{1}{2}{3}", Chars[pos4], Chars[pos3], Chars[pos2], Chars[pos1]);
		_bombHelper.Log(string.Format("Bottom Display = {0} (niftimal) = {1} (decimal)", bottomNiftimal, _bottomDisplay.text));

		if (_topDisplay.text == _solutionNiftimal && _bottomDisplay.text == _solutionDecimal.ToString("00000")) {
			_bombHelper.Log("Solved");
			_solved = true;
			_bombModule.HandlePass();
			StartCoroutine(CountDown());
		}
		else {
			_bombHelper.Log("Strike");
			_bombModule.HandleStrike();
		}
	}

	void UpdateDisplay(bool top, int pos, bool up) {
		if (!_activated) return;
		int max = top ? 36 : 10;
		char[] chars = top ? _topDisplay.text.ToCharArray() : _bottomDisplay.text.ToCharArray();
		int numPos = chars.Length - 1 - pos;
		int index = Chars.IndexOf(chars[numPos]);
		index += up ? 1 : -1;
		if (index < 0) index += max;
		index %= max;
		chars[numPos] = Chars[index];
		if (top) {
			_topDisplay.text = new string(chars);
		}
		else {
			_bottomDisplay.text = new string(chars);
		}
	}

	void CalculateSolution() {
		string serial = _bombInfo.GetSerialNumber();
		string ser1 = serial.Substring(0, 3);
		string ser2 = serial.Substring(3, 3);
		int val1 = 0;
		int val2 = 0;
		val1 += Chars.IndexOf(ser1[0]) * Unexian;
		val1 += Chars.IndexOf(ser1[1]) * Nif;
		val1 += Chars.IndexOf(ser1[2]);
		val2 += Chars.IndexOf(ser2[0]) * Unexian;
		val2 += Chars.IndexOf(ser2[1]) * Nif;
		val2 += Chars.IndexOf(ser2[2]);
		_bombHelper.Log(string.Format("Value 1 = {0} (niftimal) = {1} (decimal)", ser1, val1));
		_bombHelper.Log(string.Format("Value 2 = {0} (niftimal) = {1} (decimal)", ser2, val2));
		_bombHelper.Log(_add ? "Add" : "Subtract");
		if (_add) {
			_solutionDecimal = val1 + val2;
		}
		else {
			_solutionDecimal = Mathf.Abs(val1 - val2);
		}
		int pos1 = _solutionDecimal;
		int pos4 = pos1 / NifUnexian;
		pos1 %= NifUnexian;

		int pos3 = pos1 / Unexian;
		pos1 %= Unexian;

		int pos2 = pos1 / Nif;
		pos1 %= Nif;

		//int Pos1 = _solutionDecimal / One;
		_solutionNiftimal = string.Format("{0}{1}{2}{3}", Chars[pos4], Chars[pos3], Chars[pos2], Chars[pos1]);
		_bombHelper.Log(string.Format("Solution = {0} (niftimal) = {1} (decimal)", _solutionNiftimal, _solutionDecimal));
	}

	IEnumerator CountDown() {
		_bottomDisplay.text = (System.DateTime.Now.TotalSeconds() - System.DateTime.Today.TotalSeconds()).ToString("00000");
		int oldTime = 0;
		while (true) {
			yield return null;
			int time = (int)_bombInfo.GetTime() % Biexian;
			if (time == oldTime) {
				continue;
			}
			oldTime = time;
			int pos1 = time;
			int pos4 = pos1 / NifUnexian;
			pos1 %= NifUnexian;
			int pos3 = pos1 / Unexian;
			pos1 %= Unexian;
			int pos2 = pos1 / Nif;
			pos1 %= Nif;
			_topDisplay.text = string.Format("{0}{1}{2}{3}", Chars[pos4], Chars[pos3], Chars[pos2], Chars[pos1]);
		}
	}

	public readonly string TwitchHelpMessage = "'!{0} 0ABC 01234' to submit top display 0ABC and bottom display 01234.";

	public IEnumerator ProcessTwitchCommand(string command) {
		command = command.ToUpperInvariant().Trim();
		string[] splits = command.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
		if (splits.Length != 2) {
			yield break;
		}
		int dump;
		if (splits[0].Length != 4 || splits[1].Length != 5) {
			yield break;
		}
		foreach (char c in splits[0]) {
			if (!Chars.Contains(c.ToString())) {
				yield break;
			}
		}
		foreach(char c in splits[1]) {
			if (!char.IsDigit(c)) {
				yield break;
			}
		}
		yield return null;
		for (int i = 0; i < 4; i++) {
			while (_topDisplay.text[i] != splits[0][i]) {
				if (Chars.IndexOf(splits[0][i]) >= 18) {
					_topDown[3 - i].OnInteract();
				}
				else {
					_topUp[3 - i].OnInteract();
				}
				yield return new WaitForSeconds(0.1f);
			}
		}
		yield return new WaitForSeconds(0.1f);
		for (int i = 0; i < 5; i++) {
			while (_bottomDisplay.text[i] != splits[1][i]) {
				if (Chars.IndexOf(splits[1][i]) >= 5) {
					_bottomDown[4 - i].OnInteract();
				}
				else {
					_bottomUp[4 - i].OnInteract();
				}
				yield return new WaitForSeconds(0.1f);
			}
		}
		yield return new WaitForSeconds(0.1f);
		_submit.OnInteract();
	}

	IEnumerator TwitchHandleForcedSolve() {
		for (int i = 0; i < 4; i++) {
			while (_topDisplay.text[i] != _solutionNiftimal[i]) {
				if (Chars.IndexOf(_solutionNiftimal[i]) >= 18) {
					_topDown[3 - i].OnInteract();
				}
				else {
					_topUp[3 - i].OnInteract();
				}
				yield return new WaitForSeconds(0.1f);
			}
		}
		yield return new WaitForSeconds(0.1f);
		for (int i = 0; i < 5; i++) {
			while (_bottomDisplay.text[i] != _solutionDecimal.ToString("00000")[i]) {
				if (Chars.IndexOf(_solutionDecimal.ToString("00000")[i]) >= 5) {
					_bottomDown[4 - i].OnInteract();
				}
				else {
					_bottomUp[4 - i].OnInteract();
				}
				yield return new WaitForSeconds(0.1f);
			}
		}
		yield return new WaitForSeconds(0.1f);
		_submit.OnInteract();
	}
}
