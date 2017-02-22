using UnityEngine.UI;

public class ChoiceButton : Button
{
	public string ChoiceText;

	Slider slider;
	Text textElement;
	protected override void Start()
	{
		base.Start();
		textElement = GetComponentInChildren<Text>();
	}
	void Update()
	{
		textElement.text = ChoiceText;

	}
}
