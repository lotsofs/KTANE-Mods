using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screen : MonoBehaviour {

	[SerializeField] TextMesh _screenText;
	bool _showSmiley;

	public void UpdateCounter(int score) {
		// :)000
		if (_showSmiley) {
			string smiley = ":)";
			if (score > 999) {
				int s = score % 100;
				_screenText.text = smiley + "…" + s.ToString("00");
			}
			else {
				_screenText.text = smiley + score.ToString("000");
			}
			return;
		}
		// C0000
		if (BombHelper.ColorBlindModeActive) {
			// TODO: Figure out what color is being shown
			char color = 'K';
			if (score > 9999) {
				int s = score % 1000;
				_screenText.text = color + "…" + s.ToString("000");
			}
			else {
				_screenText.text = color + score.ToString("0000");
			}
			return;
		}
		// 00000
		else {
			if (score > 99999) {
				int s = score % 10000;
				_screenText.text = "…" + s.ToString("0000");
			}
			else {
				_screenText.text = score.ToString("00000");
			}
		}
	}

	public void ShowPresses(string presses) {
		// AAAAA
		_screenText.text = presses.Substring(presses.Length - 5);
	}
}
