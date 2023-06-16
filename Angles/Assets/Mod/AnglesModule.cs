using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnglesModule : MonoBehaviour {

	BombHelper _b;

	[SerializeField] KMSelectable _buttonDisplayLeft;
	[SerializeField] KMSelectable _buttonDisplayRight;
	[SerializeField] KMSelectable _buttonNeedleLeftUp;
	[SerializeField] KMSelectable _buttonNeedleLeftDown;
	[SerializeField] KMSelectable _buttonNeedleMiddleUp;
	[SerializeField] KMSelectable _buttonNeedleMiddleDown;
	[SerializeField] KMSelectable _buttonNeedleRightUp;
	[SerializeField] KMSelectable _buttonNeedleRightDown;


	// Use this for initialization
	void Start () {
		_b = GetComponent<BombHelper>();
		
		_b.AddGenericButtonPresses(new List<float> { 0.2f, 0.2f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f });
		_buttonDisplayLeft.OnInteract += () => { return true; } ;
		_buttonDisplayRight.OnInteract += () => { return true; } ;
		_buttonNeedleLeftUp.OnInteract += () => { return true; } ;
		_buttonNeedleLeftDown.OnInteract += () => { return true; } ;
		_buttonNeedleMiddleUp.OnInteract += () => { return true; } ;
		_buttonNeedleMiddleDown.OnInteract += () => { return true; } ;
		_buttonNeedleRightUp.OnInteract += () => { return true; } ;
		_buttonNeedleRightDown.OnInteract += () => { return true; };
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
