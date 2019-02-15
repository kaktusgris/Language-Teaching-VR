using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDictionary : MonoBehaviour {

    public InGameMenu inGameMenu;

    [SerializeField]
    private AudioSource audioSource;

    private Dictionary<string, GameObject> wordDictionary;

	// Use this for initialization
	void Start () {
        wordDictionary = new Dictionary<string, GameObject>();
        print("Dictionary initialized");
	}

    public bool AddItemToDictionary(string word, GameObject item)
    {
        if (!IsAdded(word))
        {
            wordDictionary.Add(word, item);
            inGameMenu.AddTextBlock(word);
            return true;
        }
        return false;
    }

    public void PrintDictionary()
    {
        foreach (string key in wordDictionary.Keys)
        {
            print(key + ", " + wordDictionary[key]);
        }
    }

    public GameObject GetInteractable(string word)
    {
        if (!IsAdded(word))
        {
            return null;
        }
        return wordDictionary[word];
    }

    public void RemoveItem(string word)
    {
        if (!IsAdded(word))
        {
            print("That item has not been added to the dictionary.");
            return;
        }
        wordDictionary.Remove(word);
    }

    public bool IsAdded(string word)
    {
        return wordDictionary.ContainsKey(word);
    }

    public void PlayAudio(string word)
    {
        
        audioSource.clip = GetInteractable(word).GetComponentInChildren<InteractableObjectText>().GetAudioClip();
        if (audioSource.clip != null)
        {
            print("Playing audio from " + word);
            audioSource.Play();
        }
        else
        {
            Debug.LogFormat("Tried to play audio from {0}, but id did not have an audio clip", word);
        }
    }
}
