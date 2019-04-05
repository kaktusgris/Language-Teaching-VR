using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killplane : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        MoveUp(other.gameObject, 0.2f);
    }

    private void OnTriggerExit(Collider other)
    {
        MoveUp(other.gameObject, 0.2f);
    }

    private void MoveUp(GameObject go, float yPos)
    {
        Vector3 pos = go.transform.position;

        GetRigidBody(go).velocity = Vector3.zero;
        GetRigidBody(go).angularVelocity = Vector3.zero;
        go.transform.position = new Vector3(pos.x, yPos, pos.z);
        go.transform.rotation = Quaternion.identity;
    }

    private Rigidbody GetRigidBody(GameObject go)
    {
        if (go.GetComponent<Rigidbody>() != null)
        {
            return go.GetComponent<Rigidbody>();
        }
        if (go.GetComponentInParent<Rigidbody>() != null)
        {
            return go.GetComponentInParent<Rigidbody>();
        }
        throw new MissingComponentException();
    }
}
