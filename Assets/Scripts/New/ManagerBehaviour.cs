using Custom.CaveGeneration;
using System.Collections;
using UnityEngine;

namespace Custom
{
	public class ManagerBehaviour : MonoBehaviour
	{
		[SerializeField] protected MeshFilter m_CaveMeshFilter;
		[SerializeField] protected Transform m_GroundTransform;

		public CaveGenerator.Input caveInput = CaveGenerator.Input.Default;
		public CaveMeshGenerator.Input caveMeshInput = CaveMeshGenerator.Input.Default;

		public void Build()
		{
			if (m_CaveMeshFilter == null)
				return;
			if (m_CaveMeshFilter.sharedMesh == null)
				m_CaveMeshFilter.sharedMesh = new Mesh();

			var caveMap = CaveGenerator.Generate(in caveInput);
			var caveMeshBuilder = CaveMeshGenerator.Generate(caveMap, in caveMeshInput);

			caveMeshBuilder.Overwrite(m_CaveMeshFilter.sharedMesh);

			if (m_GroundTransform != null)
			{
				m_GroundTransform.localScale = new Vector3(
					(caveInput.width - 1) * caveMeshInput.squareSize,
					(caveInput.height - 1) * caveMeshInput.squareSize - 1,
					1.0f
				);
			}
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			StartCoroutine(BuildNextFrame());
		}

		IEnumerator BuildNextFrame()
		{
			yield return null;
			Build();
		}
#endif
	}
}