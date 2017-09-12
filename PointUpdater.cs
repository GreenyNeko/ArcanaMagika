using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointUpdater : MonoBehaviour {

    private int level;
    private int points;
	// Use this for initialization
	void Start ()
    {
        level = 1;
        points = 0;
	}
	
	// Update is called once per frame
	void Update ()
    {
        GetComponent<Text>().text = "Level: " + level + "\nPoints: " + formatPoints(points);
	}

    string formatPoints(int points)
    {
        string finalStr = "";
        for(int i = 0; i < 9 - points.ToString().Length; i++)
        {
            finalStr += "0";
        }
        finalStr += points.ToString();
        return finalStr;
    }

    public void addPoints(int amount)
    {
        points += amount;
        level = points / 6330 + 1;
    }

    public int getLevel()
    {
        return level;
    }
}
