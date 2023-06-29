using raminrahimzada;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeNotation : AngularNotation {
	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	public override bool Submit(decimal current) {
		decimal margin = (119.0M / 43200.0M) * DecimalMath.Pi;
		DecimalVector2 center = new DecimalVector2(DecimalMath.Cos(0), DecimalMath.Sin(0));
		DecimalVector2 edge = new DecimalVector2(DecimalMath.Cos(margin), DecimalMath.Sin(margin));
		decimal maxDistance = DecimalVector2.Distance(center, edge);
		DecimalVector2 submitted = new DecimalVector2(DecimalMath.Cos(current), DecimalMath.Sin(current));
		decimal submittedDistance = DecimalVector2.Distance(Position, submitted);

		decimal degrees = current / DecimalMath.Pi * 43200.0M;
		string answer = string.Format("{0:0.#######} degrees", degrees);
		Bomb.LogFormat("Submitted '{0}'. Correct answer: '{1}'", answer, Name);
		Bomb.LogFormat("Submitted coordinate ({0:0.#######}, {1:0.#######}) to solution coordinate ({2:0.#######}, {3:0.#######}) yields distance {4:0.#######} out of max allowed {5:0.#######}",
			submitted.X, submitted.Y, Position.X, Position.Y, submittedDistance, maxDistance
		);
		return submittedDistance <= maxDistance;
	}

	public override decimal LargeJump(bool positive, decimal current) {
		decimal jump = (1.0M / 6.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal MediumJump(bool positive, decimal current) {
		decimal jump = (10.0M / 6.0M / 60.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal SmallJump(bool positive, decimal current) {
		decimal jump = (1.0M / 6.0M / 60.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal LargeReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = ((1.0M / 6) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal MediumReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = ((10.0M / 6.0M / 60.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal SmallReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = ((1.0M / 6.0M / 60.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}


	public TimeNotation(BombHelper b) : base(b) {
		int hour24 = Random.Range(0, 24);
		int hour12 = hour24 % 12;
 		int minute = Random.Range(0, 60);
		int total = hour12 * 60 + minute;

		decimal rads = total * DecimalMath.Pi / 360;
		Name = string.Format("{0}:{1:00}", hour24, minute);

		decimal distance = (DecimalMath.Pi * 0.5M) - rads;
		Position = new DecimalVector2(DecimalMath.Cos(distance), DecimalMath.Sin(distance));
	}
}
