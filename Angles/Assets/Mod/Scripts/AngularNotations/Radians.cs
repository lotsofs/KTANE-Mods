using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using raminrahimzada;

public class Radians : AngularNotation {

	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	private readonly float NEGATIVE_ODDS = 0.35f;
	private readonly float DOUBLE_ODDS = 0.35f;
	private readonly float TAU_ODDS = 0.4f;

	private const decimal Deg2Rad = DecimalMath.Pi / 180;

	readonly List<int> possibleDenominators = new List<int> { 
		1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 15, 16, 20, 24, 30, 32, 45, 50, 60, 64, 100, 120, 128, 160, 180, 360,
		1, 2, 3, 4, 5, 6,    8, 9, 10,     12, 15, 16, 20, 24,     32,         60, 64,           128,      180, 360,
		1, 2, 3, 4,    6,                          16,     24,                                                      
		1,                                                                                                       
		1,
	};

	public Radians(BombHelper b) : base(b) {
		int denominator = possibleDenominators[Random.Range(0, possibleDenominators.Count)];
		int numerator = Random.Range(1, denominator);

		bool doubleUp = Random.Range(0f, 1f) < DOUBLE_ODDS;
		bool negate = Random.Range(0f, 1f) < NEGATIVE_ODDS;

		char piOrTau = 'π';
		bool useTau = Random.Range(0f, 1f) < TAU_ODDS;
		if (useTau) {
			piOrTau = 'τ';
		}

		if (doubleUp) { numerator += denominator; }
		if (negate) { numerator *= -1; }

		if (denominator == 1) {
			string name = string.Format("{0}{1}", numerator.ToString(),piOrTau);
			Name = name;
		}
		else {
			Name = Name = string.Format("{0}/{1}{2}", numerator.ToString(), denominator.ToString(), piOrTau);
		}
		decimal distance = (decimal)numerator / (decimal)denominator;
		distance *= useTau ? DecimalMath.Pi * 2 : DecimalMath.Pi;
		Position = new DecimalVector2(DecimalMath.Cos(distance), DecimalMath.Sin(distance));
	}
}
