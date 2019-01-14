using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceRecognitionScript : MonoBehaviour {

    private VR_VoiceRecognition.VoiceRecognition voiceRecognition = new VR_VoiceRecognition.VoiceRecognition();
    private Valve.VR.InteractionSystem.Throwable throwableComponent;
    private bool voiceRecognitionRunning = false;

    // Use this for initialization
    void Start () {
        throwableComponent = GetComponent<Valve.VR.InteractionSystem.Throwable>();

    }
	
	// Update is called once per frame
	void Update () {
        if (!voiceRecognitionRunning && throwableComponent.getAttached())
        {
            Debug.Log("Starting voice...");
            voiceRecognitionRunning = true;
            StartVoiceRecognition();
        }
	}

    private void StartVoiceRecognition()
    {
        if (voiceRecognition.StartSpeechRecognition())
        {
            Debug.Log("VoiceRecognition: Success!");
        } else
        {
            Debug.Log("VoiceRecognition: Failure!");
        }
        Debug.Log("Shutting down voice...");

        voiceRecognitionRunning = false;
    }
}
