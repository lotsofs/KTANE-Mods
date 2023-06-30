using raminrahimzada;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DateNotation : AngularNotation {
	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	List<string> months = new List<string> { "ERROR", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

	public List<decimal> changeovers = new List<decimal> {
		(91.25M - 79) * DecimalMath.Pi / 182.5M, // Aries
		(91.25M - 49) * DecimalMath.Pi / 182.5M, // Pisces
		(91.25M - 19) * DecimalMath.Pi / 182.5M, // Aqua
		(456.25M - 355) * DecimalMath.Pi / 182.5M, // Capri
		(456.25M - 325) * DecimalMath.Pi / 182.5M, // Sagit
		(456.25M - 295) * DecimalMath.Pi / 182.5M, // Scorpius
		(456.25M - 265) * DecimalMath.Pi / 182.5M, // Libra
		(456.25M - 234) * DecimalMath.Pi / 182.5M, // Virgo
		(456.25M - 203) * DecimalMath.Pi / 182.5M, // Leo
		(456.25M - 171) * DecimalMath.Pi / 182.5M, // Cancer
		(456.25M - 140) * DecimalMath.Pi / 182.5M, // Gemini
		(456.25M - 109) * DecimalMath.Pi / 182.5M, // Taurus
	};

	public override bool Submit(decimal current, bool log = true) {
		decimal margin = ((172799.0M / 86400.0M) / 182.5M) * DecimalMath.Pi;
		DecimalVector2 center = new DecimalVector2(DecimalMath.Cos(0), DecimalMath.Sin(0));
		DecimalVector2 edge = new DecimalVector2(DecimalMath.Cos(margin), DecimalMath.Sin(margin));
		decimal maxDistance = DecimalVector2.Distance(center, edge);
		DecimalVector2 submitted = new DecimalVector2(DecimalMath.Cos(current), DecimalMath.Sin(current));
		decimal submittedDistance = DecimalVector2.Distance(Position, submitted);

		decimal degrees = current / DecimalMath.Pi * 182.5M;
		string answer = string.Format("{0:0.#######} degrees", degrees);
		if (log) {
			Bomb.LogFormat("Submitted '{0}'. Correct answer: '{1}'", answer, Name);
			Bomb.LogFormat("Submitted coordinate ({0:0.#######}, {1:0.#######}) to solution coordinate ({2:0.#######}, {3:0.#######}) yields distance {4:0.#######} out of max allowed {5:0.#######}",
				submitted.X, submitted.Y, Position.X, Position.Y, submittedDistance, maxDistance
			);
		}
		return submittedDistance <= maxDistance;
	}

	public override decimal LargeJump(bool positive, decimal current) {
		decimal jump = (31.0M / 182.5M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);      
	}

	public override decimal MediumJump(bool positive, decimal current) {
		decimal jump = (7.0M / 182.5M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal SmallJump(bool positive, decimal current) {
		decimal jump = (1.0M / 182.5M) * DecimalMath.Pi;
		return jump * (positive ? 1 : -1);
	}

	public override decimal LargeReset(bool positive, decimal current) {
		decimal currentModuloed = current;
		while (currentModuloed < 0) { currentModuloed += 2 * DecimalMath.Pi; }
		currentModuloed %= 2 * DecimalMath.Pi;
		if (positive) {
			foreach (decimal zodiac in changeovers) {
				if (zodiac <= currentModuloed + 0.000000000001M) { continue; }
				return zodiac;
			}
			return changeovers[0];
		}
		else {
			for (int i = changeovers.Count - 1; i >= 0; i--) {
				if (changeovers[i] >= currentModuloed - 0.000000000001M) { continue; }
				return changeovers[i];
			}
			return changeovers[changeovers.Count - 1];
		}
	}

	public override decimal MediumReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((7.0M / 182.5M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}

	public override decimal SmallReset(bool positive, decimal current) {
		while (current < 0) { current += DecimalMath.Pi * 2; }
		current %= DecimalMath.Pi * 2;
		decimal subtractionValue = ((1.0M / 182.5M) * DecimalMath.Pi);
		decimal remainder = current % subtractionValue;
		decimal solution = current - remainder;
		if (positive) solution += subtractionValue;
		else if (remainder < 0.0000001M) solution -= subtractionValue;
		return solution;
	}


	float MONTH_FULLNAME_ODDS = 0.4f;
	float MONTH_ABBRV_ODDS = 0.2f;

	public DateNotation(BombHelper b) : base(b) {
		DateTime date = new DateTime(2001, 1, 1);
		date = date.AddDays(UnityEngine.Random.Range(0, 365));
		int month = date.Month;
		int day = date.Day;
		int yearDay = (date.DayOfYear + 365 /*- 109*/) % 365;

		float rand = UnityEngine.Random.Range(0f, 1f);
		if (rand < MONTH_ABBRV_ODDS) {
			Name = string.Format("{0} {1}", months[month].Substring(0, 3), day);
		}
		else if (rand < MONTH_FULLNAME_ODDS) {
			Name = string.Format("{0} {1}", months[month], day);
		}
		else {
			Name = string.Format("{0} {1}", month, day);
		}
		
		decimal rads = yearDay * DecimalMath.Pi / 182.5M;
		//decimal april19Offset = (109.0M / 365.0M / 2.0M) * DecimalMath.Pi;
		decimal distance = (DecimalMath.Pi * 0.5M) - rads;
		Position = new DecimalVector2(DecimalMath.Cos(distance), DecimalMath.Sin(distance));
	}
}
