using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicPosition : MonoBehaviour {

    [SerializeField] private GameObject objectToTrack;
    public bool drag = true;
    void Update()
    {
        if (drag)
        {
            GetComponent<Rigidbody>().drag = GetComponent<Rigidbody>().mass;
            GetComponent<Rigidbody>().MovePosition(objectToTrack.transform.position);
        }
        else
        {
            GetComponent<Rigidbody>().drag = 0;
        }
    }
}
