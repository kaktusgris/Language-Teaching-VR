using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTNU.CarloMarton.VRLanguage
{
	public class EditGameObject : MonoBehaviour
	{
		[SerializeField]
		private List<float> customSizes;
		[SerializeField]
		private List<Color> customColors;
		[SerializeField]
		private MeshRenderer materialToChange;

		private List<float> sizes;
		private List<Color> colors;

		private int currentVariation = 0;

		private void Start() {
			sizes = new List<float>() { { gameObject.transform.localScale.x } };
			colors = new List<Color>() { { materialToChange.material.color } };
			sizes.AddRange(customSizes);
			colors.AddRange(customColors);
		}

		public float GetSize() {
			return sizes[currentVariation];
		}

		public Color GetColor() {
			return colors[currentVariation];
		}

		public void NextVariation() {
			currentVariation++;
			if (currentVariation == sizes.Count)
				currentVariation = 0;

			transform.localScale = new Vector3(GetSize(), GetSize(), GetSize());
			materialToChange.material.color = GetColor();
		}
	}
}
