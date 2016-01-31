using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName ="GameEventDatabase", menuName = "GameEventDatabase")]
public class GameEventDatabase : ScriptableObject
{
	[SerializeField]
	private List<GameEvent> database;

	void OnEnable()
	{
		if (database == null)
			database = new List<GameEvent>();
	}

	public int Add(GameEvent e)
	{
		database.Add(e);
		return database.Count - 1;
	}

	public void Remove(GameEvent e)
	{
		database.Remove(e);
	}

	public void RemoveAt(int index)
	{
		database.RemoveAt(index);
	}
	public void Clear()
	{
		database.Clear();
	}

	public int Count
	{
		get { return database.Count; }
	}

	public int IndexOf(string key)
	{
		return database.IndexOf(this[key]);
	}

	public GameEvent this[int key]
	{
		get
		{
			return database[key];
		}
		set
		{
			database[key] = value;
		}
	}

	public GameEvent this[string key]
	{
		get
		{
			var events = database.Where(l => l.Key == key);
			if (events.IsNullOrEmpty())
			{
				Debug.LogError("There is no event with key [" + key + "]");
				return this["TheGameBroke"];
			}
			var list = events.ToList();
			return list[Random.Range(0, list.Count)];
		}
	}
}