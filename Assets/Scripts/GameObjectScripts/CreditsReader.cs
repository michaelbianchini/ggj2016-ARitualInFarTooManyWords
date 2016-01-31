using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class CreditsReader : MonoBehaviour
{
	Text textBox;
	[SerializeField]
	TextAsset creditsAsset;

	void OnEnable()
	{
		GetComponent<Text>().text = creditsAsset.text;
	}
}
