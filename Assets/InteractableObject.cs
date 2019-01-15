using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InteractableObject : MonoBehaviour
{
    [SerializeField] private bool updatePrefab = false;

    [SerializeField] private new string name;
    [SerializeField] private GameObject physicalObject;
    [SerializeField] private AudioClip audioClip;

    GameObject instantiatedObject;

    private void OnValidate()
    {
        if (updatePrefab)
        {
            UpdateInstantiation();
            updatePrefab = false;
        }
    }

    // Updates the instansiation of a gameobject
    private void UpdateInstantiation()
    {
        if (instantiatedObject != null)
        {
            StartCoroutine(Destroy(instantiatedObject));
        }

        instantiatedObject = Instantiate(physicalObject);
        instantiatedObject.transform.parent = this.transform;
        instantiatedObject.transform.localPosition = Vector3.zero;

        instantiatedObject.AddComponent<Rigidbody>();
        instantiatedObject.AddComponent<Valve.VR.InteractionSystem.Throwable>();
        instantiatedObject.AddComponent<Valve.VR.InteractionSystem.Interactable>();
        instantiatedObject.AddComponent<Valve.VR.InteractionSystem.VelocityEstimator>();
        instantiatedObject.AddComponent<Photon.Pun.PhotonView>();

        Photon.Pun.PhotonView pw = instantiatedObject.GetComponent<Photon.Pun.PhotonView>();
        Valve.VR.InteractionSystem.Throwable th = instantiatedObject.GetComponent<Valve.VR.InteractionSystem.Throwable>();
        //StartCoroutine(AddThrowableToPhotonViewsObservedComponents(pw, th));
        pw.ObservedComponents.Add(th);
    }

    // Can only destroy gameobjects in OnValidate using a coroutine
    IEnumerator Destroy(GameObject go)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(go);
    }

    IEnumerator AddThrowableToPhotonViewsObservedComponents(Photon.Pun.PhotonView pw, Valve.VR.InteractionSystem.Throwable th)
    {
        print("started");
        yield return new WaitForEndOfFrame();
        print("done");
        pw.ObservedComponents.Add(th);
    }

    void Awake()
    {
        
    }

    void Update()
    {
        
    }
}
