using raminrahimzada;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AkhnamZam : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }


	public AkhnamZam(BombHelper b) : base(b) {
		int num = Random.Range(0, 225);
		int akhnam = num / 7;
		int zam = num % 7;
		if (zam == 0) {
			Name = string.Format("{0}a", akhnam);
		}
		else {
			Name = string.Format("{0}a{1}z", akhnam, zam);
		}
		decimal rads = num * DecimalMath.Pi / 112;
		Position = new DecimalVector2(DecimalMath.Cos(rads), DecimalMath.Sin(rads));
	}
}
