using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using raminrahimzada;

public class Radians : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	private readonly float NEGATIVE_ODDS = 0.35f;
	private readonly float DOUBLE_ODDS = 0.35f;
	private readonly float TAU_ODDS = 0.4f;

	private const decimal Deg2Rad = DecimalMath.Pi / 180;

	public override bool Submit(decimal current, bool log = true) {
		decimal margin = (1.0M / 32.0M) * DecimalMath.Pi;
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
		decimal jump = (1.0M / 4.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal MediumJump(bool positive, decimal current) {
		decimal jump = (1.0M / 16.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal SmallJump(bool positive, decimal current) {
		decimal jump = (1.0M / 64.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal LargeReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((1.0M / 4) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal MediumReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((1.0M / 16) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal SmallReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((1.0M / 64) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}


	readonly List<int> possibleDenominators = new List<int> { 
		1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 15, 16, 20, 24, 30, 32, 45, 50, 60, 64, 100, 120, 128, 160, 180, 360,
		1, 2, 3, 4, 5, 6,    8, 9, 10,     12, 15, 16, 20, 24,     32,         60, 64,           128,      180, 360,
		1, 2, 3, 4,    6,                          16,     24,                                                      
		1,                                                                                                       
		1,
	};

	public Radians(BombHelper b) : base(b) {
		int denominator = possibleDenominators[Random.Range(0, possibleDenominators.Count)];
		int numerator = Random.Range(1, denominator);

		bool doubleUp = Random.Range(0f, 1f) < DOUBLE_ODDS;
		bool negate = Random.Range(0f, 1f) < NEGATIVE_ODDS;

		char piOrTau = 'π';
		bool useTau = Random.Range(0f, 1f) < TAU_ODDS;
		if (useTau) {
			piOrTau = 'τ';
		}

		if (doubleUp) { numerator += denominator; }
		if (negate) { numerator *= -1; }

		if (denominator == 1) {
			string name = string.Format("{0}{1}", numerator.ToString(),piOrTau);
			Name = name;
		}
		else {
			Name = Name = string.Format("{0}/{1}{2}", numerator.ToString(), denominator.ToString(), piOrTau);
		}
		decimal distance = (decimal)numerator / (decimal)denominator;
		distance *= useTau ? DecimalMath.Pi * 2 : DecimalMath.Pi;
		Position = new DecimalVector2(DecimalMath.Cos(distance), DecimalMath.Sin(distance));
	}
}
