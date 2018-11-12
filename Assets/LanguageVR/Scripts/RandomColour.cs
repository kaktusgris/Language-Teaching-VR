﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomColour : MonoBehaviour {

    [SerializeField] private bool selectColourManually;
    [SerializeField] private Color colour;
    [SerializeField] private Material otherMaterialToChange;

	// Use this for initialization
	void Start () {
        
    }

    public void newColour()
    {
        Color newColour = generateColour();
        gameObject.GetComponent<Renderer>().material.color = newColour;
        if (otherMaterialToChange)
        {
            print("ChangedColour");
            otherMaterialToChange.SetColor("_Color", newColour);
        } else
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
}
