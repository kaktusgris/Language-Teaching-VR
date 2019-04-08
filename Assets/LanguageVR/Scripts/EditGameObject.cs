using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTNU.CarloMarton.VRLanguage
{
	public class EditGameObject : MonoBehaviour, IPunObservable
	{
		[SerializeField]
		private List<float> customScales = new List<float>() { { 0.5f}, { 1.5f} };
		[SerializeField]
        private List<Color> customColors = new List<Color>() { { Color.red }, { Color.blue } };
		[SerializeField] [Tooltip("Will change the first material on each meshrenderer")]
		private List<MeshRenderer> meshRenderersToChange;

        private Vector3 normalisedScale;
		private List<float> scales;
		private List<Color> colors;

		private int currentVariation = 0;

		private void Start()
        {
            normalisedScale = GetDefaultScale();
            scales = new List<float>() { { 1f } };
			colors = new List<Color>() { { GetDefaultColor() } };
			scales.AddRange(customScales);
			colors.AddRange(customColors);
		}

		public Vector3 GetScale()
        {
            print(scales[0]);
            print(normalisedScale);
            print(scales[0] * normalisedScale);
            if (currentVariation < scales.Count)
                return scales[currentVariation] * normalisedScale;
            else
                return scales[0] * normalisedScale;
		}

		public Color GetColor()
        {
            if (currentVariation < colors.Count)
                return colors[currentVariation];
            else
                return colors[0];
		}

        public int GetVariation()
        {
            return currentVariation;
        }

        // Must be able to run before Start
        public void LoadVariation(int newVariation)
        {
            if (newVariation < System.Math.Max(customScales.Count, customColors.Count) + 1)
                currentVariation = newVariation;

            transform.position += new Vector3(0, 0.1f, 0);

            Vector3 scale;
            Color color;

            if (currentVariation == 0)
            {
                scale = GetDefaultScale();
                color = GetDefaultColor();
            }
            else
            {
                if (currentVariation - 1 < customScales.Count)
                    scale = GetDefaultScale() * customScales[currentVariation - 1];
                else
                    scale = GetDefaultScale();
                if (currentVariation - 1 < customColors.Count)
                    color = customColors[currentVariation - 1];
                else
                    color = GetDefaultColor();
            }

            transform.localScale = scale;

            foreach (MeshRenderer mr in meshRenderersToChange)
            {
                mr.material.color = color;
            }
        }

        public void SetVariation(int newVariation)
        {
            currentVariation = newVariation - 1;
            NextVariation();
        }

		public void NextVariation()
        {
			currentVariation++;
			if (currentVariation == System.Math.Max(scales.Count, colors.Count))
				currentVariation = 0;

            transform.position += new Vector3(0, 0.1f, 0);
            transform.localScale = GetScale();
            print(transform.localScale);
            foreach (MeshRenderer mr in meshRenderersToChange)
            {
                mr.material.color = GetColor();
            }
		}

        private Vector3 GetDefaultScale()
        {
            float x = transform.localScale.x;
            float y = transform.localScale.y;
            float z = transform.localScale.z;
            return new Vector3(x, y, z);
        }

        private Color GetDefaultColor()
        {
            if (meshRenderersToChange.Count == 0)
                return Color.white;
            return meshRenderersToChange[0].material.color;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(currentVariation);
            }
            else
            {
                int newVariation = (int)stream.ReceiveNext();
                if (newVariation != currentVariation)
                    SetVariation(newVariation);
            }
        }
    }
}
