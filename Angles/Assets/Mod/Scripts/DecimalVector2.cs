using raminrahimzada;
using UnityEngine;

public struct DecimalVector2 {
	public decimal X;
	public decimal Y;

	public DecimalVector2(decimal x, decimal y) {
		X = x;
		Y = y;
	}

	private static readonly DecimalVector2 rightVector = new DecimalVector2(1.0M, 0.0M);
	private static readonly DecimalVector2 upVector = new DecimalVector2(0.0M, 1.0M);
	private static readonly DecimalVector2 leftVector = new DecimalVector2(-1.0M, 0.0M);
	private static readonly DecimalVector2 downVector = new DecimalVector2(0.0M, -1.0M);
	private static readonly DecimalVector2 zeroVector = new DecimalVector2(0.0M, 0.0M);

	public static DecimalVector2 Right => rightVector;
	public static DecimalVector2 Up => upVector;
	public static DecimalVector2 Left => leftVector;
	public static DecimalVector2 Down => downVector;
	public static DecimalVector2 Zero => zeroVector;

	public decimal Magnitude => DecimalMath.Sqrt(X * X + Y * Y);

	public static decimal Distance(DecimalVector2 a, DecimalVector2 b) {
		decimal distance = 0.0M;
		decimal width = DecimalMath.Abs(b.X - a.X);
		decimal height = DecimalMath.Abs(b.Y - a.Y);
		decimal hypotenuse = DecimalMath.Sqrt(width * width + height * height);
		return hypotenuse;
	}

	public DecimalVector2 Normalized {
		get {
			DecimalVector2 newVector = new DecimalVector2(X, Y);
			decimal num = Magnitude;
			if (num > 1E-20M) {
				return newVector /= num;
			}
			else {
				return Zero;
			}
		}
	}

	public static DecimalVector2 operator /(DecimalVector2 a, decimal d) {
		return new DecimalVector2(a.X / d, a.Y / d);
	}

}