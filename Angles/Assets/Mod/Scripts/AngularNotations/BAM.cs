using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using raminrahimzada;
using System;

public class BAM : AngularNotation {
	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	public override bool Submit(decimal current) {
		decimal margin = (0.000001M) * DecimalMath.Pi;
		DecimalVector2 center = new DecimalVector2(DecimalMath.Cos(0), DecimalMath.Sin(0));
		DecimalVector2 edge = new DecimalVector2(DecimalMath.Cos(margin), DecimalMath.Sin(margin));
		decimal maxDistance = DecimalVector2.Distance(center, edge);
		DecimalVector2 submitted = new DecimalVector2(DecimalMath.Cos(current), DecimalMath.Sin(current));
		decimal submittedDistance = DecimalVector2.Distance(Position, submitted);

		while (current < 0) current += 2 * DecimalMath.Pi;
		current %= 2 * DecimalMath.Pi;
		decimal binary = current / DecimalMath.Pi * 128.0M;
		int binaryInt = Mathf.RoundToInt((float)binary);
		string binaryS = Convert.ToString(binaryInt, 2).PadLeft(8, '0');
		string answer = string.Format("0b{0} ({1:0.#######})", binaryS, current);
		Bomb.LogFormat("Submitted '{0}'. Correct answer: '{1}'", answer, Name);
		Bomb.LogFormat("Submitted coordinate ({0:0.#######}, {1:0.#######}) to solution coordinate ({2:0.#######}, {3:0.#######}) yields distance {4:0.#######} out of max allowed {5:0.#######}",
			submitted.X, submitted.Y, Position.X, Position.Y, submittedDistance, maxDistance
		);
		return submittedDistance <= maxDistance;
	}

	public override decimal LargeJump(bool positive, decimal current) {
		decimal jump = (16.0M / 128.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal MediumJump(bool positive, decimal current) {
		decimal jump = (4.0M / 128.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal SmallJump(bool positive, decimal current) {
		decimal jump = (1.0M / 128.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal LargeReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = ((16.0M / 128.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal MediumReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = ((4.0M / 128.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal SmallReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = ((1.0M / 128.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}


	public BAM(BombHelper b) : base(b) {
		decimal number;
		int num = UnityEngine.Random.Range(0, 256);
		number = num;
		decimal rads = number * DecimalMath.Pi / 128;
		string binary = Convert.ToString(num, 2).PadLeft(8, '0');
		Name = string.Format("0b{0}", binary);
		Position = new DecimalVector2(DecimalMath.Cos(rads), DecimalMath.Sin(rads));
	}

}
