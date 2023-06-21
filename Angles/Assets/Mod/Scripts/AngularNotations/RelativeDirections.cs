using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using raminrahimzada;

public class RelativeDirections : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

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
		new WipNotentialItem(new DecimalVector2(DecimalMath.Cos(DecimalMath.Pi / 4.0M), DecimalMath.Sin(DecimalMath.Pi / 4.0M)), new string []{ "Topright", "Above Right", "Right Up", "Upper Right", "Upper Dextral", "Starboard Top" } ),
		new WipNotentialItem(new DecimalVector2(DecimalMath.Cos(DecimalMath.Pi / 4.0M*3.0M), DecimalMath.Sin(DecimalMath.Pi / 4.0M*3.0M)), new string []{ "Topleft", "Above Left", "Left Up", "Upper Left", "Upper Sinistral", "Port Top" } ),
		new WipNotentialItem(new DecimalVector2(DecimalMath.Cos(-DecimalMath.Pi / 4.0M), DecimalMath.Sin(-DecimalMath.Pi / 4.0M)), new string []{ "Bottomright", "Below Right", "Right Down", "Lower Right", "Lower Dextral", "Starboard Bottom" } ),
		new WipNotentialItem(new DecimalVector2(DecimalMath.Cos(-DecimalMath.Pi / 4.0M*3.0M), DecimalMath.Sin(-DecimalMath.Pi / 4.0M*3.0M)), new string []{ "Bottomleft", "Below Left", "Left Down", "Lower Left", "Lower Sinistral", "Port Bottom" } ),
	};

	public RelativeDirections(BombHelper b) : base(b) {
		int rand = UnityEngine.Random.Range(0, _possibleNotations.Count);
		this.Position = _possibleNotations[rand].Vector;
		int rand2 = UnityEngine.Random.Range(0, _possibleNotations[rand].Names.Length);
		this.Name = _possibleNotations[rand].Names[rand2];
	}
}
