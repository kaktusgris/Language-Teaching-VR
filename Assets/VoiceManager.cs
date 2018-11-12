using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceManager : MonoBehaviour {

    // Use this for initialization
    void Awake () {
        var voiceObject = GameObject.FindGameObjectWithTag("Voice");
        if(voiceObject != null)
        {
            print("Delete this");
            //Destroy(gameObject);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
