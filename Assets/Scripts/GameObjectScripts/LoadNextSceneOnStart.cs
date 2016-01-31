using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextSceneOnStart: MonoBehaviour
{
	[SerializeField]
	int _level = -1;
	[SerializeField]
	float _delay = 0.0f;

	AsyncOperation sceneLoadAsync;
	void Start ()
	{
		if (_level == -1)
			_level = SceneManager.GetActiveScene().buildIndex+1;
		sceneLoadAsync = SceneManager.LoadSceneAsync(_level, LoadSceneMode.Single);
		sceneLoadAsync.allowSceneActivation = false;
		Invoke("Load", _delay);
	}
	public void Load()
	{
		sceneLoadAsync.allowSceneActivation = true;
	}
}
