using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using raminrahimzada;

public class Arrows : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	public Arrows(BombHelper b) : base(b) {
		int rand = UnityEngine.Random.Range(0, 8);
		switch (rand) {
			case 0: this.Position = DecimalVector2.Right; this.Name = "→"; break;
			case 1: this.Position = DecimalVector2.Up; this.Name = "↑"; break;
			case 2: this.Position = DecimalVector2.Left; this.Name = "←"; break;
			case 3: this.Position = DecimalVector2.Down; this.Name = "↓"; break;
			case 4: this.Position = new DecimalVector2(DecimalMath.Cos(DecimalMath.Pi / 4.0M), DecimalMath.Sin(DecimalMath.Pi / 4.0M)); this.Name = "↗"; break;
			case 5: this.Position = new DecimalVector2(DecimalMath.Cos(DecimalMath.Pi / 4.0M * 3.0M), DecimalMath.Sin(DecimalMath.Pi / 4.0M * 3.0M)); this.Name = "↖"; break;
			case 6: this.Position = new DecimalVector2(DecimalMath.Cos(-DecimalMath.Pi / 4.0M), DecimalMath.Sin(-DecimalMath.Pi / 4.0M)); this.Name = "↘"; break;
			case 7: this.Position = new DecimalVector2(DecimalMath.Cos(-DecimalMath.Pi / 4.0M * 3.0M), DecimalMath.Sin(-DecimalMath.Pi / 4.0M * 3.0M)); this.Name = "↙"; break;
		}
	}
}
