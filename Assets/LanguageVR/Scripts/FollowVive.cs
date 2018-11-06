using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FollowVive : MonoBehaviourPun {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (photonView.IsMine)
        {
            transform.position = ViveManager.Instance.head.transform.position;
            transform.rotation = ViveManager.Instance.head.transform.rotation;
        }
	}
}
