using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminMode : MonoBehaviour
{
    [SerializeField]
    private PlayerDictionary playerDictionary;

    private bool isAdmin;

    void Start()
    {
        isAdmin = (bool) PhotonNetwork.LocalPlayer.CustomProperties["admin"];

        if (isAdmin)
        {
            FillDictionary();
        }
    }


    private void FillDictionary()
    {
        print("Filling dictionary with all interactable objects from Resources/InteractableObjects");
        Object[] objects = Resources.LoadAll("InteractableObjects");
        
        foreach (Object ob in objects)
        {
            GameObject item = (GameObject) ob;
            string name = item.GetComponentInChildren<TextMesh>().text;
            playerDictionary.AddItemToDictionary(name, Instantiate(item));
        }
    }
}
