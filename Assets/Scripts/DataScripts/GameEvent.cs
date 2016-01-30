using UnityEngine;

[System.Serializable]
public class GameEvent
{
	public string Key;
	public string Text;
	public EventOption []Options;
}

[System.Serializable]
public class EventOption
{
	public string Text;
	[SerializeField]
	private string []_targets;

	public string Target
	{
		get
		{
			if(_targets.IsNullOrEmpty())
			{
				Debug.LogError("Event Option " + Text + " does not have any targets");
				return string.Empty;
			}
			return _targets[Random.Range(0, _targets.Length)];
		}
	}
}