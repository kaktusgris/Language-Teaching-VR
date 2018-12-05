using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FollowVive : MonoBehaviourPun {

    [Tooltip("Which object is this. 0 for head. 1/2 for left/right hand")]
    //public int index = 0;
    public BodyParts bodyPart;

    private bool isMine;

	// Use this for initialization
	void Start () {
        isMine = photonView.IsMine;

        // The instantiated hands are only visible for the other players
        if (bodyPart != BodyParts.head && (isMine || !PhotonNetwork.IsConnected))
        {
            transform.localScale = new Vector3(0, 0, 0);
        }
    }

	// Update is called once per frame
	void Update () {
        if (isMine || !PhotonNetwork.IsConnected)
        {
            switch (bodyPart) {
                case BodyParts.head:
                    transform.position = ViveManager.Instance.head.transform.position;
                    transform.rotation = ViveManager.Instance.head.transform.rotation;
                    break;
                case BodyParts.leftHand:
                    transform.position = ViveManager.Instance.leftHand.transform.position;
                    transform.rotation = ViveManager.Instance.leftHand.transform.rotation;
                    break;
                case BodyParts.rightHand:
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
