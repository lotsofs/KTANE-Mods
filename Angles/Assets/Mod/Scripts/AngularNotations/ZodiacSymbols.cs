using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using raminrahimzada;

public class ZodiacSymbols : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	public List<decimal> firstOfTheMonths = new List<decimal> {
		(91.25M - 90) * DecimalMath.Pi / 182.5M, // April
		(91.25M - 59) * DecimalMath.Pi / 182.5M, // March
		(91.25M - 31) * DecimalMath.Pi / 182.5M, // Feb
		(91.25M - 0) * DecimalMath.Pi / 182.5M, // Jan
		(456.25M - 334) * DecimalMath.Pi / 182.5M, // Dec
		(456.25M - 304) * DecimalMath.Pi / 182.5M, // Nov
		(456.25M - 273) * DecimalMath.Pi / 182.5M, // Oct
		(456.25M - 243) * DecimalMath.Pi / 182.5M, // Sept
		(456.25M - 212) * DecimalMath.Pi / 182.5M, // Aug
		(456.25M - 181) * DecimalMath.Pi / 182.5M, // July
		(456.25M - 151) * DecimalMath.Pi / 182.5M, // June
		(456.25M - 120) * DecimalMath.Pi / 182.5M, // May
	};

	public override bool Submit(decimal current, bool log = true) {
		decimal margin = ((172799.0M / 86400.0M) / 182.5M) * DecimalMath.Pi;
		DecimalVector2 center = new DecimalVector2(DecimalMath.Cos(0), DecimalMath.Sin(0));
		DecimalVector2 edge = new DecimalVector2(DecimalMath.Cos(margin), DecimalMath.Sin(margin));
		decimal maxDistance = DecimalVector2.Distance(center, edge);
		DecimalVector2 submitted = new DecimalVector2(DecimalMath.Cos(current), DecimalMath.Sin(current));
		decimal submittedDistance = DecimalVector2.Distance(Position, submitted);

		decimal degrees = current / DecimalMath.Pi * 182.5M;
		string answer = string.Format("{0:0.#######} degrees", degrees);
		if (log) {
			Bomb.LogFormat("Submitted '{0}'. Correct answer: '{1}'", answer, Name);
			Bomb.LogFormat("Submitted coordinate ({0:0.#######}, {1:0.#######}) to solution coordinate ({2:0.#######}, {3:0.#######}) yields distance {4:0.#######} out of max allowed {5:0.#######}",
				submitted.X, submitted.Y, Position.X, Position.Y, submittedDistance, maxDistance
			);
		}
		return submittedDistance <= maxDistance;
	}

	public override decimal LargeJump(bool positive, decimal current) {
		decimal jump = (31.0M / 182.5M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal MediumJump(bool positive, decimal current) {
		decimal jump = (7.0M / 182.5M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal SmallJump(bool positive, decimal current) {
		decimal jump = (1.0M / 182.5M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal LargeReset(bool positive, decimal current) {
		decimal currentModuloed = current;
		while (currentModuloed < 0) { currentModuloed += 2 * DecimalMath.Pi; }
		currentModuloed %= 2 * DecimalMath.Pi;
		if (positive) {
			foreach (decimal zodiac in firstOfTheMonths) {
				if (zodiac <= currentModuloed + 0.000000000001M) { continue; }
				return zodiac;
			}
			return firstOfTheMonths[0];
		}
		else {
			for (int i = firstOfTheMonths.Count - 1; i >= 0; i--) {
				if (firstOfTheMonths[i] >= currentModuloed - 0.000000000001M) { continue; }
				return firstOfTheMonths[i];
			}
			return firstOfTheMonths[firstOfTheMonths.Count - 1];
		}
	}

	public override decimal MediumReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((7.0M / 182.5M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal SmallReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((1.0M / 182.5M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}


	public ZodiacSymbols(BombHelper b) : base(b) {
		int rand = UnityEngine.Random.Range(0, 12);
		decimal distance = DecimalMath.Pi * 0.5M;
		switch (rand) {
			case 0: this.Name = "♈"; distance -= (79) * DecimalMath.Pi / 182.5M; break;
			case 1: this.Name = "♉"; distance -= (109) * DecimalMath.Pi / 182.5M; break;
			case 2: this.Name = "♊"; distance -= (140) * DecimalMath.Pi / 182.5M; break;
			case 3: this.Name = "♋"; distance -= (171) * DecimalMath.Pi / 182.5M; break;
			case 4: this.Name = "♌"; distance -= (203) * DecimalMath.Pi / 182.5M; break;
			case 5: this.Name = "♍"; distance -= (234) * DecimalMath.Pi / 182.5M; break;
			case 6: this.Name = "♎"; distance -= (265) * DecimalMath.Pi / 182.5M; break;
			case 7: this.Name = "♏"; distance -= (295) * DecimalMath.Pi / 182.5M; break;
			//case 8: this.Name = "⛎"; distance -= (225 + 108) * DecimalMath.Pi / 182.5M; break;
			case 8: this.Name = "♐"; distance -= (325) * DecimalMath.Pi / 182.5M; break;
			case 9: this.Name = "♑"; distance -= (355) * DecimalMath.Pi / 182.5M; break;
			case 10: this.Name = "♒"; distance -= (19) * DecimalMath.Pi / 182.5M; break;
			case 11: this.Name = "♓"; distance -= (49) * DecimalMath.Pi / 182.5M; break;
		}
		this.Position = new DecimalVector2(DecimalMath.Cos(distance), DecimalMath.Sin(distance));
	}
}