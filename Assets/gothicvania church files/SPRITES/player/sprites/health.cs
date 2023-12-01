using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class health : MonoBehaviour
{
    // Start is called before the first frame update
	public int numHealth = 3;
	public int maxHearts = 3;
	
	public Image[] hearts;
	public Sprite fullHeart;
	public Sprite emptyHeart;
	
    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < hearts.Length; i++)
		{
			if (i < numHealth)
			{
				hearts[i].sprite = fullHeart;
			}
			else
			{
				hearts[i].sprite = emptyHeart;
			}
		}
    }
	public void setHealth(int input)
	{
		numHealth = input;
	}
}
