using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    int storedHealth = 3;
	public bool hasKey = false;
	public int prevScene = 1;
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
	public void saveHealth(int health)
	{
		storedHealth = health;
	}
	public int loadHealth()
	{
		return storedHealth;
	}
	public void setKey()
	{
		hasKey = true;
	}
	public bool checkKey()
	{
		return hasKey;
	}
	public void setPrevScene(int scene)
	{
		prevScene = scene;
	}
	public int getPrevScene()
	{
		return prevScene;
	}
}
