using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
//using Photon;

public class RandomColour : MonoBehaviour, IPunObservable {

    [SerializeField] private bool selectColourManually;
    [SerializeField] private Color colour;
    [SerializeField] private Material otherMaterialToChange;
    [SerializeField] private PhotonView photonView;

    private Color generatedColour;

	// Use this for initialization
	void Start () {
        
    }

    public void newColour()
    {
        generatedColour = generateColour();
        newColour(generatedColour);
    }

    public void newColour(Color colour)
    {
        gameObject.GetComponent<Renderer>().material.color = colour;
        if (otherMaterialToChange)
        {
            print("ChangedColour");
            otherMaterialToChange.SetColor("_Color", colour);
        }
        else
        {
            Debug.LogWarning("No material to change colour on head");
        }
    }

    private Color generateColour()
    {
        if (selectColourManually)
        {
            return colour;
        } else
        {
            return getRandomColour();
        }
    }

    // Returns a random colour
    public Color getRandomColour()
    {
        float r = Random.value;
        float g = Random.value;
        float b = Random.value;
        print(r + ", " + g + ", " + b);
        return new Color(r, g, b);
    }

    public static string colorToString(Color color)
    {
        return color.r + "," + color.g + "," + color.b + "," + color.a;
    }
    public static Color stringToColor(string colorString)
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(colorToString(generatedColour));
        } else
        {
            newColour(stringToColor((string)stream.ReceiveNext()));
        }
    }
}
