﻿using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameEvent
{
	public string Key = string.Empty;
	public string Text = string.Empty;
	public string SoundId = string.Empty;
	public float SoundVolume = 1;
	public string ImageId = string.Empty;
	public List<string> Flags = new List<string>();
	public List<string> ClearFlags = new List<string>();
	public List<EventOption> Options = new List<EventOption>();
}

[System.Serializable]
public class EventOption
{
	public string Text = string.Empty;
	public List<string> Targets = new List<string>();
	public List<string> RequiredFlags = new List<string>();
	public List<string> NotAllowedFlags = new List<string>();

	public string Target
	{
		get
		{
			if (Targets.IsNullOrEmpty())
			{
				Debug.LogError("Event Option " + Text + " does not have any targets");
				return string.Empty;
			}
			return Targets[Random.Range(0, Targets.Count)];
		}
	}
}