using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepInside : MonoBehaviour
{
    public GameObject[] objectsToKeepInside;
    private Dictionary<GameObject, Vector3> positions = new Dictionary<GameObject, Vector3>();

    private void Start()
    {
        foreach (GameObject go in objectsToKeepInside)
        {
            positions.Add(go, go.transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        foreach (GameObject go in objectsToKeepInside)
        {
            if (go == other.gameObject)
            {
                ResetTransformOnGameObject(other.transform, positions[go]);
                break;
            }
        }
    }

    private void ResetTransformOnGameObject(Transform transformToReset, Vector3 originalPosition)
    {
        transformToReset.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        transformToReset.position = originalPosition;
    }
}
