using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FontSizeAdjuster : MonoBehaviour
{
	public float FontSize
	{
		get { return (float)text.fontSize; }
		set { text.fontSize = (int)value; }
	}
	Text text;
	//Slider slider;
	void Start()
	{
		text = GetComponent<Text>();
		//slider = GameObject.Find("TextSizeSlider").GetComponent<Slider>();
	}
	//void Update()
	//{
	//	text.fontSize = (int)slider.value;
	//}
}
