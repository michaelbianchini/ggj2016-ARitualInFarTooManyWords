using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class GameEventDatabaseEditor : EditorWindow
{
	private enum State
	{
		BLANK,
		EDIT,
		PARSE,
	}

	private State state;
	private int selectedEvent;
	private string newDatabaseName;
	private List<GameEventDatabase> databases;

	private string lastFolder = string.Empty;
	private const string DATABASE_PATH = @"Assets/Resource/Database/";

	private GameEventDatabase currentDatabase;
	private Vector2 _scrollPos;
	private Vector2 _optionsScrollPos;

	[MenuItem("Database/Edit Game Events Database %#w")]
	public static void Init()
	{
		GameEventDatabaseEditor window = EditorWindow.GetWindow<GameEventDatabaseEditor>();
		window.minSize = new Vector2(800, 400);
		window.Show();
	}

	void OnEnable()
	{
		if (currentDatabase == null)
			LoadDatabase();

		state = State.BLANK;
	}

	void OnGUI()
	{
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		DisplayDatabasesArea();
		if (currentDatabase != null)
		{
			DisplayListArea();
			DisplayMainArea();
		}
		EditorGUILayout.EndHorizontal();
	}


	void LoadDatabase()
	{
		databases = new List<GameEventDatabase>();
		foreach (var path in AssetDatabase.GetAllAssetPaths())
		{
			if (path.EndsWith(".asset"))
			{
				var db = AssetDatabase.LoadAssetAtPath(path, typeof(GameEventDatabase)) as GameEventDatabase;
				if (db != null)
					databases.Add(db);
			}
		}
		if (currentDatabase == null)
			currentDatabase = databases.FirstOrDefault();

		if (currentDatabase == null)
			CreateDatabase("default");
	}

	void CreateDatabase(string name)
	{
		currentDatabase = ScriptableObject.CreateInstance<GameEventDatabase>();
		AssetDatabase.CreateAsset(currentDatabase, DATABASE_PATH + name + ".asset");
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	private void DisplayDatabasesArea()
	{
		EditorGUILayout.BeginVertical(GUILayout.Width(250));
		EditorGUILayout.Space();
		for (int i = 0; i < databases.Count; i++)
		{
			try
			{
				if (GUILayout.Button(databases[i].name, GUILayout.ExpandWidth(true)))
				{
					currentDatabase = databases[i];
				}
			}
			catch (Exception exc)
			{
				Debug.Log(exc.Message);
				databases.RemoveAt(i);
				i--;
			}
		}
		EditorGUILayout.BeginHorizontal();
		GUI.enabled = !string.IsNullOrEmpty(newDatabaseName);
		if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
		{
			CreateDatabase(newDatabaseName);
			LoadDatabase();
			newDatabaseName = string.Empty;
		}
		GUI.enabled = true;
		newDatabaseName = EditorGUILayout.TextField(newDatabaseName, GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
	}
	void DisplayListArea()
	{
		EditorGUILayout.BeginVertical(GUILayout.Width(250));
		EditorGUILayout.Space();
		EditorGUILayout.LabelField(currentDatabase.name);
		_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, "box", GUILayout.ExpandHeight(true));

		for (int i = 0; i < currentDatabase.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("-", GUILayout.Width(25)))
			{
				currentDatabase.RemoveAt(i);
				EditorUtility.SetDirty(currentDatabase);
				state = State.BLANK;
				return;
			}
			if (currentDatabase[i] != null)
			{
				if (GUILayout.Button(currentDatabase[i].Key, "box", GUILayout.ExpandWidth(true)))
				{
					selectedEvent = i;
					state = State.EDIT;
				}
			}
			EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(25));

			EditorGUILayout.EndHorizontal();
		}
		if (GUILayout.Button("+", GUILayout.Width(100)))
		{
			selectedEvent = currentDatabase.Add(new GameEvent());
			state = State.EDIT;
		}

		EditorGUILayout.EndScrollView();

		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		EditorGUILayout.LabelField("Events: " + currentDatabase.Count, GUILayout.Width(100));
		if (GUILayout.Button("Parse"))
		{
			state = State.PARSE;
		}
		if (GUILayout.Button("Verify"))
		{
			Verify();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();
	}

	void DisplayMainArea()
	{
		EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
		EditorGUILayout.Space();

		switch (state)
		{
			case State.EDIT:
				DisplayEditMainArea();
				break;
			case State.PARSE:
				DisplayParseMainArea();
				break;
			default:
				DisplayBlankMainArea();
				break;
		}

		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();
	}

	private void DisplayParseMainArea()
	{
		EditorGUILayout.LabelField(
			"Are you sure you want to re-parse the database from a file?\n" +
			"This will completely overwrite your current database.\n",
			GUILayout.ExpandHeight(true));
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Yes, Chose a file!"))
		{
			var path = EditorUtility.OpenFilePanel("Chose an adventure text file.", lastFolder, "txt");
			if (!string.IsNullOrEmpty(path))
			{
				lastFolder = Path.GetDirectoryName(path);
				using (var s = new StreamReader(path))
				{
					var str = s.ReadToEnd();
					ParseDatabaseText(str);
				}
			}
		}
		else if (GUILayout.Button("Cancel."))
		{
			state = State.BLANK;
		}
		EditorGUILayout.EndHorizontal();

	}

	void ParseDatabaseText(string str)
	{
		str = str + @"[TheGameBroke]
You Died because the game developers didn’t write a scenario to handle that last choice.
<>I guess I’ll start over![Start]
;";
		currentDatabase.Clear();
		var eventRegex = new Regex(@"\[[\s\S]+?(?=;)", RegexOptions.Multiline);
		var keyAndTextRegex = new Regex(@"\[(?<key>.+?)\] ?(?<flags>{[\w,. !:/]+})?(\r\n)?(?<text>[\s\S]+?)(?=<)", RegexOptions.Multiline);
		var optionRegex = new Regex("(?<flags><[!,\\w ]*>) ?(?<text>[\\s\\S]+?\\]) *(?:\n|\r|\r\n)", RegexOptions.Multiline);
		var targetRegex = new Regex(@"\[(?<target>.+?)\]", RegexOptions.Multiline);
		var choiceTextRegex = new Regex(@"^([\s\S]+?(?=\[))", RegexOptions.Multiline);
		var flagsRegex = new Regex(@"!?[\w:./]+");
		//var optionFlagsRegex = new Regex("<(?:!?\\w+,?)+>");

		var eventMatches = eventRegex.Matches(str);
		foreach (Match em in eventMatches)
		{
			var e = new GameEvent();
			var km = keyAndTextRegex.Match(em.Value);
			e.Key = km.Groups["key"].Value;
			e.Text = km.Groups["text"].Value;

			string flags = km.Groups["flags"].Value;
			var flagMatches = flagsRegex.Matches(flags);
			foreach (Match fm in flagMatches)
			{
				var flag = fm.Value.TrimStart('!');
				if (flag.Contains(':'))
				{
					if (flag.StartsWith("s", StringComparison.InvariantCultureIgnoreCase))
					{
						// Doing two separate trims here as we only want to trim the s before the colon, inscase the soudn name starts with s
						flag = flag.TrimStart('s', 'S').TrimStart(':');
						var v = 1.0f;
						if (flag.Contains(':'))
						{
							var vol = flag.Substring(flag.IndexOf(':') + 1);
							v = float.Parse(vol);
							if (v < 0 || v > 1)
								Debug.LogError("Sound Volume must be between 0 and 1 in event " + e.Key);
							v = Mathf.Clamp01(v);

							flag = flag.Substring(0, flag.IndexOf(':'));
						}
						e.SoundVolumes.Add(v);
						e.SoundIds.Add(flag);
					}
					else if (flag.StartsWith("i", StringComparison.InvariantCultureIgnoreCase))
					{
						// Doing two separate trims here as we only want to trim the s before the colon, inscase the soudn name starts with i
						flag = flag.TrimStart('i', 'I').TrimStart(':');
						e.ImageId = flag;
					}
					else
					{
						Debug.LogError("Unrecognized flag in event " + e.Key);
					}
				}
				else
				{
					if (fm.Value.StartsWith("!"))
						e.ClearFlags.Add(flag);
					else
						e.Flags.Add(flag);
				}
			}

			var optionMatches = optionRegex.Matches(em.Value);
			foreach (Match om in optionMatches)
			{
				var eo = new EventOption();
				var omText = om.Groups["text"];
				eo.Text = choiceTextRegex.Match(omText.Value).Value;

				var flagMatches2 = flagsRegex.Matches(om.Groups["flags"].Value);
				foreach (Match fm in flagMatches2)
				{
					var flag = fm.Value.TrimStart('!');
					if (fm.Value.StartsWith("!"))
						eo.NotAllowedFlags.Add(flag);
					else
						eo.RequiredFlags.Add(flag);
				}

				var targetMatches = targetRegex.Matches(omText.Value);
				foreach (Match tm in targetMatches)
				{
					eo.Targets.Add(tm.Groups["target"].Value);
				}
				e.Options.Add(eo);
			}
			currentDatabase.Add(e);
		}

		EditorUtility.SetDirty(currentDatabase);
	}

	void DisplayBlankMainArea()
	{
		EditorGUILayout.LabelField(
			"There are 3 things that can be displayed here.\n" +
			"1) Game Event info for editing.\n" +
			"2) Utilities to parse a file.\n" +
			"3) This Blank Area.",
			GUILayout.ExpandHeight(true));
	}

	void DisplayEditMainArea()
	{
		try
		{
			currentDatabase[selectedEvent].Key = EditorGUILayout.TextField(new GUIContent("Key: "), currentDatabase[selectedEvent].Key);

			_optionsScrollPos = EditorGUILayout.BeginScrollView(_optionsScrollPos, "box", GUILayout.ExpandHeight(true));
			EditorStyles.textField.wordWrap = true;
			currentDatabase[selectedEvent].Text = EditorGUILayout.TextArea(currentDatabase[selectedEvent].Text, GUILayout.ExpandHeight(true));

			EditorGUILayout.Space();


			EditorGUILayout.PrefixLabel("Image: ");
			currentDatabase[selectedEvent].ImageId = EditorGUILayout.TextField(currentDatabase[selectedEvent].ImageId);

			EditorGUILayout.Space();
			EditorGUILayout.PrefixLabel("Sounds: ");
			var si = currentDatabase[selectedEvent].SoundIds;
			var sv = currentDatabase[selectedEvent].SoundVolumes;
			for (int i = 0; i < si.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(25)))
				{
					si.RemoveAt(i);
					EditorUtility.SetDirty(currentDatabase);
					return;
				}

				si[i] = GUILayout.TextField(si[i]);
				EditorGUILayout.EndHorizontal();
				sv[i] = EditorGUILayout.Slider("Volume", sv[i], 0, 1);
			}
			if (GUILayout.Button("+", GUILayout.Width(100)))
			{
				si.Add("");
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.PrefixLabel("Flags: ");
			var f1 = currentDatabase[selectedEvent].Flags;
			for (int i = 0; i < f1.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(25)))
				{
					f1.RemoveAt(i);
					EditorUtility.SetDirty(currentDatabase);
					return;
				}

				f1[i] = GUILayout.TextField(f1[i]);
				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("+", GUILayout.Width(100)))
			{
				currentDatabase[selectedEvent].Flags.Add("");
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.PrefixLabel("Clear Flags: ");
			var cf = currentDatabase[selectedEvent].ClearFlags;
			for (int i = 0; i < cf.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(25)))
				{
					cf.RemoveAt(i);
					EditorUtility.SetDirty(currentDatabase);
					return;
				}

				cf[i] = GUILayout.TextField(cf[i]);
				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("+", GUILayout.Width(100)))
			{
				currentDatabase[selectedEvent].ClearFlags.Add("");
			}

			EditorGUILayout.Space();
			EditorGUILayout.PrefixLabel("Options:");
			var o = currentDatabase[selectedEvent].Options;
			for (int i = 0; i < o.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("-", GUILayout.Width(25)))
				{
					o.RemoveAt(i);
					EditorUtility.SetDirty(currentDatabase);
					return;
				}

				o[i].Text = GUILayout.TextField(o[i].Text);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Targets:");
				EditorGUILayout.BeginVertical();
				var t = o[i].Targets;
				for (int j = 0; j < t.Count; j++)
				{
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("-", GUILayout.Width(25)))
					{
						t.RemoveAt(j);
						EditorUtility.SetDirty(currentDatabase);
						return;
					}
					t[j] = GUILayout.TextField(t[j]);
					if (GUILayout.Button("→", GUILayout.Width(25)))
						selectedEvent = currentDatabase.IndexOf(t[j]);
					EditorGUILayout.EndHorizontal();
				}
				if (GUILayout.Button("+", GUILayout.Width(100)))
				{
					o[i].Targets.Add("");
				}
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();


				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Required Flags:");
				EditorGUILayout.BeginVertical();
				var rf = o[i].RequiredFlags;
				for (int j = 0; j < rf.Count; j++)
				{
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("-", GUILayout.Width(25)))
					{
						rf.RemoveAt(j);
						EditorUtility.SetDirty(currentDatabase);
						return;
					}
					rf[j] = GUILayout.TextField(rf[j]);
					if (GUILayout.Button("→", GUILayout.Width(25)))
						selectedEvent = currentDatabase.IndexOf(rf[j]);
					EditorGUILayout.EndHorizontal();
				}
				if (GUILayout.Button("+", GUILayout.Width(100)))
				{
					o[i].RequiredFlags.Add("");
				}
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();


				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Not Allowed Flags:");
				EditorGUILayout.BeginVertical();
				var nf = o[i].NotAllowedFlags;
				for (int j = 0; j < nf.Count; j++)
				{
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("-", GUILayout.Width(25)))
					{
						nf.RemoveAt(j);
						EditorUtility.SetDirty(currentDatabase);
						return;
					}
					nf[j] = GUILayout.TextField(nf[j]);
					if (GUILayout.Button("→", GUILayout.Width(25)))
						selectedEvent = currentDatabase.IndexOf(nf[j]);
					EditorGUILayout.EndHorizontal();
				}
				if (GUILayout.Button("+", GUILayout.Width(100)))
				{
					o[i].NotAllowedFlags.Add("");
				}
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("+", GUILayout.Width(100)))
			{
				o.Add(new EventOption());
			}

			EditorGUILayout.EndScrollView();
		}
		catch (Exception exc)
		{
			Debug.LogError(exc, this);
			state = State.BLANK;
		}
	}
	void Verify()
	{
		HashSet<string> eventCache = new HashSet<string>();
		for (int i = 0; i < currentDatabase.Count; i++)
		{
			var e = currentDatabase[i];
			if (string.IsNullOrEmpty(e.Key))
				Debug.LogError("Event: " + i + " has no key.");

			if (string.IsNullOrEmpty(e.Text))
				Debug.LogError("Event: " + i + " : " + e.Key + " has no text.");

			if (e.Options.IsNullOrEmpty())
				Debug.LogError("Event: " + i + " : " + e.Key + " has no Options.");
			else
			{
				for (int j = 0; j < e.Options.Count; j++)
				{
					var o = e.Options[j];
					if (string.IsNullOrEmpty(o.Text))
						Debug.LogError("Event: " + i + " : " + e.Key + " Option " + j + " has no text.");
					if (o.Targets.IsNullOrEmpty())
						Debug.LogError("Event: " + i + " : " + e.Key + " Option " + j + " has no targets.");
					else
					{
						for (int k = 0; k < o.Targets.Count; k++)
						{
							var t = o.Targets[k];
							eventCache.Add(t);
							var targetEvent = currentDatabase[t];
							if (targetEvent.Key == "TheGameBroke")
								Debug.LogError("Event: " + i + " : " + e.Key + " Option " + j + " target " + k + " : " + t + " caused an access to TheGameBroke event");
						}
					}
				}
			}
		}
		for (int i = 0; i < currentDatabase.Count; i++)
		{
			var e = currentDatabase[i];
			if (eventCache.Add(e.Key))
			{
				Debug.LogError("Event: " + i + " : " + e.Key + " is never accessed.");
			}
		}
	}
}