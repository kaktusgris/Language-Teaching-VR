using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Photon.Pun;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NTNU.CarloMarton.VRLanguage
{
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

        private void OnValidate()
        {
            if (instantiatePrefab)
            {
                GameObject go = InstantiatePhysicalObject();
                if (go != null)
                {
                    go = InstantiateText(go);

                    go.tag = "InteractableObject";
                    ChangeLayersRecursively(go.transform, "DeletableObjects");

                    SaveToResources(go);
                }
                instantiatePrefab = false;
            }
        }

        // Instansiates the given gameobject
        private GameObject InstantiatePhysicalObject()
        {
#if UNITY_EDITOR
            // Destroy previous object if a new one is selected
            if (transform.childCount > 0)
            {
                if (EditorUtility.DisplayDialog("Override existing interactable object?", "There already exists a " + transform.GetChild(0).name + " Are you sure you want to override it?", "Override", "Cancel"))
                {
                    StartCoroutine(Destroy(transform.GetChild(0).gameObject));
                }
                else
                {
                    return null;
                }
            }
#endif

            GameObject instantiatedObject = Instantiate(physicalObject);
            instantiatedObject.name = name;
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

            ThrowableObject throwable = instantiatedObject.AddComponent<ThrowableObject>();
            //instantiatedObject.AddComponent<Interactable>(); // Comes with Throwable
            //instantiatedObject.AddComponent<VelocityEstimator>(); // Comes with throwable
            PhotonView photonView = instantiatedObject.AddComponent<PhotonView>();

            photonView.ObservedComponents = new List<Component>();
            photonView.ObservedComponents.Add(throwable);
            photonView.OwnershipTransfer = OwnershipOption.Takeover;
            photonView.Synchronization = ViewSynchronization.UnreliableOnChange;

            return instantiatedObject;
        }

        private GameObject InstantiateText(GameObject instantiatedObject)
        {
            GameObject instantiatedTextObject = new GameObject("Text");
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

            return instantiatedObject;
        }

        private void ChangeLayersRecursively(Transform trans, string layerName)
        {
            trans.gameObject.layer = LayerMask.NameToLayer(layerName);
            foreach (Transform child in trans)
            {
                ChangeLayersRecursively(child, layerName);
            }
        }

        private void SaveToResources(GameObject instantiatedObject)
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
}