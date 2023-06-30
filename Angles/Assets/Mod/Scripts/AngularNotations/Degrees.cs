using raminrahimzada;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Degrees : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	private readonly float NEGATIVE_ODDS = 0.4f;
	private readonly float DOUBLE_ODDS = 0.2f;

	private readonly float DEGREES_ROUND_ODDS = 0.05f;
	private readonly float DEGREES_DECIMAL_ODDS = 0.15f;
	private readonly float ARCMINUTES_ROUND_ODDS = 0.1f;
	private readonly float ARCMINUTES_DECIMAL_ODDS = 0.3f;

	private const decimal Deg2Rad = DecimalMath.Pi / 180;

	public override bool Submit(decimal current, bool log = true) {
		decimal margin = (30.0M / 648000.0M) * DecimalMath.Pi;
		DecimalVector2 center = new DecimalVector2(DecimalMath.Cos(0), DecimalMath.Sin(0));
		DecimalVector2 edge = new DecimalVector2(DecimalMath.Cos(margin), DecimalMath.Sin(margin));
		decimal maxDistance = DecimalVector2.Distance(center, edge);
		DecimalVector2 submitted = new DecimalVector2(DecimalMath.Cos(current), DecimalMath.Sin(current));
		decimal submittedDistance = DecimalVector2.Distance(Position, submitted);

		decimal seconds = current / DecimalMath.Pi * 648000.0M;
		decimal minutes = seconds / 60.0M;
		decimal degrees = minutes / 60.0M;
		string answer = string.Format("{0:0}°{1:0}′{2:0.##}″", degrees, minutes, seconds);
		if (log) {
			Bomb.LogFormat("Submitted '{0}'. Correct answer: '{1}'", answer, Name);
			Bomb.LogFormat("Submitted coordinate ({0:0.#######}, {1:0.#######}) to solution coordinate ({2:0.#######}, {3:0.#######}) yields distance {4:0.#######} out of max allowed {5:0.#######}",
				submitted.X, submitted.Y, Position.X, Position.Y, submittedDistance, maxDistance
			);
		}
		return submittedDistance <= maxDistance;
	}

	public override decimal LargeJump(bool positive, decimal current) {
		decimal jump = (1.0M / 180.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal MediumJump(bool positive, decimal current) {
		decimal jump = (6.0M / 180.0M / 60.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal SmallJump(bool positive, decimal current) {
		decimal jump = (6.0M / 180.0M / 60.0M / 60.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal LargeReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((1.0M / 180) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal MediumReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((6.0M / 180.0M / 60.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal SmallReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((6.0M / 180.0M / 60.0M / 60.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}


	public Degrees(BombHelper b) : base(b) {
		string name = "";
		int degrees = 0;
		int arcMinutes = 0;
		int arcSeconds = 0;

		degrees = Random.Range(0, 360);

		bool doubleUp = Random.Range(0f, 1f) < DOUBLE_ODDS;
		bool negate = Random.Range(0f, 1f) < NEGATIVE_ODDS;
		decimal radians;

		float randDecimal = Random.Range(0f, 1f);
		if (randDecimal < DEGREES_ROUND_ODDS) {
			if (doubleUp) { degrees += 360; }
			if (negate) { degrees *= -1; }

			name = degrees.ToString() + "°";
			Name = name;
			radians = Deg2Rad * degrees;
			Position = new DecimalVector2(DecimalMath.Cos(radians), DecimalMath.Sin(radians));
			return;
		}
		if (randDecimal < DEGREES_DECIMAL_ODDS) {
			// Ensure non-repeating decimals of less than 7 digits that results in a round number of arcseconds
			int randFive = Random.Range(0, 3);
			int randTwo = Random.Range(0, 5);
			int denominator = (int)(Mathf.Pow(5, randFive) * Mathf.Pow(2, randTwo));
			int numerator = Random.Range(1, denominator);
			decimal mantissa = (decimal)numerator / (decimal)denominator;
			decimal degF = degrees + mantissa;

			if (doubleUp) { degF += 360; };
			if (negate) { degF *= -1; }

			name = degF.ToString("0.#######") + "°";
			Name = name;
			radians = Deg2Rad * degF;
			Position = new DecimalVector2(DecimalMath.Cos(radians), DecimalMath.Sin(radians));
			return;
		}

		arcMinutes = Random.Range(0, 60);
		float randArcMinute = Random.Range(0f, 1f);
		if (randArcMinute < ARCMINUTES_ROUND_ODDS) {
			decimal degF = degrees + ((decimal)arcMinutes / 60.0M);
			if (doubleUp) { degF += 360; degrees += 360; }
			if (negate) { degF *= -1; degrees *= -1; }

			name = string.Format("{0:0}°{1:0.#######}′", degrees, arcMinutes);
			Name = name;
			radians = Deg2Rad * degF;
			Position = new DecimalVector2(DecimalMath.Cos(radians), DecimalMath.Sin(radians));
			return;
		}
		if (randArcMinute < ARCMINUTES_DECIMAL_ODDS) {
			// Ensure non-repeating decimals of less than 7 digits that results in a round number of arcseconds
			List<int> validDenominators = new List<int> { 1, 2, 4, 5, 10, 20, 25, 50 };
			int denominator = validDenominators[Random.Range(0, validDenominators.Count)];
			int numerator = Random.Range(1, denominator);
			decimal mantissa = (decimal)numerator / (decimal)denominator;
			decimal arcmF = arcMinutes + mantissa;
			decimal degF = degrees + ((decimal)arcmF / 60.0M);

			if (doubleUp) { degF += 360; degrees += 360; }
			if (negate) { degF *= -1; degrees *= -1; }

			name = string.Format("{0:0}°{1:0}′", degrees, arcmF);
			Name = name;
			radians = Deg2Rad * degF;
			Position = new DecimalVector2(DecimalMath.Cos(radians), DecimalMath.Sin(radians));
			return;
		}
		arcSeconds = Random.Range(0, 60);
		if (true) {
			decimal degF = degrees + ((decimal)arcMinutes / 60.0M) + ((decimal)arcSeconds / 3600.0M);

			if (doubleUp) { degF += 360; degrees += 360; }
			if (negate) { degF *= -1; degrees *= -1; }

			name = string.Format("{0:0}°{1:0}′{2:0}″", degrees, arcMinutes, arcSeconds);
			Name = name;
			radians = Deg2Rad * degF;
			Position = new DecimalVector2(DecimalMath.Cos(radians), DecimalMath.Sin(radians));
			return;
		}
	}
}
