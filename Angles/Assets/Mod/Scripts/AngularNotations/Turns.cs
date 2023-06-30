using raminrahimzada;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turns : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }


	private readonly float WHOLE_ODDS = 0.2f;
	private readonly float NEGATIVE_ODDS = 0.2f;
	private readonly float DOUBLE_ODDS = 0.2f;
	private readonly float WRITEASDECIMAL_ODDS = 0.8f;

	public override bool Submit(decimal current, bool log = true) {
		decimal margin = 0.02M * DecimalMath.Pi;
		DecimalVector2 center = new DecimalVector2(DecimalMath.Cos(0), DecimalMath.Sin(0));
		DecimalVector2 edge = new DecimalVector2(DecimalMath.Cos(margin), DecimalMath.Sin(margin));
		decimal maxDistance = DecimalVector2.Distance(center, edge);
		DecimalVector2 submitted = new DecimalVector2(DecimalMath.Cos(current), DecimalMath.Sin(current));
		decimal submittedDistance = DecimalVector2.Distance(Position, submitted);

		decimal turns = current / 2 * DecimalMath.Pi;
		string answer = string.Format("{0:0.#######} gradians", turns);
		if (log) {
			Bomb.LogFormat("Submitted '{0}'. Correct answer: '{1}'", answer, Name);
			Bomb.LogFormat("Submitted coordinate ({0:0.#######}, {1:0.#######}) to solution coordinate ({2:0.#######}, {3:0.#######}) yields distance {4:0.#######} out of max allowed {5:0.#######}",
				submitted.X, submitted.Y, Position.X, Position.Y, submittedDistance, maxDistance
			);
		}
		return submittedDistance <= maxDistance;
	}


	public override decimal LargeJump(bool positive, decimal current) {
		decimal jump = 2 * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal MediumJump(bool positive, decimal current) {
		decimal jump = 0.2M * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal SmallJump(bool positive, decimal current) {
		decimal jump = 0.02M * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal LargeReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((2) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder == 0) { solution -= subtractionValue; }
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal MediumReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((0.2M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal SmallReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((0.02M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}


	readonly List<int> possibleDenominators = new List<int> {
		2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 15, 16, 18, 20, 22, 25, 30, 50, 60, 180, 270, 360, 720,
		2, 3, 4, 5, 6,    8, 9, 10,     12, 15, 16,           
		2, 3, 4,          8,                    16,     
	};

	public Turns(BombHelper b) : base(b) {
		decimal number;
		decimal radians;
		if (Random.Range(0f, 1f) < WHOLE_ODDS) {
			int wholeNum = Random.Range(-2, 4);
			Name = wholeNum.ToString();
			Position = DecimalVector2.Right;
			radians = 0;
			return;
		}

		int denominator = possibleDenominators[Random.Range(0, possibleDenominators.Count)];
		int numerator = Random.Range(1, denominator);

		bool doubleUp = Random.Range(0f, 1f) < DOUBLE_ODDS;
		bool negate = Random.Range(0f, 1f) < NEGATIVE_ODDS;

		if (doubleUp) { numerator += denominator; }
		if (negate) { numerator *= -1; }

		decimal decVal = (decimal)numerator / (decimal)denominator;
		if (new List<int> { 2, 4, 5, 10, 20, 25, 50 }.Contains(denominator)) {
			if (Random.Range(0f,1f) < WRITEASDECIMAL_ODDS) {
				Name = string.Format("{0:0.#######}", decVal);
			}
			else {
				Name = string.Format("{0}/{1}", numerator, denominator);
			}
		}
		else {
			Name = string.Format("{0}/{1}", numerator, denominator);
		}
		radians = decVal * 2.0M * DecimalMath.Pi;
		Position = new DecimalVector2(DecimalMath.Cos(radians), DecimalMath.Sin(radians));
	}
}