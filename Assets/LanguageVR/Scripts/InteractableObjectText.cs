using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class InteractableObjectText : MonoBehaviour
{
    [SerializeField]
    private AudioClip audioClip;

    private Throwable throwable;
    private MeshRenderer textRenderer;

    private void Start()
    {
        throwable = gameObject.GetComponentInParent<Throwable>();
        textRenderer = gameObject.GetComponent<MeshRenderer>();
    }

    void Update()
    {
        // Only shows the text if the user is picking up the object
        if (throwable.IsAttached() && !textRenderer.enabled && throwable.IsMine())
        {
            textRenderer.enabled = true;
        }
        else if (!throwable.IsAttached() && textRenderer.enabled && throwable.IsMine())
        {
            textRenderer.enabled = false;
        }

        // Rotate text to be seen by the user if it is visible
        if (textRenderer.enabled)
        {
            Transform headTransform = ViveManager.Instance.head.transform;
            gameObject.transform.rotation = Quaternion.LookRotation(gameObject.transform.position - headTransform.position);
        }
    }

    public void SetAudioClip(AudioClip audioClip)
    {
        this.audioClip = audioClip;
    }

    public AudioClip GetAudioClip()
    {
        return audioClip;
    }
}
