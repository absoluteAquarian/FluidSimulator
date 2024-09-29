namespace AbsoluteCommons.Utility {
	public static class AngleMath {
		public static float AngleToScale(float angle, float maxNegative, float maxPositive) => angle == 0 ? 0 : angle < 0 ? angle / maxNegative : angle / maxPositive;
	}
}
