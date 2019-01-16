using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Photon.Pun;

[ExecuteInEditMode]
public class InteractableObject : MonoBehaviour
{
    [Tooltip("Click to instantiate the prefab in scene. Sometimes the old is not removed, manually delete it then")]
    [SerializeField] private bool instantiatePrefab = false;

    [Tooltip ("The word to appear over the object")]
    [SerializeField] private new string name;

    [Tooltip ("The GameObject to be instantiated in the scene")]
    [SerializeField] private GameObject physicalObject;

    [Tooltip ("The audioclip associated with this object")]
    [SerializeField] private AudioClip audioClip;

    GameObject instantiatedObject;

    AudioSource audio;

    //public SteamVR_Input_Sources handType; // 1
    //public SteamVR_Action_Boolean menuButtonAction; // 2

    void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        //if (menuButtonAction.GetStateDown(SteamVR_Input_Sources.Any))
        //{
        //    audio.clip = audioClip;
        //    audio.Play();
        //}
    }

    private void OnValidate()
    {
        if (instantiatePrefab)
        {
            InstantiatePhysicalObject();
            instantiatePrefab = false;
        }
    }

    // Updates the instansiation of a gameobject
    private void InstantiatePhysicalObject()
    {
        // Destroy previous object if a new one is selected
        if (instantiatedObject != null)
        {
            StartCoroutine(Destroy(instantiatedObject));
        }

        instantiatedObject = Instantiate(physicalObject);
        instantiatedObject.transform.parent = this.transform;
        instantiatedObject.transform.localPosition = Vector3.zero;
        instantiatedObject.GetComponent<MeshCollider>().enabled = false;

        instantiatedObject.AddComponent<Rigidbody>();
        instantiatedObject.AddComponent<BoxCollider>();
        instantiatedObject.AddComponent<Throwable>();
        instantiatedObject.AddComponent<Interactable>();
        instantiatedObject.AddComponent<VelocityEstimator>();
        instantiatedObject.AddComponent<PhotonView>();

        PhotonView photonView = instantiatedObject.GetComponent<PhotonView>();
        Throwable throwable = instantiatedObject.GetComponent<Throwable>();

        photonView.ObservedComponents.Add(throwable);
        photonView.OwnershipTransfer = OwnershipOption.Takeover;
        photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
    }

    // Can only destroy gameobjects in OnValidate using a coroutine
    IEnumerator Destroy(GameObject go)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(go);
    }
}
