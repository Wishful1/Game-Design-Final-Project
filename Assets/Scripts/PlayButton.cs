using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayButton:MonoBehaviour
{
	public void ChangeScene(string sceneName)
	{
		Application.LoadLevel(sceneName);
	}
}