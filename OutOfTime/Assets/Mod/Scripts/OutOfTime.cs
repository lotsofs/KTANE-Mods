using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfTime : MonoBehaviour {

    KMBombInfo _bombInfo;
	KMBombModule _bombModule;
	KMSelectable _bombSelectable;
	BombHelper _bombHelper;

	[SerializeField] Button[] _keypadButtons;
	[SerializeField] KMSelectable _displayButton;

	[SerializeField] TextMesh _screenText;

	int _currentIndex;
	
	string _buttonLabels = "QWERTYUIO";

	// Use this for initialization
	void Awake () {
		_bombHelper = GetComponent<BombHelper>();
		_bombInfo = GetComponent<KMBombInfo>();
		_bombModule = GetComponent<KMBombModule>();
		_bombSelectable = GetComponent<KMSelectable>();
	}

	void Start() {
		ConfigureSelectables();
		ChooseStartingPoint();
	}
	
	void ChooseStartingPoint() {
		_currentIndex = UnityEngine.Random.Range(0, GridSequence.Sequence.Length);
		BombHelper.Log(string.Format("Starting at index {0}", _currentIndex));

		string initialSequence = "";
		for (int i = _currentIndex - 5; i < _currentIndex; i++) {
			initialSequence += GridSequence.Sequence[i + 1000 % 1000];	// add 1000 first because modulo with negative numbers is poopy
		}
		_screenText.text = initialSequence;
		BombHelper.Log(string.Format("Starting Sequence: {0}.", initialSequence));
		BombHelper.Log(string.Format("Expecting {0} for the first button press.", GridSequence.Sequence[_currentIndex]));

		for (int i = 0; i < _keypadButtons.Length; i++) {
			_keypadButtons[i].TextMesh.text = _buttonLabels[i].ToString();
		}

	}

	void ConfigureSelectables() {
		foreach (Button button in _keypadButtons) {
			KMSelectable sel = button.Selectable;
			sel.OnInteract += () => {
				_bombHelper.GenericButtonPress(sel, true, 0.05f);
				button.GetComponent<MovableObject>().SetPosition(1);
				return false;
			};
			sel.OnInteractEnded += () => {
				button.GetComponent<MovableObject>().SetPosition(0);
			};
		}
		_displayButton.OnInteract += () => {
			_bombHelper.GenericButtonPress(_displayButton, false, 0.1f);
			return false;
		};
		_displayButton.OnInteractEnded += () => {
			_bombHelper.GenericButtonPress(_displayButton, false, 0.1f);
		};
	}

	// Update is called once per frame
	void Update () {
		
	}
}
