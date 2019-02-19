using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RandomColour : MonoBehaviour, IPunObservable {

    private Color colour { get; set; }

    [Tooltip ("Select if the random colour should be totally random or chosen from a list of colours (wich will result in different colours for each user)")]
    [SerializeField] private bool randomFromList = true;

    [SerializeField] private Renderer head;
    [SerializeField] private Renderer leftHand;
    [SerializeField] private Renderer rightHand;
    [SerializeField] private Renderer torso;

    private Material headMaterial;
    private Material leftHandMaterial;
    private Material rightHandMaterial;
    private Material torsoMaterial;

    private List<Color> colours { get; set; }

    // Could not change instance material on own hands, therefore changing the material itself
    // Optimally not used, but done now as a lack of time.
    // TODO: Change to use if handMaterial and only change instance of material
    public Material handMat;

    [SerializeField] private PhotonView photonView;

    private System.Random rng = new System.Random();
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
            torsoMaterial = torso.material;
        }

        //SetColours();

        UpdateColour();
    }

    public void UpdateColour()
    {
        //generatedColour = randomFromList ? GetSemiRandomColour() : GetRandomColour();
        if (PhotonNetwork.LocalPlayer.CustomProperties["Color"] != null)
        {
            generatedColour = (Color) PhotonNetwork.LocalPlayer.CustomProperties["Color"];
        }
        else
        {
            generatedColour = GetRandomColour();
        }

        UpdateColour(generatedColour);
    }

    public void UpdateColour(Color colour)
    {
        this.colour = colour;
        UpdateColourOnMaterial(headMaterial, colour);
        
        // Update hands localy as they would change all hands when a new player logs in otherwise
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            UpdateColourOnMaterial(handMat, colour);
        }
        UpdateColourOnMaterial(torsoMaterial, colour);
    }

    public void UpdateColourOnMaterial(Material material, Color colour)
    {
        if (material)
        {
            material.color = colour;
        }
    }

    // Returns a random colour
    public static Color GetRandomColour()
    {
        float r = Random.value;
        float g = Random.value;
        float b = Random.value;

        return new Color(r, g, b);
    }

    /*
    // Returns a random colour from a set of colours
    public Color GetSemiRandomColour()
    {
        int i = rng.Next(colours.Count);
        print(colours.Count);
        print(i);
        return colours[i];
    }

    private void SetColours()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            colours = new List<Color>() { Color.blue, Color.red };//, Color.yellow, Color.green };
            //foreach (RandomColour rc in FindObjectsOfType<RandomColour>())
            //{
            //    Color occupiedColour = rc.colour;
            //    colours.Remove(occupiedColour);
            //}
            this.colours = colours;
        }
        else
        {
            foreach (RandomColour rc in FindObjectsOfType<RandomColour>())
            {
                if (rc.photonView.)
            }
        }
    }
    */

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
            UpdateColourOnMaterial(this.torsoMaterial, newColor);
        }
    }
}
