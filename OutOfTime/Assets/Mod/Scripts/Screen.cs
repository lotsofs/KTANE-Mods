using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screen : MonoBehaviour {

	[SerializeField] TextMesh _screenText;
	float _smileyDuration = 0.5f;
	float _showSmiley = 0;
	int _score;

	public void UpdateCounter() {
		UpdateCounter(_score);
	}

	public void UpdateCounter(int score) {
		_score = score;
		// :)000
		if (_showSmiley > 0) {
			if (score > 999) {
				int s = score % 100;
				_screenText.text = ":)…" + s.ToString("00");
			}
			else {
				_screenText.text = ":)" + score.ToString("000");
			}
			return;
		}
		// C0000
		char colChar = 'K';
		if (BombHelper.ColorBlindModeActive) {
			// Because there apparently is no way to just dump a color to an int for some reason and Im fed up with trying to get this to work in a more elegant way
			Color col = _screenText.color;
			if (col.r == 1) {
				if (col.g == 1) {
					if (col.b == 1) {
						colChar = 'W';
					}
					else {
						colChar = 'Y';
					}
				}
				else if (col.b == 1) {
					colChar = 'M';
				}
				else {
					colChar = 'R';
				}
			}
			else if (col.g == 1) {
				if (col.b == 1) {
					colChar = 'C';
				}
				else {
					colChar = 'G';
				}
			}
			else if (col.b == 1) {
				colChar = 'B';
			}
		}
		if (colChar != 'K') {
			if (score > 9999) {
				int s = score % 1000;
				_screenText.text = colChar + "…" + s.ToString("000");
			}
			else {
				_screenText.text = colChar + score.ToString("0000");
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

	public void ShowSmiley() {
		_showSmiley = _smileyDuration;
		UpdateCounter();
	}

	public void ShowPresses(string presses) {
		// AAAAA
		_screenText.text = presses.Substring(presses.Length - 5);
	}

	void Update() {
		if (_showSmiley > 0) {
			_showSmiley -= Time.deltaTime;
			if (_showSmiley <= 0) {
				UpdateCounter();
			}
		}
	}
}
