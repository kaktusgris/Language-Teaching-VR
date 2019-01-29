using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Photon.Pun;

//[ExecuteInEditMode]
public class InteractableObject : MonoBehaviour
{
    [Tooltip("Click to instantiate the prefab in scene. Sometimes the old is not removed, manually delete it then")]
    [SerializeField] private bool instantiatePrefab = false;

    [Tooltip ("The word to appear over the object")]
    [SerializeField] private new string name;

    // Changing font not working (out of the box)
    //[Tooltip ("Font of name displayed in scene")]
    //[SerializeField] private Font font;

    [Tooltip ("The GameObject to be instantiated in the scene")]
    [SerializeField] private GameObject physicalObject;

    [Tooltip ("The audioclip associated with this object")]
    [SerializeField] private AudioClip audioClip;

    private GameObject instantiatedObject;
    private GameObject instantiatedTextObject;
    private TextMesh textMesh;

    private AudioSource audioSource;

    void Awake()
    {
        instantiatedObject = transform.Find(physicalObject.name + "(Clone)").gameObject;
        instantiatedTextObject = instantiatedObject.transform.Find("Text(Clone)").gameObject;

        audioSource = GetComponent<AudioSource>();
        textMesh = GetComponentInChildren<TextMesh>();
    }

    void Update()
    {
        Throwable throwable = instantiatedObject.GetComponent<Throwable>();
        if (throwable.IsAttached() && !instantiatedTextObject.activeSelf && throwable.IsMine())
        {
            instantiatedTextObject.SetActive(true);
        }
        else if (!throwable.IsAttached() && instantiatedTextObject.activeSelf)
        {
            instantiatedTextObject.SetActive(false);
        }

        if (instantiatedTextObject.activeSelf)
        {
            Transform headTransform = ViveManager.Instance.head.transform;
            instantiatedTextObject.transform.rotation = Quaternion.LookRotation(instantiatedTextObject.transform.position - headTransform.position);
        }
    }

    private void OnValidate()
    {
        if (instantiatePrefab)
        {
            InstantiatePhysicalObject();
            InstantiateText();
            instantiatePrefab = false;
        }
    }

    // Instansiates the given gameobject
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
        Throwable throwable = instantiatedObject.AddComponent<Throwable>();
        instantiatedObject.AddComponent<Interactable>();
        instantiatedObject.AddComponent<VelocityEstimator>();
        PhotonView photonView = instantiatedObject.AddComponent<PhotonView>();

        photonView.ObservedComponents.Add(throwable);
        photonView.OwnershipTransfer = OwnershipOption.Takeover;
        photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
    }

    private void InstantiateText()
    {
        instantiatedTextObject = new GameObject("Text");
        instantiatedTextObject.transform.parent = instantiatedObject.transform;
        instantiatedObject.transform.localPosition = Vector3.zero;

        textMesh = instantiatedTextObject.AddComponent<TextMesh>();

        textMesh.text = name;
        textMesh.transform.localPosition = Vector3.zero;
        textMesh.transform.localScale = Vector3.one * 0.01f;
        textMesh.fontSize = 50;
        textMesh.anchor = TextAnchor.MiddleCenter;
        //textMesh.font = font;
    }

    // Can only destroy gameobjects in OnValidate using a coroutine
    IEnumerator Destroy(GameObject go)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(go);
    }
}
