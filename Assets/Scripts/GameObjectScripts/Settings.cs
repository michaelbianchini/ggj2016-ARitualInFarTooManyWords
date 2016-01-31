using UnityEngine;
using System.Collections;

public class Settings : MonoBehaviour
{
	public void Quit()
	{
		if(Application.isEditor)
		{
			UnityEditor.EditorApplication.isPlaying = false;
		}
		else
			Application.Quit();
	}
}
