using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChoiceButton : Button
{
	public string ChoiseText;

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
		textElement.text = ChoiseText;
		layoutElement.preferredHeight = textElement.preferredHeight;
	}
}
