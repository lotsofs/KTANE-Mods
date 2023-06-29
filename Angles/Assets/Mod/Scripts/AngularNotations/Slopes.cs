using raminrahimzada;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slopes : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	private readonly float NEGATIVE_ODDS = 0.2f;
	private readonly float LEFT_ODDS = 0.2f;
	private readonly float INFINITY_ODDS = 0.05f;
	private readonly float ZERO_ODDS = 0.05f;
	private readonly float HUGE_ODDS = 0.1f;
	private readonly float BIG_ODDS = 0.2f;
	private readonly float UPPER_ODDS = 0.4f;

	public override bool Submit(decimal current) {
		decimal margin = (15.0M / 180.0M) * DecimalMath.Pi;
		DecimalVector2 center = new DecimalVector2(DecimalMath.Cos(0), DecimalMath.Sin(0));
		DecimalVector2 edge = new DecimalVector2(DecimalMath.Cos(margin), DecimalMath.Sin(margin));
		decimal maxDistance = DecimalVector2.Distance(center, edge);
		DecimalVector2 submitted = new DecimalVector2(DecimalMath.Cos(current), DecimalMath.Sin(current));
		decimal submittedDistance = DecimalVector2.Distance(Position, submitted);

		decimal degrees = current / DecimalMath.Pi * 180.0M;
		string answer = string.Format("{0:0.#######} degrees", degrees);
		Bomb.LogFormat("Submitted '{0}'. Correct answer: '{1}'", answer, Name);
		Bomb.LogFormat("Submitted coordinate ({0:0.#######}, {1:0.#######}) to solution coordinate ({2:0.#######}, {3:0.#######}) yields distance {4:0.#######} out of max allowed {5:0.#######}",
			submitted.X, submitted.Y, Position.X, Position.Y, submittedDistance, maxDistance
		);
		return submittedDistance <= maxDistance;
	}

	public override decimal LargeJump(bool positive, decimal current) {
		decimal jump = (30.0M / 180.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal MediumJump(bool positive, decimal current) {
		decimal jump = (15.0M / 180.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal SmallJump(bool positive, decimal current) {
		decimal jump = (1.0M / 180.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal LargeReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = ((30.0M / 180.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal MediumReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = ((15.0M / 180.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal SmallReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = ((1.0M / 180.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}


	public Slopes(BombHelper b) : base(b) {
		bool left = Random.Range(0f, 1f) < LEFT_ODDS;
		bool zero = Random.Range(0f, 1f) < ZERO_ODDS;
		if (zero) {
			Position = left ? DecimalVector2.Left: DecimalVector2.Right;
			Name = left ? "%0" : "0%";
			return;
		}
		
		bool infinite = Random.Range(0f, 1f) < INFINITY_ODDS;
		bool negate = Random.Range(0f, 1f) < NEGATIVE_ODDS;
		if (infinite) {
			Position = negate ? DecimalVector2.Down: DecimalVector2.Up;
			string minus = negate ? "-" : "";
			if (left) {
				Name = string.Format("%{0}{1}", minus, "∞");
			}
			else {
				Name = string.Format("{0}{1}%", minus, "∞");
			}
			return;
		}

		float sizeRng = Random.Range(0f, 1f);
		decimal y;
		int x = 100;
		if (sizeRng < HUGE_ODDS) {
			int r = Random.Range(205, 10000);
			y = r;
		}
		else if (sizeRng < BIG_ODDS) {
			int r = Random.Range(100, 205);
			y = r;
		}
		else if (sizeRng < UPPER_ODDS) {
			int r = Random.Range(30, 101);
			y = r;
		}
		else {
			int r = Random.Range(0, 60);
			y = (decimal)r / 2.0M;
		}
		Position = new DecimalVector2(x, y).Normalized;
		if (left) Name = string.Format("%{0:0.#}", y);
		else Name = string.Format("{0:0.#}%", y);
	}
}
