using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Photon.Pun;
using UnityEditor;

//[ExecuteInEditMode]
public class InteractableObjectInitialiser : MonoBehaviour
{
    [Tooltip("Click to instantiate the prefab in scene. Sometimes the old is not removed, manually delete it then")]
    [SerializeField] private bool instantiatePrefab = false;

    [Tooltip("The word to appear over the object")]
    [SerializeField] private new string name;

    // Changing font not working (out of the box)
    //[Tooltip ("Font of name displayed in scene")]
    //[SerializeField] private Font font;

    [Tooltip("The GameObject to be instantiated in the scene")]
    [SerializeField] private GameObject physicalObject;

    [Tooltip("The audioclip associated with this object")]
    [SerializeField] private AudioClip audioClip;

    private GameObject instantiatedObject;
    private GameObject instantiatedTextObject;

    void Awake()
    {
        instantiatedObject = transform.Find(physicalObject.name + "(Clone)").gameObject;
        instantiatedTextObject = instantiatedObject.transform.Find("Text").gameObject;
    }

    

    private void OnValidate()
    {
        if (instantiatePrefab)
        {
            InstantiatePhysicalObject();
            InstantiateText();
            SaveToResources();
            instantiatePrefab = false;
        }
    }

    // Instansiates the given gameobject
    private void InstantiatePhysicalObject()
    {

        // Destroy previous object if a new one is selected
        if (transform.childCount > 0)
        {
            StartCoroutine(Destroy(transform.GetChild(0).gameObject));
        }

        instantiatedObject = Instantiate(physicalObject);
        instantiatedObject.transform.parent = this.transform;
        instantiatedObject.transform.localPosition = Vector3.zero;
        instantiatedObject.AddComponent<Rigidbody>();

        int numberOfChildren = instantiatedObject.transform.childCount;
        if (numberOfChildren == 0)
        {
            if (!instantiatedObject.GetComponent<MeshCollider>())
            {
                instantiatedObject.AddComponent<MeshCollider>();
            }
            instantiatedObject.GetComponent<MeshCollider>().convex = true;
        }
        else
        {
            for (int i = 0; i < numberOfChildren; i++)
            {
                GameObject child = instantiatedObject.transform.GetChild(i).gameObject;
                if (!child.GetComponent<MeshCollider>())
                {
                    child.AddComponent<MeshCollider>();
                }
                child.GetComponent<MeshCollider>().convex = true;
            }
        }

        Throwable throwable = instantiatedObject.AddComponent<Throwable>();
        //instantiatedObject.AddComponent<Interactable>(); // Comes with Throwable
        instantiatedObject.AddComponent<VelocityEstimator>();
        PhotonView photonView = instantiatedObject.AddComponent<PhotonView>();

        photonView.ObservedComponents = new List<Component>();
        photonView.ObservedComponents.Add(throwable);
        photonView.OwnershipTransfer = OwnershipOption.Takeover;
        photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
    }

    private void InstantiateText()
    {
        instantiatedTextObject = new GameObject("Text");
        instantiatedTextObject.transform.parent = instantiatedObject.transform;
        instantiatedObject.transform.localPosition = Vector3.zero;

        InteractableObjectText iot = instantiatedTextObject.AddComponent<InteractableObjectText>();
        iot.SetAudioClip(audioClip);

        TextMesh textMesh = instantiatedTextObject.AddComponent<TextMesh>();
        textMesh.text = name;
        textMesh.transform.localPosition = Vector3.zero;
        textMesh.transform.localScale = Vector3.one * 0.01f;
        textMesh.fontSize = 100;
        textMesh.anchor = TextAnchor.MiddleCenter;
        //textMesh.font = font;
    }

    private void SaveToResources()
    {
        Debug.LogError("Cannot create prefab of interactable object."); // A bug in unity 2018.3.5
        //PrefabUtility.SaveAsPrefabAsset(instantiatedObject, "Assets/LanguageVR/Resources/InteractableObjects/" + textMesh.text + ".prefab");
    }

    // Can only destroy gameobjects in OnValidate using a coroutine
    IEnumerator Destroy(GameObject go)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(go);
    }
}