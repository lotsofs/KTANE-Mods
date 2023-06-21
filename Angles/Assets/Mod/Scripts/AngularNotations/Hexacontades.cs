using raminrahimzada;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexacontades : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	private readonly float DECIMAL_ODDS = 0.5f;
	private readonly float NEGATIVE_ODDS = 0.2f;
	private readonly float DOUBLE_ODDS = 0.2f;

	public Hexacontades(BombHelper b) : base(b) {
		decimal number;
		if (Random.Range(0f, 1f) < DECIMAL_ODDS) {
			int num = Random.Range(0, 6000);
			number = (decimal)num / 100.0M;
		}
		else {
			int num = Random.Range(0, 60);
			number = num;
		}
		if (Random.Range(0f, 1f) < DOUBLE_ODDS) {
			number += 60.0M;
		}
		if (Random.Range(0f, 1f) < NEGATIVE_ODDS) {
			number *= -1.0M;
		}
		decimal rads = number * DecimalMath.Pi / 30;
		Name = string.Format("{0:0.#}h", number);
		Position = new DecimalVector2(DecimalMath.Cos(rads), DecimalMath.Sin(rads));
	}
}