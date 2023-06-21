using raminrahimzada;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turns : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	private readonly float WHOLE_ODDS = 0.1f;
	private readonly float NEGATIVE_ODDS = 0.2f;
	private readonly float DOUBLE_ODDS = 0.2f;
	private readonly float WRITEASDECIMAL_ODDS = 0.8f;

	readonly List<int> possibleDenominators = new List<int> {
		2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 15, 16, 18, 20, 22, 25, 30, 50, 60, 180, 270, 360, 720,
		2, 3, 4, 5, 6,    8, 9, 10,     12, 15, 16,           
		2, 3, 4,          8,                    16,     
	};

	public Turns(BombHelper b) : base(b) {
		decimal number;
		if (Random.Range(0f, 1f) < WHOLE_ODDS) {
			int wholeNum = Random.Range(-2, 4);
			Name = wholeNum.ToString();
			Position = DecimalVector2.Right;
			return;
		}

		int denominator = possibleDenominators[Random.Range(0, possibleDenominators.Count)];
		int numerator = Random.Range(1, denominator);

		bool doubleUp = Random.Range(0f, 1f) < DOUBLE_ODDS;
		bool negate = Random.Range(0f, 1f) < NEGATIVE_ODDS;

		if (doubleUp) { numerator += denominator; }
		if (negate) { numerator *= -1; }

		decimal decVal = numerator / denominator;
		if (new List<int> { 2, 4, 5, 10, 20, 25, 50 }.Contains(denominator)) {
			if (Random.Range(0f,1f) < WRITEASDECIMAL_ODDS) {
				Name = string.Format("{0:0.#######}", decVal);
			}
			else {
				Name = string.Format("{0}/{1}", numerator, denominator);
			}
		}
		else {
			Name = string.Format("{0}/{1}", numerator, denominator);
		}
		decimal rads = decVal / 2.0M;
		Position = new DecimalVector2(DecimalMath.Cos(rads), DecimalMath.Sin(rads));
	}
}