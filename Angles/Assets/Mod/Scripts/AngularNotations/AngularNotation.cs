using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AngularNotation {

	public abstract DecimalVector2 Position { get; protected set; }

	public abstract string Name { get; protected set; }

	public abstract decimal LargeJump(bool positive, decimal current);
	public abstract decimal MediumJump(bool positive, decimal current);
	public abstract decimal SmallJump(bool positive, decimal current);
	public abstract decimal LargeReset(bool positive, decimal current);
	public abstract decimal MediumReset(bool positive, decimal current);
	public abstract decimal SmallReset(bool positive, decimal current);
	public abstract bool Submit(decimal current, bool log = true);

	public string PointName;

	protected BombHelper Bomb { get; set; }

	public AngularNotation(BombHelper b) {
		Bomb = b;
	}
}
