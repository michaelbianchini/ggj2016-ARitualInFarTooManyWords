using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChoiceButton : Button
{
	public string ChoiceText;

	Text textElement;
	LayoutElement layoutElement;
	protected override void Start()
	{
		base.Start();
		textElement = GetComponentInChildren<Text>();
		layoutElement = GetComponent<LayoutElement>();
	}
	// Update is called once per frame
	void Update ()
	{
		textElement.text = ChoiceText;
		layoutElement.preferredHeight = textElement.preferredHeight+10;
	}
}
