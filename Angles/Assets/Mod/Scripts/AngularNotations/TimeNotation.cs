using raminrahimzada;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeNotation : AngularNotation {
	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	public TimeNotation(BombHelper b) : base(b) {
		int hour24 = Random.Range(0, 24);
		int hour12 = hour24 % 12;
 		int minute = Random.Range(0, 60);
		int total = hour12 * 60 + minute;

		decimal rads = total * DecimalMath.Pi / 360;
		Name = string.Format("({0}:{1:00})", hour24, minute);

		decimal distance = (DecimalMath.Pi * 0.5M) - rads;
		Position = new DecimalVector2(DecimalMath.Cos(rads), DecimalMath.Sin(rads));
	}
}
