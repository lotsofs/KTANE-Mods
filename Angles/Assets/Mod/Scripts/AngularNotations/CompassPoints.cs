using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using raminrahimzada;

public class CompassPoints : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

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
