using raminrahimzada;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CartesianCoordinates : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	public CartesianCoordinates(BombHelper b) : base(b) {
		int x = 0;
		int y = 0;
		while (x == 0 && y == 0) {
			x = Random.Range(-9, 10);
			y = Random.Range(-9, 10);
		}

		decimal opposite = DecimalMath.Abs(y);
		decimal adjacent = DecimalMath.Abs(x);
		decimal hypotenuse = DecimalMath.Sqrt(opposite * opposite + adjacent + adjacent);

		Position = new DecimalVector2(opposite / hypotenuse, adjacent / hypotenuse);
		Name = string.Format("({0},{1})", x, y);
	}
}
