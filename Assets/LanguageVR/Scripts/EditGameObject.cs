using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTNU.CarloMarton.VRLanguage
{
	public class EditGameObject : MonoBehaviour
	{
		[SerializeField]
		private List<float> customScales = new List<float>() { { 0.5f}, { 1.5f} };
		[SerializeField]
        private List<Color> customColors = new List<Color>() { { Color.red }, { Color.blue } };
		[SerializeField] [Tooltip("Will change the first material on each meshrenderer")]
		private List<MeshRenderer> meshRenderersToChange;

        private float normalisedScale;
		private List<float> scales;
		private List<Color> colors;

		private int currentVariation = 0;

		private void Start()
        {
            normalisedScale = gameObject.transform.localScale.x;
            scales = new List<float>() { { 1f } };
			colors = new List<Color>() { { meshRenderersToChange[0].material.color } };
			scales.AddRange(customScales);
			colors.AddRange(customColors);
		}

		public float GetScale()
        {
            if (currentVariation < scales.Count)
                return scales[currentVariation] * normalisedScale;
            else
                return normalisedScale;
		}

		public Color GetColor()
        {
            if (currentVariation < colors.Count)
                return colors[currentVariation];
            else
                return colors[0];
		}

		public void NextVariation()
        {
			currentVariation++;
			if (currentVariation == System.Math.Max(scales.Count, colors.Count))
				currentVariation = 0;

            transform.position += new Vector3(0, 0.1f, 0);
			transform.localScale = new Vector3(GetScale(), GetScale(), GetScale());
            foreach (MeshRenderer mr in meshRenderersToChange)
            {
                mr.material.color = GetColor();
            }
		}
	}
}
