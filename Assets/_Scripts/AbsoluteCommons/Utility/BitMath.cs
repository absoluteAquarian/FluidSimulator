namespace AbsoluteCommons.Utility {
	public static class BitMath {
		public static byte AsByte(this bool flag) => (byte)(flag ? 1 : 0);

		public static int AsInt(this bool flag) => flag ? 1 : 0;
	}
}
