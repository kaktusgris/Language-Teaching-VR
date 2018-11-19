using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideIfMine : MonoBehaviour {


    public GameObject photonViewToHide;

	// Use this for initialization
	void Start () {
		if (photonViewToHide.GetComponent<PhotonView>().IsMine)
        {
            photonViewToHide.SetActive(false);
        }
	}
}
