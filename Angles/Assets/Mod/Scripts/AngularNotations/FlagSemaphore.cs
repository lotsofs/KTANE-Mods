using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using raminrahimzada;

public class FlagSemaphore : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	private class WipNotentialItem {
		public DecimalVector2 Vector;
		public string[] Names;

		public WipNotentialItem(DecimalVector2 v, string[] n) {
			Vector = v; Names = n;
		}
	}

	public override bool Submit(decimal current) {
		decimal margin = (0.000001M) * DecimalMath.Pi;
		DecimalVector2 center = new DecimalVector2(DecimalMath.Cos(0), DecimalMath.Sin(0));
		DecimalVector2 edge = new DecimalVector2(DecimalMath.Cos(margin), DecimalMath.Sin(margin));
		decimal maxDistance = DecimalVector2.Distance(center, edge);
		DecimalVector2 submitted = new DecimalVector2(DecimalMath.Cos(current), DecimalMath.Sin(current));
		decimal submittedDistance = DecimalVector2.Distance(Position, submitted);

		string answer = string.Format("{0:0.#######} radians", current);
		Bomb.LogFormat("Submitted '{0}'. Correct answer: '{1}'", answer, Name);
		Bomb.LogFormat("Submitted coordinate ({0:0.#######}, {1:0.#######}) to solution coordinate ({2:0.#######}, {3:0.#######}) yields distance {4:0.#######} out of max allowed {5:0.#######}",
			submitted.X, submitted.Y, Position.X, Position.Y, submittedDistance, maxDistance
		);
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
		decimal subtractionValue = ((1.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal MediumReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = ((1.0M / 2) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal SmallReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = ((1.0M / 4) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}


	private static DecimalVector2 BottomLeft = new DecimalVector2(DecimalMath.Cos(-DecimalMath.Pi / 4.0M * 3.0M), DecimalMath.Sin(-DecimalMath.Pi / 4.0M * 3.0M));
	private static DecimalVector2 TopLeft = new DecimalVector2(DecimalMath.Cos(DecimalMath.Pi / 4.0M * 3.0M), DecimalMath.Sin(DecimalMath.Pi / 4.0M * 3.0M));
	private static DecimalVector2 BottomRight = new DecimalVector2(DecimalMath.Cos(-DecimalMath.Pi / 4.0M), DecimalMath.Sin(-DecimalMath.Pi / 4.0M));
	private static DecimalVector2 TopRight = new DecimalVector2(DecimalMath.Cos(DecimalMath.Pi / 4.0M), DecimalMath.Sin(DecimalMath.Pi / 4.0M));

	private Dictionary<char, DecimalVector2> flags = new Dictionary<char, DecimalVector2> {
		{ 'A', DecimalVector2.Down },
		{ 'B', DecimalVector2.Down },
		{ 'C', DecimalVector2.Down },
		{ 'D', DecimalVector2.Down },
		{ 'E', TopRight },
		{ 'F', DecimalVector2.Right },
		{ 'G', BottomRight },
		{ 'H', BottomLeft },
		{ 'I', BottomLeft },
		{ 'K', DecimalVector2.Up },
		{ 'L', TopRight },
		{ 'M', DecimalVector2.Right },
		{ 'N', BottomRight },
		{ 'O', TopLeft },
		{ 'P', DecimalVector2.Up },
		{ 'Q', TopLeft },
		{ 'R', DecimalVector2.Right },
		{ 'S', BottomRight },
		{ 'T', DecimalVector2.Up },
		{ 'U', TopLeft },
		{ 'Y', DecimalVector2.Right },
		{ 'J', DecimalVector2.Right },
		{ 'V', BottomRight },
		{ 'W', DecimalVector2.Right },
		{ 'X', BottomRight },
		{ 'Z', DecimalVector2.Right },

		{ 'a', BottomLeft },
		{ 'b', DecimalVector2.Left },
		{ 'c', TopLeft },
		{ 'd', DecimalVector2.Up },
		{ 'e', DecimalVector2.Down },
		{ 'f', DecimalVector2.Down },
		{ 'g', DecimalVector2.Down },
		{ 'h', DecimalVector2.Left },
		{ 'i', TopLeft },
		{ 'k', BottomLeft },
		{ 'l', BottomLeft },
		{ 'm', BottomLeft },
		{ 'n', BottomLeft },
		{ 'o', DecimalVector2.Left },
		{ 'p', DecimalVector2.Left },
		{ 'q', DecimalVector2.Left },
		{ 'r', DecimalVector2.Left },
		{ 's', DecimalVector2.Left },
		{ 't', TopLeft },
		{ 'u', TopLeft },
		{ 'y', TopLeft },
		{ 'j', DecimalVector2.Up },
		{ 'v', DecimalVector2.Up },
		{ 'w', TopRight },
		{ 'x', TopRight },
		{ 'z', BottomRight },
	};

	private string chars = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm";

	public FlagSemaphore(BombHelper b) : base(b) {
		int rand = UnityEngine.Random.Range(0, chars.Length);
		char ch = chars[rand];
		this.Name = ch.ToString();
		this.Position = flags[ch];
	}
}
