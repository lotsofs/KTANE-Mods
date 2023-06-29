using raminrahimzada;
using System.Collections;
using System.Collections.Generic;
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

	public override bool Submit(decimal current) {
		throw new System.NotImplementedException();
	}

	public override decimal LargeJump(bool positive, decimal current) {
		decimal currentMod = current;
		while (currentMod < 0) { current += 2 * DecimalMath.Pi; }
		currentMod = current % (2 * DecimalMath.Pi);
		decimal destination = DecimalMath.Pi - currentMod;
		destination = (destination + 2*DecimalMath.Pi) % (2*DecimalMath.Pi);
		return destination - currentMod;
	}

	public override decimal MediumJump(bool positive, decimal current) {
		decimal currentMod = current;
		while (currentMod < 0) { current += 2 * DecimalMath.Pi; }
		currentMod = current % (2 * DecimalMath.Pi); 
		
		if (currentMod == DecimalMath.Pi * 0.5M) { // straight up
			if (positive) {
				//return DecimalMath.Pi * 0.5M;
				return 0.0M;
			}
			else {
				if (Name[0] == '%') {
					//return DecimalMath.Pi - DecimalMath.ATan(10.0M);
					return DecimalMath.Pi - DecimalMath.ATan(10.0M) - currentMod;
				}
				else {
					//return DecimalMath.ATan(10.0M);
					return DecimalMath.ATan(10.0M) - currentMod;
				}
			}
		}
		if (currentMod == DecimalMath.Pi * 1.5M) { // straight down
			if (positive) {
				if (Name[0] == '%') {
					//return DecimalMath.Pi + DecimalMath.ATan(10.0M);
					return DecimalMath.Pi + DecimalMath.ATan(10.0M) - currentMod;
				}
				else {
					//return -DecimalMath.ATan(10.0M);
					return -DecimalMath.ATan(10.0M) - currentMod;
				}
			}
			else {
				//return DecimalMath.Pi * 1.5M;
				return 0.0M;
			}
		}
		if (currentMod == 0.0M) { // straight right
			//return 0.0M;
			return 0.0M;
		}
		if (currentMod == 1.0M) { // straight left
			//return DecimalMath.Pi;
			return 0.0M;
		}
		if (currentMod > 0 && currentMod < DecimalMath.Pi * 0.5M) { // top right quadrant
			decimal percentage = DecimalMath.Tan(currentMod);
			percentage *= positive ? 10.0M : 0.1M;
			if (percentage >= 100.0M) {
				//return DecimalMath.Pi * 0.5M; 
				return DecimalMath.Pi * 0.5M - currentMod;
			}
			if (percentage <= 0.0001M) {
				//return 0.0M;
				return 0.0M - currentMod;
			}
			return DecimalMath.ATan(percentage);
		}
		if (currentMod > DecimalMath.Pi * 0.5M && currentMod < DecimalMath.Pi) { // top left quadrant
			decimal angle = DecimalMath.Pi - currentMod;
			decimal percentage = DecimalMath.Tan(angle);
			percentage *= positive ? 10.0M : 0.1M;
			if (percentage >= 100.0M) {
				//return DecimalMath.Pi * 0.5M;
				return DecimalMath.Pi * 0.5M - currentMod;
			}
			if (percentage <= 0.0001M) {
				//return DecimalMath.Pi;
				return DecimalMath.Pi - currentMod;
			}
			//return DecimalMath.Pi - DecimalMath.ATan(percentage);
			return DecimalMath.Pi - DecimalMath.ATan(percentage) - currentMod;
		}
		if (currentMod < DecimalMath.Pi * 1.5M && currentMod > DecimalMath.Pi) { // bottom left quadrant
			decimal angle = currentMod - DecimalMath.Pi;
			decimal percentage = DecimalMath.Tan(angle);
			percentage *= positive ? 0.1M : 10.0M;
			if (percentage >= 100.0M) {
				//return DecimalMath.Pi * 0.5M;
				return DecimalMath.Pi * 0.5M - currentMod;
			}
			if (percentage <= 0.0001M) {
				//return DecimalMath.Pi;
				return DecimalMath.Pi - currentMod;
			}
			//return DecimalMath.Pi - DecimalMath.ATan(percentage);
			return DecimalMath.Pi - DecimalMath.ATan(percentage) - currentMod;
		}
		if (currentMod < DecimalMath.Pi * 1.5M && currentMod > DecimalMath.Pi) { // bottom right quadrant
			decimal angle = 2 * DecimalMath.Pi - currentMod;
			decimal percentage = DecimalMath.Tan(angle);
			percentage *= positive ? 0.1M : 10.0M;
			if (percentage >= 100.0M) {
				//return DecimalMath.Pi * 1.5M;
				return DecimalMath.Pi * 1.5M - currentMod;
			}
			if (percentage <= 0.0001M) {
				//return 0.0M;
				return 0.0M - currentMod;
			}
			//return 2 * DecimalMath.Pi - DecimalMath.ATan(percentage);
			return 2 * DecimalMath.Pi - DecimalMath.ATan(percentage) - currentMod;
		}

		// This should never happen
		return 2 * DecimalMath.Pi;
	}

	public override decimal SmallJump(bool positive, decimal current) {
		decimal currentMod = current;
		while (currentMod < 0) { current += 2 * DecimalMath.Pi; }
		currentMod = current % (2 * DecimalMath.Pi);

		if (currentMod == DecimalMath.Pi * 0.5M) { // straight up
			if (positive) {
				return 0.0M;
			}
			else {
				if (Name[0] == '%') {
					return DecimalMath.Pi - DecimalMath.ATan(99.99M) - currentMod;
				}
				else {
					return DecimalMath.ATan(99.99M) - currentMod;
				}
			}
		}
		if (currentMod == DecimalMath.Pi * 1.5M) { // straight down
			if (positive) {
				if (Name[0] == '%') {
					return DecimalMath.Pi + DecimalMath.ATan(99.99M) - currentMod;
				}
				else {
					return -DecimalMath.ATan(99.99M) - currentMod;
				}
			}
			else {
				return 0.0M;
			}
		}
		if (currentMod == 0.0M) { // straight right
			return 0.0M;
		}
		if (currentMod == 1.0M) { // straight left
			return 0.0M;
		}
		if (currentMod > 0 && currentMod < DecimalMath.Pi * 0.5M) { // top right quadrant
			decimal percentage = DecimalMath.Tan(currentMod);
			percentage += positive ? 0.01M : -0.01M;
			if (percentage >= 100.0M) {
				return DecimalMath.Pi * 0.5M - currentMod;
			}
			if (percentage <= 0.0001M) {
				return 0.0M - currentMod;
			}
			return DecimalMath.ATan(percentage);
		}
		if (currentMod > DecimalMath.Pi * 0.5M && currentMod < DecimalMath.Pi) { // top left quadrant
			decimal angle = DecimalMath.Pi - currentMod;
			decimal percentage = DecimalMath.Tan(angle);
			percentage += positive ? 0.01M : -0.01M;
			if (percentage >= 100.0M) {
				return DecimalMath.Pi * 0.5M - currentMod;
			}
			if (percentage <= 0.0001M) {
				return DecimalMath.Pi - currentMod;
			}
			return DecimalMath.Pi - DecimalMath.ATan(percentage) - currentMod;
		}
		if (currentMod < DecimalMath.Pi * 1.5M && currentMod > DecimalMath.Pi) { // bottom left quadrant
			decimal angle = currentMod - DecimalMath.Pi;
			decimal percentage = DecimalMath.Tan(angle);
			percentage += positive ? 0.01M : -0.01M;
			if (percentage >= 100.0M) {
				return DecimalMath.Pi * 0.5M - currentMod;
			}
			if (percentage <= 0.0001M) {
				return DecimalMath.Pi - currentMod;
			}
			return DecimalMath.Pi - DecimalMath.ATan(percentage) - currentMod;
		}
		if (currentMod < DecimalMath.Pi * 1.5M && currentMod > DecimalMath.Pi) { // bottom right quadrant
			decimal angle = 2 * DecimalMath.Pi - currentMod;
			decimal percentage = DecimalMath.Tan(angle);
			percentage += positive ? 0.01M : -0.01M;
			if (percentage >= 100.0M) {
				return DecimalMath.Pi * 1.5M - currentMod;
			}
			if (percentage <= 0.0001M) {
				return 0.0M - currentMod;
			}
			return 2 * DecimalMath.Pi - DecimalMath.ATan(percentage) - currentMod;
		}

		// This should never happen
		return 2 * DecimalMath.Pi;
	}

	public override decimal LargeReset(bool positive, decimal current) {
		throw new System.NotImplementedException();
	}

	public override decimal MediumReset(bool positive, decimal current) {
		throw new System.NotImplementedException();
	}

	public override decimal SmallReset(bool positive, decimal current) {
		throw new System.NotImplementedException();
	}

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
		decimal y;
		int x = 100;
		if (sizeRng < HUGE_ODDS) {
			int r = Random.Range(205, 10000);
			y = r;
		}
		else if (sizeRng < BIG_ODDS) {
			int r = Random.Range(100, 205);
			y = r;
		}
		else if (sizeRng < UPPER_ODDS) {
			int r = Random.Range(30, 101);
			y = r;
		}
		else {
			int r = Random.Range(0, 60);
			y = (decimal)r / 2.0M;
		}
		Position = new DecimalVector2(x, y).Normalized;
		if (left) Name = string.Format("%{0:0.#}", y);
		else Name = string.Format("{0:0.#}%", y);
	}
}
