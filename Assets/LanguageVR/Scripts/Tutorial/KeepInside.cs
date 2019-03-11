using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepInside : MonoBehaviour
{
    public GameObject[] objectsToKeepInside;
    private Dictionary<GameObject, Vector3> positions = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Quaternion> rotations = new Dictionary<GameObject, Quaternion>();

    private void Start()
    {
        foreach (GameObject go in objectsToKeepInside)
        {
            positions.Add(go, go.transform.position);
            rotations.Add(go, go.transform.rotation);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        foreach (GameObject go in objectsToKeepInside)
        {
            if (go == other.gameObject)
            {
                ResetTransformOnGameObject(other.transform, go);
                break;
            }
        }
    }

    private void ResetTransformOnGameObject(Transform transformToReset, GameObject gameObject)
    {
        Vector3 originalPosition = positions[gameObject];
        Quaternion originalRotation = rotations[gameObject];

        transformToReset.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        transformToReset.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        transformToReset.position = originalPosition;
        transformToReset.rotation = originalRotation;
    }
}
