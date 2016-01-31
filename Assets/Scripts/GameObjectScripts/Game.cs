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
	Transform _ChoicesArea;

	Text _eventText;
	StringBuilder _story = new StringBuilder();
	HashSet<string> _flags = new HashSet<string>();
	
	void Start ()
	{
		_eventText = _textAreaScrollView.GetComponentInChildren<Text>();
		GoToEvent("Start");
	}	

	void GoToEvent(string key)
	{
		if (key == "Start")
			_story = new StringBuilder();
		var e = _eventDatabase[key];
		_eventText.text = "<color=grey>" + _story.ToString() + "</color>\n" + e.Text;
		_story.Append("\n"+e.Text);
		_textAreaScrollView.verticalNormalizedPosition = 0;

		e.Flags.ForEach(f => _flags.Add(f));
		for (int i = 0; i < _ChoicesArea.childCount; i++)
		{
			Destroy(_ChoicesArea.GetChild(i).gameObject);
		}
		foreach (var opt in e.Options)
		{
			if (opt.RequiredFlags.Any(f => !_flags.Contains(f))) // If we are missing any of the required flags dont add this option
				continue;
			if (opt.NotAllowedFlags.Any(f => _flags.Contains(f))) // If we have any of the not allowed flags don't add this option
				continue;

			var o = opt;
			var b = Instantiate(_choiceButtonPrefab);
			b.ChoiseText = o.Text;
			b.onClick.AddListener(() => GoToEvent(o.Target));
			b.transform.SetParent(_ChoicesArea, false);
		}
	}
}
