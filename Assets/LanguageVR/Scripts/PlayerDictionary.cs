using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDictionary : MonoBehaviour {

    private List<string> wordsList;
    private List<GameObject> worldObjects;

	// Use this for initialization
	void Start () {
        wordsList = new List<string>();
        worldObjects = new List<GameObject>();
        print("Dictionary initialized");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void addItemToDictionary(string word, GameObject item)
    {
        if (!wordsList.Contains(word))
        {
            wordsList.Add(word);

            worldObjects.Add(item);            
        }
    }

    public void printLists()
    {
        for (int i = 0; i < wordsList.Count; i++)
        {
            print(wordsList[i] + ", " + worldObjects[i]);
        }
    }

}
