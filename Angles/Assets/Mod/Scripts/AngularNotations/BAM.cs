using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using raminrahimzada;
using System;

public class BAM : AngularNotation {
	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	public BAM(BombHelper b) : base(b) {
		decimal number;
		int num = UnityEngine.Random.Range(0, 256);
		number = num;
		decimal rads = number * DecimalMath.Pi / 128;
		string binary = Convert.ToString(num, 2).PadLeft(8, '0');
		Name = string.Format("0b{0:B}p", binary);
		Position = new DecimalVector2(DecimalMath.Cos(rads), DecimalMath.Sin(rads));
	}

}
