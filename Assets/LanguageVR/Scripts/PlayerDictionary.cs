using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDictionary : MonoBehaviour {

    public InGameMenu inGameMenu;

    private Dictionary<string, GameObject> wordDictionary;

	// Use this for initialization
	void Start () {
        wordDictionary = new Dictionary<string, GameObject>();
        print("Dictionary initialized");
	}

    public void addItemToDictionary(string word, GameObject item)
    {
        if (!isAdded(word))
        {
            wordDictionary.Add(word, item);
            inGameMenu.AddTextBlock(word);
        }
    }

    public void printDictionary()
    {
        foreach (string key in wordDictionary.Keys)
        {
            print(key + ", " + wordDictionary[key]);
        }
    }

    public GameObject getInteractable(string word)
    {
        if (!isAdded(word))
        {
            return null;
        }
        return wordDictionary[word];
    }

    public void removeItem(string word)
    {
        if (!isAdded(word))
        {
            print("That item has not been added to the dictionary.");
            return;
        }
        wordDictionary.Remove(word);
    }

    public bool isAdded(string word)
    {
        return wordDictionary.ContainsKey(word);
    }

}
