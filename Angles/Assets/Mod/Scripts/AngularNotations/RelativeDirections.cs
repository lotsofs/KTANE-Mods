using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using raminrahimzada;

public class RelativeDirections : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

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
		decimal jump = DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal MediumJump(bool positive, decimal current) {
		decimal jump = 0.5M * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal SmallJump(bool positive, decimal current) {
		decimal jump = 0.25M * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal LargeReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = DecimalMath.Pi;
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal MediumReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = 0.5M * DecimalMath.Pi;
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal SmallReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		decimal subtractionValue = 0.25M * DecimalMath.Pi;
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}


	private class WipNotentialItem {
		public DecimalVector2 Vector;
		public string[] Names;

		public WipNotentialItem(DecimalVector2 v, string[] n) {
			Vector = v; Names = n;
		}
	}

	private List<WipNotentialItem> _possibleNotations = new List<WipNotentialItem> {
		new WipNotentialItem(DecimalVector2.Right, new string []{"Right", "Dextral", "Starboard"} ),
		new WipNotentialItem(DecimalVector2.Up, new string []{ "Top", "Above", "Over", "Up", "Upper", "Zenith" } ),
		new WipNotentialItem(DecimalVector2.Left, new string []{ "Left", "Sinistral", "Port" } ),
		new WipNotentialItem(DecimalVector2.Down, new string []{ "Bottom", "Below", "Under", "Down", "Lower", "Nadir" } ),
		new WipNotentialItem(new DecimalVector2(DecimalMath.Cos(DecimalMath.Pi / 4.0M), DecimalMath.Sin(DecimalMath.Pi / 4.0M)), new string []{ "Top right", "Above Right", "Right Up", "Upper Right", "Upper Dextral", "Starboard Top" } ),
		new WipNotentialItem(new DecimalVector2(DecimalMath.Cos(DecimalMath.Pi / 4.0M*3.0M), DecimalMath.Sin(DecimalMath.Pi / 4.0M*3.0M)), new string []{ "Top left", "Above Left", "Left Up", "Upper Left", "Upper Sinistral", "Port Top" } ),
		new WipNotentialItem(new DecimalVector2(DecimalMath.Cos(-DecimalMath.Pi / 4.0M), DecimalMath.Sin(-DecimalMath.Pi / 4.0M)), new string []{ "Bottom right", "Below Right", "Right Down", "Lower Right", "Lower Dextral", "Starboard Bottom" } ),
		new WipNotentialItem(new DecimalVector2(DecimalMath.Cos(-DecimalMath.Pi / 4.0M*3.0M), DecimalMath.Sin(-DecimalMath.Pi / 4.0M*3.0M)), new string []{ "Bottom left", "Below Left", "Left Down", "Lower Left", "Lower Sinistral", "Port Bottom" } ),
	};

	public RelativeDirections(BombHelper b) : base(b) {
		int rand = UnityEngine.Random.Range(0, _possibleNotations.Count);
		this.Position = _possibleNotations[rand].Vector;
		int rand2 = UnityEngine.Random.Range(0, _possibleNotations[rand].Names.Length);
		this.Name = _possibleNotations[rand].Names[rand2];
	}
}
