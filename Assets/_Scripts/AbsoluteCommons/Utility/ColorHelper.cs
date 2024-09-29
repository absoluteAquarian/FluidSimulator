using UnityEngine;

namespace AbsoluteCommons.Utility {
	public static class ColorHelper {
		public static Color FromRGB(byte r, byte g, byte b) => new Color(r / 255f, g / 255f, b / 255f);
	}
}
