using UnityEngine;
using System.Collections;

public class CursorAnimator : MonoBehaviour {

	[SerializeField]
	Texture2D[] _cursorFrames;
	[SerializeField]
	float _speed = 1.0f;
	float _lastUpdate;
	int _index = 0;
	void Start()
	{
		_lastUpdate = Time.realtimeSinceStartup;
		Cursor.SetCursor(_cursorFrames[_index], Vector2.zero, CursorMode.Auto);
	}
	void Update ()
	{	
		if(Time.realtimeSinceStartup - _lastUpdate > _speed)
		{
			_index++;
			if (_index >= _cursorFrames.Length)
				_index = 0;
			Cursor.SetCursor(_cursorFrames[_index], Vector2.zero, CursorMode.Auto);
			_lastUpdate = Time.realtimeSinceStartup;
		}
	}
}
