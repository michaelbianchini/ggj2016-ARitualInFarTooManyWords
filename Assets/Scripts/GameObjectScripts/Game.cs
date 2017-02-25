﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
	[SerializeField]
	ChoiceButton _choiceButtonPrefab;
	[SerializeField]
	GameEventDatabase _eventDatabase;
	[SerializeField]
	ScrollRect _textAreaScrollView;
	[SerializeField]
	private Image _backgroundImage;
	[SerializeField]
	Transform[] _choicesAreas;

	Text _eventText;
	StringBuilder _story = new StringBuilder();
	HashSet<string> _flags = new HashSet<string>();

	void Start()
	{
		_eventText = _textAreaScrollView.GetComponentInChildren<Text>();
		if (_eventDatabase == null)
		{
			Debug.Log("GameEventDatabase not set, looking for one in resources.");
			_eventDatabase = Resources.FindObjectsOfTypeAll<GameEventDatabase>().First();
		}
		if (_eventDatabase == null)
			Debug.LogError("Unable to find a GameEventDatabase.");
		GoToEvent("Start");
	}
	IEnumerator UpdateScroll()
	{
		yield return null;
		_textAreaScrollView.verticalNormalizedPosition = 0;
	}
	void GoToEvent(string key)
	{
		if (key == "Start")
			_story = new StringBuilder();
		var e = _eventDatabase[key];
		//_eventText.text = "<color=grey>" + _story.ToString() + "</color>" + "<color=white>" + e.Text + "</color>";
		_eventText.text = "<color=white>" + e.Text + "</color>";
		_story.Append(e.Text);
		StartCoroutine(UpdateScroll());

		e.Flags.ForEach(f => _flags.Add(f));
		e.ClearFlags.ForEach(f => _flags.RemoveWhere(f2 => f2 == f));

		var opts = new List<EventOption>();
		foreach (var opt in e.Options)
		{
			if (opt.RequiredFlags.Any(f => !_flags.Contains(f))) // If we are missing any of the required flags dont add this option
				continue;
			if (opt.NotAllowedFlags.Any(f => _flags.Contains(f))) // If we have any of the not allowed flags don't add this option
				continue;
			opts.Add(opt);
		}
		foreach (var choicesArea in _choicesAreas)
		{
			choicesArea.gameObject.SetActive(false);
		}
		if (opts.Count == 0 || opts.Count > _choicesAreas.Length)
		{
			Debug.LogError("No view for " + opts.Count + " Choices");
		}
		else
		{
			var area = _choicesAreas[opts.Count - 1];
			area.gameObject.SetActive(true);
			var buttons = area.GetComponentsInChildren<ChoiceButton>();
			if (buttons.Length != opts.Count)
				Debug.LogError(area.name + " does not have the required " + opts.Count + " ChoiceButtons", area);
			for (int i = 0; i < opts.Count; i++)
			{
				var o = opts[i];
				var b = buttons[i];
				b.ChoiceText = o.Text;
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(() => GoToEvent(o.Target));
			}
		}
		if (e.ImageId.IsNullOrEmpty())
		{
			//_backgroundImage.enabled = false;
			_backgroundImage.enabled = true;
			_backgroundImage.sprite = Resources.Load<Sprite>("Images/black");
		}
		else
		{
			var img = Resources.Load<Sprite>(e.ImageId);
			if (img != null)
			{
				_backgroundImage.enabled = true;
				_backgroundImage.sprite = img;
			}
			else
			{
				Debug.LogError("No image found for " + e.ImageId);
			}

		}
		if (!e.SoundId.IsNullOrEmpty())
		{
			var ac = Resources.Load<AudioClip>(e.SoundId);
			if (ac != null)
			{
				var audioElement = GetComponentInChildren<AudioSource>();
				if (audioElement != null)
				{
					audioElement.PlayOneShot(ac, e.SoundVolume);
				}
			}
			else
			{
				Debug.LogError("No audio clip found for " + e.SoundId);
			}
		}
	}
}
