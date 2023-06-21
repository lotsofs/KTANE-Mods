using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using raminrahimzada;

public class ZodiacSymbols : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	public ZodiacSymbols(BombHelper b) : base(b) {
		int rand = UnityEngine.Random.Range(0, 13);
		decimal distance = DecimalMath.Pi * 0.5M;
		switch (rand) {
			case 0: this.Name = "♈"; distance -= 0 * DecimalMath.Pi / 182.5M; break;
			case 1: this.Name = "♉"; distance -= 25 * DecimalMath.Pi / 182.5M; break;
			case 2: this.Name = "♊"; distance -= 62 * DecimalMath.Pi / 182.5M; break;
			case 3: this.Name = "♋"; distance -= 93 * DecimalMath.Pi / 182.5M; break;
			case 4: this.Name = "♌"; distance -= 113 * DecimalMath.Pi / 182.5M; break;
			case 5: this.Name = "♍"; distance -= 150 * DecimalMath.Pi / 182.5M; break;
			case 6: this.Name = "♎"; distance -= 195 * DecimalMath.Pi / 182.5M; break;
			case 7: this.Name = "♏"; distance -= 218 * DecimalMath.Pi / 182.5M; break;
			case 8: this.Name = "⛎"; distance -= 225 * DecimalMath.Pi / 182.5M; break;
			case 9: this.Name = "♐"; distance -= 243 * DecimalMath.Pi / 182.5M; break;
			case 10: this.Name = "♑"; distance -= 275 * DecimalMath.Pi / 182.5M; break;
			case 11: this.Name = "♒"; distance -= 303 * DecimalMath.Pi / 182.5M; break;
			case 12: this.Name = "♓"; distance -= 328 * DecimalMath.Pi / 182.5M; break;
		}
		this.Position = new DecimalVector2(DecimalMath.Cos(distance), DecimalMath.Sin(distance));
	}
}
