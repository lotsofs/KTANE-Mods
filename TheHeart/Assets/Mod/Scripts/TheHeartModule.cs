using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheHeartModule : MonoBehaviour {

	[SerializeField] KMBombInfo _bombInfo;
	[SerializeField] KMBombModule _bombModule;
	[SerializeField] BombHelper _bombHelper;
	[SerializeField] KMAudio _bombAudio;

	[Space]

	[SerializeField] HeartBeat _heartBeat;
	[SerializeField] KMSelectable _theHeartSelectable;

	float _activationTime;

	// Use this for initialization
	void Start () {
		_activationTime = (int)_bombInfo.GetTime();
		StartModule();
		_bombModule.OnActivate += ActivateModule;
	}

	void ActivateModule() {
		_activationTime = (int)_bombInfo.GetTime();
		_heartBeat.Beating = true;
	}

	void StartModule() {
		_theHeartSelectable.OnInteract += delegate { Defibrilate(); return false; };
	}
	
	void Defibrilate() {
		_bombHelper.GenericButtonPress(_theHeartSelectable, false, 55);
	}

	// Update is called once per frame
	void Update () {
		if ( Mathf.Abs((int)_bombInfo.GetTime() - _activationTime) >= 60) {
			Debug.Log("HEEEY");
		}
	}
}
