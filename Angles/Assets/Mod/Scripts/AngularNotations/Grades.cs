using raminrahimzada;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Grades : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	private readonly float NEGATIVE_ODDS = 0.2f;
	private readonly float LEFT_ODDS = 0.2f;
	private readonly float INFINITY_ODDS = 0.05f;
	private readonly float ZERO_ODDS = 0.05f;
	private readonly float HUGE_ODDS = 0.1f;
	private readonly float BIG_ODDS = 0.2f;
	private readonly float UPPER_ODDS = 0.4f;

	public Grades(BombHelper b) : base(b) {
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
		decimal opposite;
		int adjacent = 100;
		if (sizeRng < HUGE_ODDS) {
			int opp = Random.Range(205, 10000);
			opposite = opp;
		}
		else if (sizeRng < BIG_ODDS) {
			int opp = Random.Range(100, 205);
			opposite = opp;
		}
		else if (sizeRng < UPPER_ODDS) {
			int opp = Random.Range(30, 101);
			opposite = opp;
		}
		else {
			int opp = Random.Range(0, 60);
			opposite = (decimal)opp / 2.0M;
		}
		decimal hypotenuse = DecimalMath.Sqrt(adjacent*adjacent+opposite*opposite);
		decimal x = opposite / hypotenuse;
		decimal y = adjacent / hypotenuse;
		if (left) x = -x;
		if (negate) {
			opposite = -opposite;
			y = -y;
		}
		Position = new DecimalVector2(x, y);
		if (left) Name = string.Format("%{0}", opposite);
		else Name = string.Format("{0}%}", opposite);
	}
}
