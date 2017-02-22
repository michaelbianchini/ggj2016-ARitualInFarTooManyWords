using UnityEngine;
using UnityEngine.Audio;

public class MuteButton : MonoBehaviour
{
	[SerializeField]
	private AudioMixerSnapshot _normalSnapshot;
	[SerializeField]
	private AudioMixerSnapshot _muteSnapshot;
	private bool _mute;
	public bool Mute
	{
		get { return _mute; }
		set
		{
			_mute = value;
			if (_mute)
				_muteSnapshot.TransitionTo(0.5f);
			else
				_normalSnapshot.TransitionTo(0.5f);
		}
	}
}
