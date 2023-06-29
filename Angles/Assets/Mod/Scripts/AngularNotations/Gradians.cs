using raminrahimzada;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gradians : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	private readonly float DECIMAL_ODDS = 0.2f;
	private readonly float NEGATIVE_ODDS = 0.2f;
	private readonly float DOUBLE_ODDS = 0.2f;

	public override bool Submit(decimal current) {
		decimal margin = (0.01M / 200.0M) * DecimalMath.Pi;
		DecimalVector2 center = new DecimalVector2(DecimalMath.Cos(0), DecimalMath.Sin(0));
		DecimalVector2 edge = new DecimalVector2(DecimalMath.Cos(margin), DecimalMath.Sin(margin));
		decimal maxDistance = DecimalVector2.Distance(center, edge);
		DecimalVector2 submitted = new DecimalVector2(DecimalMath.Cos(current), DecimalMath.Sin(current));
		decimal submittedDistance = DecimalVector2.Distance(Position, submitted);

		decimal gradians = current / DecimalMath.Pi * 200.0M;
		string answer = string.Format("{0:0.#######} gradians", gradians);
		Bomb.LogFormat("Submitted '{0}'. Correct answer: '{1}'", answer, Name);
		Bomb.LogFormat("Submitted coordinate ({0:0.#######}, {1:0.#######}) to solution coordinate ({2:0.#######}, {3:0.#######}) yields distance {4:0.#######} out of max allowed {5:0.#######}",
			submitted.X, submitted.Y, Position.X, Position.Y, submittedDistance, maxDistance
		);
		return submittedDistance <= maxDistance;
	}

	public override decimal LargeJump(bool positive, decimal current) {
		decimal jump = (10.0M / 200.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal MediumJump(bool positive, decimal current) {
		decimal jump = (1.0M / 200.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal SmallJump(bool positive, decimal current) {
		decimal jump = (0.1M / 200.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal LargeReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = ((10.0M / 200) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal MediumReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = ((1.0M / 200) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal SmallReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = ((0.1M / 200) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}


	public Gradians(BombHelper b) : base(b) {
		decimal number;
		if (Random.Range(0f, 1f) < DECIMAL_ODDS) {
			int num = Random.Range(0, 4000);
			number = (decimal)num / 10.0M;
		}
		else {
			int num = Random.Range(0, 400);
			number = num;
		}
		if (Random.Range(0f, 1f) < DOUBLE_ODDS) {
			number += 400.0M;
		}
		if (Random.Range(0f, 1f) < NEGATIVE_ODDS) {
			number *= -1.0M;
		}
		decimal rads = number * DecimalMath.Pi / 200;
		Name = string.Format("{0:0.#}g", number);
		Position = new DecimalVector2(DecimalMath.Cos(rads), DecimalMath.Sin(rads));
	}
}