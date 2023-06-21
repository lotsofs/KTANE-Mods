using raminrahimzada;

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
}