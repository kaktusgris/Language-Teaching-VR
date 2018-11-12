using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FollowVive : MonoBehaviourPun {

    [Tooltip("Which object is this. 0 for head. 1/2 for left/right hand")]
    //public int index = 0;
    public BodyParts bodyPart; 

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            switch (bodyPart) {
                case BodyParts.head: // head
                    transform.position = ViveManager.Instance.head.transform.position;
                    transform.rotation = ViveManager.Instance.head.transform.rotation;
                    break;
                case BodyParts.leftHand: // left hand
                    transform.position = ViveManager.Instance.leftHand.transform.position;
                    transform.rotation = ViveManager.Instance.leftHand.transform.rotation;
                    break;
                case BodyParts.rightHand: // right hand
                    transform.position = ViveManager.Instance.rightHand.transform.position;
                    transform.rotation = ViveManager.Instance.rightHand.transform.rotation;
                    break;
            }
        }
	}

    public enum BodyParts
    {
        head,
        leftHand,
        rightHand
    }
}
