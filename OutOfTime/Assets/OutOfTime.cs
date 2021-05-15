using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfTime : MonoBehaviour {

    KMBombInfo _bombInfo;
	KMBombModule _bombModule;
	KMSelectable _bombSelectable;
	BombHelper _bombHelper;

	[SerializeField] KMSelectable[] _keypadButtons;
	[SerializeField] KMSelectable _displayButton;

	// Use this for initialization
	void Awake () {
		_bombHelper = GetComponent<BombHelper>();
		_bombInfo = GetComponent<KMBombInfo>();
		_bombModule = GetComponent<KMBombModule>();
		_bombSelectable = GetComponent<KMSelectable>();
	}

	void Start() {
		foreach (KMSelectable button in _keypadButtons) {
			button.OnInteract += () => {
				_bombHelper.GenericButtonPress(button, true, 0.05f);
				button.GetComponent<MovableObject>().SetPosition(1);
				return false;
			};
			button.OnInteractEnded += () => {
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
