using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AngularNotation {

	public abstract DecimalVector2 Position { get; protected set; }

	public abstract string Name { get; protected set; }

	protected BombHelper Bomb { get; set; }

	public AngularNotation(BombHelper b) {
		Bomb = b;
	}
}
