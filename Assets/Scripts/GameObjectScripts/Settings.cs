using UnityEngine;
using System.Collections;

public class Settings : MonoBehaviour
{
	public void Quit()
	{
#if UNITY_EDITOR
		if (Application.isEditor)
		{
			UnityEditor.EditorApplication.isPlaying = false;
			return;
		}
#else
			Application.Quit();
#endif
	}
}
