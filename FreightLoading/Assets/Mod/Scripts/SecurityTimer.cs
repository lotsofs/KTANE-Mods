using System;
using UnityEngine;

public class SecurityTimer : MonoBehaviour {
	[SerializeField] TextMesh _text;
	[SerializeField] float _blipOnTime;
	[SerializeField] float _blipOffTime;
	float _blipTimer = 0;
	string _car = "Trains";

	/// <summary>
	/// Sets the currently displayed car on the security footage
	/// </summary>
	/// <param name="stage"></param>
	public void NewCar(int stage) {
		if (stage == 16) {
			_car = "Outbound";
		}
		else {
			_car = string.Format("Car {0}", stage);
		}
	}

	/// <summary>
	/// Changes the security cam text
	/// </summary>
	public void SetText() {
		// flickering recording dot
		_blipTimer += Time.deltaTime;
		if (_blipTimer > _blipOnTime + _blipOffTime) {
			_blipTimer = 0;
		}
		// ● 00:00:00.00 - Car 1
		//   01:23:45.67 - Outbound
		_text.text = string.Format("{0} {1} - {2}", _blipTimer < _blipOnTime ? "●" : " ", DateTime.Now.ToString("HH:mm:ss.ff"), _car);
	}

	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update() {
		SetText();
	}
}
