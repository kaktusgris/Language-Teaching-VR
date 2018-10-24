using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomColour : MonoBehaviour {

    [SerializeField] private bool selectColourManually;
    [SerializeField] private Color colour;

	// Use this for initialization
	void Start () {
        gameObject.GetComponent<Renderer>().material.color = generateColour();
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
