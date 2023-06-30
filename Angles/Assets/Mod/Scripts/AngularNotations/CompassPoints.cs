using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using raminrahimzada;

public class CompassPoints : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	public override bool Submit(decimal current, bool log = true) {
		decimal margin = (0.000001M) * DecimalMath.Pi;
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
		decimal jump = (1.0M / 8.0M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal SmallJump(bool positive, decimal current) {
		decimal jump = (1.0M / 16.0M) * DecimalMath.Pi;
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
		decimal subtractionValue = ((1.0M / 8) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal SmallReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((1.0M / 16.0M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}


	private class WipNotentialItem {
		public string LongName;
		public string AbbreviatedName;

		public WipNotentialItem(string l, string a) {
			LongName = l; AbbreviatedName = a;
		}
	}

	private readonly float ABBREVIATION_ODDS = 0.5f;

	private List<WipNotentialItem> _possibleNotations = new List<WipNotentialItem> {
		new WipNotentialItem("North", "North"),
		new WipNotentialItem("North by east", "NbE"),
		new WipNotentialItem("North-northeast", "NNE"),
		new WipNotentialItem("Northeast by north", "NEbN"),
		new WipNotentialItem("Northeast", "NE"),
		new WipNotentialItem("Northeast by east", "NEbE"),
		new WipNotentialItem("East-northeast", "ENE"),
		new WipNotentialItem("East by north", "EbN"),
		new WipNotentialItem("East", "East"),
		new WipNotentialItem("East by south", "EbS"),
		new WipNotentialItem("East-southeast", "ESE"),
		new WipNotentialItem("Southeast by east", "SEbE"),
		new WipNotentialItem("Southeast", "SE"),
		new WipNotentialItem("Southeast by south", "SEbS"),
		new WipNotentialItem("South-southeast", "SSE"),
		new WipNotentialItem("South by east", "SbE"),
		new WipNotentialItem("South", "South"),
		new WipNotentialItem("South by west", "SbW"),
		new WipNotentialItem("South-southwest", "SSW"),
		new WipNotentialItem("Southwest by south", "SWbS"),
		new WipNotentialItem("Southwest", "SW"),
		new WipNotentialItem("Southwest by west", "SWbW"),
		new WipNotentialItem("West-southwest", "WSW"),
		new WipNotentialItem("West by south", "WbS"),
		new WipNotentialItem("West", "West"),
		new WipNotentialItem("West by north", "WbN"),
		new WipNotentialItem("West-northwest", "WNW"),
		new WipNotentialItem("Northwest by west", "NWbW"),
		new WipNotentialItem("Northwest", "NW"),
		new WipNotentialItem("Northwest by north", "NWbN"),
		new WipNotentialItem("North-northwest", "NNW"),
		new WipNotentialItem("North by west", "NbW"),
	};

	public CompassPoints(BombHelper b) : base(b) {
		string firstLetter = b.GetSerialFirstLetter().ToString();
		decimal piAmount;
		if ("ABCDEFG".Contains(firstLetter)) {
			piAmount = 0.0M;
		}
		else if ("HIJKLMNOP".Contains(firstLetter)) {
			piAmount = 1.0M;
		}
		else if ("QRSTUV".Contains(firstLetter)) {
			piAmount = 1.5M;
		}
		else if ("WXYZ".Contains(firstLetter)) {
			piAmount = 0.5M;
		}
		else {
			Position = DecimalVector2.Zero;
			Name = null;
			return;
		}

		// 1/16
		int rand = UnityEngine.Random.Range(0, _possibleNotations.Count);
		piAmount -= (rand / 16.0M);
		Name = UnityEngine.Random.Range(0f, 1f) < ABBREVIATION_ODDS ? _possibleNotations[rand].AbbreviatedName : _possibleNotations[rand].LongName;
		Position = new DecimalVector2(DecimalMath.Cos(piAmount * DecimalMath.Pi), DecimalMath.Sin(piAmount * DecimalMath.Pi));
	}
}
