using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RandomColour : MonoBehaviour, IPunObservable {

    [SerializeField] private bool selectColourManually;
    [SerializeField] private Color colour;
    //[SerializeField] private Material otherMaterialToChange;

    [SerializeField] private Renderer head;
    [SerializeField] private Renderer leftHand;
    [SerializeField] private Renderer rightHand;
    [SerializeField] private Renderer torso;

    private Material headMaterial;
    private Material leftHandMaterial;
    private Material rightHandMaterial;
    private Material bodyMaterial;

    // Could not change instance material on own hands, therefore changing the material itself
    // Optimally not used, but done now as a lack of time.
    // TODO: Change to use if handMaterial and only change instance of material
    public Material handMat;

    [SerializeField] private PhotonView photonView;

    private Color generatedColour;

    public void Awake()
    {
        if (head)
        {
            // Using the robot head with four materials
            headMaterial = head.materials[1];
        }
        if (leftHand)
        {
            leftHandMaterial = leftHand.material;
        }
        if (rightHand)
        {
            rightHandMaterial = rightHand.material;
        }
        if (torso)
        {
            bodyMaterial = torso.material;
        }

        UpdateColour();
    }

    public void UpdateColour()
    {
        generatedColour = GenerateColour();
        UpdateColour(generatedColour);
    }

    public void UpdateColour(Color colour)
    {
        UpdateColourOnMaterial(headMaterial, colour);
        
        // Update hands localy as they would change all hands when a new player logs in otherwise
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            UpdateColourOnMaterial(handMat, colour);
        }
        UpdateColourOnMaterial(bodyMaterial, colour);
    }

    public void UpdateColourOnMaterial(Material material, Color colour)
    {
        if (material)
        {
            material.color = colour;
        }
    }

    private Color GenerateColour()
    {
        if (selectColourManually)
        {
            return colour;
        } else
        {
            return GetRandomColour();
        }
    }

    // Returns a random colour
    public Color GetRandomColour()
    {
        float r = Random.value;
        float g = Random.value;
        float b = Random.value;

        return new Color(r, g, b);
    }

    public static string ColorToString(Color color)
    {
        return color.r + "," + color.g + "," + color.b + "," + color.a;
    }

    public static Color StringToColor(string colorString)
    {
        try
        {
            string[] colors = colorString.Split(',');
            return new Color(float.Parse(colors[0]), float.Parse(colors[1]), float.Parse(colors[2]), float.Parse(colors[3]));
        }
        catch
        {
            return Color.white;
        }
    }

    // Updates the head's colour to other users
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ColorToString(generatedColour));
        }
        else
        {
            Color newColor = StringToColor((string)stream.ReceiveNext());
            UpdateColourOnMaterial(this.headMaterial, newColor);
            UpdateColourOnMaterial(this.leftHandMaterial, newColor);
            UpdateColourOnMaterial(this.rightHandMaterial, newColor);
        }
    }
}
