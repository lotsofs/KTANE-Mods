using raminrahimzada;
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
		_buttonDisplayLeft.OnInteract += () => { return false; } ;
		_buttonDisplayRight.OnInteract += () => { return false; } ;
		_buttonNeedleLeftUp.OnInteract += () => { return false; } ;
		_buttonNeedleLeftDown.OnInteract += () => { return false; } ;
		_buttonNeedleMiddleUp.OnInteract += () => { return false; } ;
		_buttonNeedleMiddleDown.OnInteract += () => { return false; } ;
		_buttonNeedleRightUp.OnInteract += () => { return false; } ;
		_buttonNeedleRightDown.OnInteract += () => { return false; };

		//Debug.Log(DecimalMath.Cos(DecimalMath.Pi * 2 / 3));

		for (int i = 0; i < 100; i++) {
			AngularNotation dir = new BAM(_b);
			Debug.LogFormat("{2} = ({0:0.##########}, {1:0.##########})", dir.Position.X, dir.Position.Y, dir.Name);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
