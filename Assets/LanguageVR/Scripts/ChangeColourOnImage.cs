using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeColourOnImage : MonoBehaviour
{
    public RandomColour randomColour;
    public Image image;

    void Update()
    {
        //image.color = ConvertFloatToColor(PhotonNetwork.LocalPlayer);
    }

    private Color ConvertFloatToColor(Player p)
    {
        float[] colorAsFloat = (float[])p.CustomProperties["Color"];
        if (colorAsFloat == null)
        {
            return Color.black;
        }

        float r = colorAsFloat[0];
        float g = colorAsFloat[1];
        float b = colorAsFloat[2];
        float a = colorAsFloat[3];

        return new Color(r, g, b, a);
    }
}
