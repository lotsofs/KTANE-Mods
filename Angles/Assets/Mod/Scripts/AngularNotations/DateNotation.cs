using raminrahimzada;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DateNotation : AngularNotation {
	public override DecimalVector2 Position { get; protected set; }
	public override string Name { get; protected set; }

	List<string> months = new List<string> { "ERROR", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

	float MONTH_FULLNAME_ODDS = 0.4f;
	float MONTH_ABBRV_ODDS = 0.2f;

	public DateNotation(BombHelper b) : base(b) {
		DateTime date = new DateTime(2001, 1, 1);
		date = date.AddDays(UnityEngine.Random.Range(0, 365));
		int month = date.Month;
		int day = date.Day;
		int yearDay = date.DayOfYear;

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
		decimal distance = (DecimalMath.Pi * 0.5M) - rads;
		Position = new DecimalVector2(DecimalMath.Cos(rads), DecimalMath.Sin(rads));
	}
}
