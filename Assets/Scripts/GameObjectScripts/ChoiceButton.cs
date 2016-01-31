using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChoiceButton : Button
{
	public string ChoiceText;

	Slider slider;
	Text textElement;
	LayoutElement layoutElement;
	protected override void Start()
	{
		base.Start();
		textElement = GetComponentInChildren<Text>();
		layoutElement = GetComponent<LayoutElement>();
		slider = GameObject.Find("ButtonTextSlider").GetComponent<Slider>();
	}
	void Update ()
	{
		textElement.text = ChoiceText;
		layoutElement.preferredHeight = textElement.preferredHeight+10;
		textElement.fontSize = (int)slider.value;
	}
}
