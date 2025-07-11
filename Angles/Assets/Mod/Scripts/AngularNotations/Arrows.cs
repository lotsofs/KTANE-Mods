﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using raminrahimzada;

public class Arrows : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	public override bool Submit(decimal current, bool log = true) {
		decimal margin = (0.000001M) * DecimalMath.Pi;
		DecimalVector2 center = new DecimalVector2(DecimalMath.Cos(0), DecimalMath.Sin(0));
		DecimalVector2 edge = new DecimalVector2(DecimalMath.Cos(margin), DecimalMath.Sin(margin));
		decimal maxDistance = DecimalVector2.Distance(center, edge);
		DecimalVector2 submitted = new DecimalVector2(DecimalMath.Cos(current), DecimalMath.Sin(current));
		decimal submittedDistance = DecimalVector2.Distance(Position, submitted);
		
		string answer = string.Format("{0:0.#######} radians", current);
		if (log) {
			Bomb.LogFormat("Submitted '{0}'. Correct answer: '{1}'", answer, Name);
			Bomb.LogFormat("Submitted coordinate ({0:0.#######}, {1:0.#######}) to solution coordinate ({2:0.#######}, {3:0.#######}) yields distance {4:0.#######} out of max allowed {5:0.#######}",
				submitted.X, submitted.Y, Position.X, Position.Y, submittedDistance, maxDistance
			);
		}
		return submittedDistance <= maxDistance;
	}

	public override decimal LargeJump(bool positive, decimal current) {
		decimal jump = (1.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal MediumJump(bool positive, decimal current) {
		decimal jump = (1.0M / 2.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal SmallJump(bool positive, decimal current) {
		decimal jump = (1.0M / 4.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal LargeReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((1.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal MediumReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((1.0M / 2.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal SmallReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((1.0M / 4.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}


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
