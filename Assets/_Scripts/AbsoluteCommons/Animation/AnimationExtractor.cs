using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace AbsoluteCommons.Animation {
	// Taken from: https://stackoverflow.com/questions/22662008/how-to-create-anim-file-from-fbx-file-in-unity
	public class AnimationExtractor : MonoBehaviour {
		[MenuItem("Assets/Extract Animation")]
		private static void ExtractAnimation() {
			foreach (Object obj in Selection.objects) {
				string fbx = AssetDatabase.GetAssetPath(obj);
				string directory = Path.GetDirectoryName(fbx);
				CreateAnim(fbx, directory);
			}
		}

		private static void CreateAnim(string fbx, string target) {
			string fileName = Path.GetFileNameWithoutExtension(fbx);
			string filePath = Path.Combine(target, fileName + ".anim");
			AnimationClip src = AssetDatabase.LoadAssetAtPath<AnimationClip>(fbx);
			AnimationClip temp = new AnimationClip();
			EditorUtility.CopySerialized(src, temp);
			AssetDatabase.CreateAsset(temp, filePath);
			AssetDatabase.SaveAssets();
		}
	}
}
#endif
