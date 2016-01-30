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

	public void Add(GameEvent e)
	{
		database.Add(e);
	}

	public void Remove(GameEvent e)
	{
		database.Remove(e);
	}

	public void RemoveAt(int index)
	{
		database.RemoveAt(index);
	}

	public int Count
	{
		get { return database.Count; }
	}

	public GameEvent this[string key]
	{
		get
		{
			var events = database.Where(l => l.Key == key);
			if (events == null)
				Debug.LogError("There is no event with key [" + key + "]");
			var list = events.ToList();
			return list[Random.Range(0, list.Count)];
		}
	}
}