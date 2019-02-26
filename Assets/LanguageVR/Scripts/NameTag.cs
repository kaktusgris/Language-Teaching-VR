using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTNU.CarloMarton.VRLanguage
{
    public class NameTag : MonoBehaviour, IPunObservable
    {
        private string name;

        private TextMesh textMesh;

        private void Awake()
        {
            textMesh = GetComponent<TextMesh>();
            name = PhotonNetwork.NickName;
            textMesh.text = name;
        }

        private void Update()
        {
            Transform eyes = ViveManager.Instance.head.transform;
            textMesh.transform.LookAt(eyes);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(name);
            }
            else
            {
                name = (string) stream.ReceiveNext();
                textMesh.text = name;
            }
        }
    }
}
