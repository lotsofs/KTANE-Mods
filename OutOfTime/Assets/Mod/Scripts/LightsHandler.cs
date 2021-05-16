using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightsHandler : MonoBehaviour {

	[SerializeField] GameObject _lightContainer;
	[SerializeField] Light[] _lightSources;
	[SerializeField] TextMesh _display;
	[Space]
	[SerializeField] GameObject _redBackdrop;
	[SerializeField] GameObject _yellowBackdrop;
	[SerializeField] GameObject _greenBackdrop;
	[SerializeField] GameObject _cyanBackdrop;
	[SerializeField] GameObject _blueBackdrop;
	[SerializeField] GameObject _magentaBackdrop;
	[SerializeField] GameObject _whiteBackdrop;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
