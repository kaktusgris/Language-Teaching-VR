using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDictionary : MonoBehaviour {

    public InGameMenu inGameMenu;

    public List<string> wordsList;
    private List<GameObject> worldObjects;

	// Use this for initialization
	void Start () {
        wordsList = new List<string>();
        worldObjects = new List<GameObject>();
        print("Dictionary initialized");
        printLists();
	}

    public void addItemToDictionary(string word, GameObject item)
    {
        printLists();
        if (wordsList.Count == 0 || !wordsList.Contains(word))
        {
            print("inside");
            wordsList.Add(word);

            worldObjects.Add(item);

            inGameMenu.AddTextBlock(word);
        }
    }

    public void printLists()
    {
        for (int i = 0; i < wordsList.Count; i++)
        {
            print(wordsList[i] + ", " + worldObjects[i]);
        }
    }

    public GameObject getInteractable(string word)
    {
        if (!wordsList.Contains(word))
        {
            return null;
        }

        int index = wordsList.IndexOf(word);
        return worldObjects[index];
    }

    public void removeItem(string word)
    {
        if (!wordsList.Contains(word))
        {
            print("That item has not been added to the dictionary.");
            return;
        }

        int index = wordsList.IndexOf(word);
        wordsList.RemoveAt(index);
        worldObjects.RemoveAt(index);
    }

    public bool isAdded(string word)
    {
        return wordsList.Contains(word);
    }

}
