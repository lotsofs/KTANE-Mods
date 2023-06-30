using raminrahimzada;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hexacontades : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	private readonly float DECIMAL_ODDS = 0.2f;
	private readonly float NEGATIVE_ODDS = 0.2f;
	private readonly float DOUBLE_ODDS = 0.2f;

	public override bool Submit(decimal current, bool log = true) {
		decimal margin = (0.01M / 30.0M) * DecimalMath.Pi;
		DecimalVector2 center = new DecimalVector2(DecimalMath.Cos(0), DecimalMath.Sin(0));
		DecimalVector2 edge = new DecimalVector2(DecimalMath.Cos(margin), DecimalMath.Sin(margin));
		decimal maxDistance = DecimalVector2.Distance(center, edge);
		DecimalVector2 submitted = new DecimalVector2(DecimalMath.Cos(current), DecimalMath.Sin(current));
		decimal submittedDistance = DecimalVector2.Distance(Position, submitted);

		string firstLetter = Bomb.GetSerialFirstLetter().ToString();
		decimal startingPiAmount = 0.0M;
		if ("ABCDEFG".Contains(firstLetter)) {
			startingPiAmount = 1.5M * DecimalMath.Pi;
		}
		else if ("HIJKLMNOP".Contains(firstLetter)) {
			startingPiAmount = 0.5M * DecimalMath.Pi;
		}
		else if ("QRSTUV".Contains(firstLetter)) {
			startingPiAmount = 1.0M * DecimalMath.Pi;
		}
		else if ("WXYZ".Contains(firstLetter)) {
			startingPiAmount = 0.0M * DecimalMath.Pi;
		}

		current -= startingPiAmount;
		decimal hexacontades = current / DecimalMath.Pi * 30.0M;
		string answer = string.Format("{0:0.#######} hexacontades", hexacontades);
		if (log) {
			Bomb.LogFormat("Submitted '{0}'. Correct answer: '{1}'", answer, Name);
			Bomb.LogFormat("Submitted coordinate ({0:0.#######}, {1:0.#######}) to solution coordinate ({2:0.#######}, {3:0.#######}) yields distance {4:0.#######} out of max allowed {5:0.#######}",
				submitted.X, submitted.Y, Position.X, Position.Y, submittedDistance, maxDistance
			);
		}
		return submittedDistance <= maxDistance;
	}


	public override decimal LargeJump(bool positive, decimal current) {
		decimal jump = (1.0M / 30.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal MediumJump(bool positive, decimal current) {
		decimal jump = (0.1M / 30.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal SmallJump(bool positive, decimal current) {
		decimal jump = (0.01M / 30.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal LargeReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((1.0M / 30) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal MediumReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((0.1M / 30) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal SmallReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((0.01M / 30) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public Hexacontades(BombHelper b) : base(b) {
		string firstLetter = b.GetSerialFirstLetter().ToString();
		decimal startingPiAmount;
		if ("ABCDEFG".Contains(firstLetter)) {
			startingPiAmount = 1.5M * DecimalMath.Pi;
		}
		else if ("HIJKLMNOP".Contains(firstLetter)) {
			startingPiAmount = 0.5M * DecimalMath.Pi;
		}
		else if ("QRSTUV".Contains(firstLetter)) {
			startingPiAmount = 1.0M * DecimalMath.Pi;
		}
		else if ("WXYZ".Contains(firstLetter)) {
			startingPiAmount = 0.0M * DecimalMath.Pi;
		}
		else {
			Position = DecimalVector2.Zero;
			Name = null;
			return;
		}

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
		rads += startingPiAmount;
		Name = string.Format("{0:0.##}h", number);
		Position = new DecimalVector2(DecimalMath.Cos(rads), DecimalMath.Sin(rads));
	}
}