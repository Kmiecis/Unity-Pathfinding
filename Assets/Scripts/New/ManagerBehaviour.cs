using Custom.CaveGeneration;
using System.Collections;
using UnityEngine;

namespace Custom
{
	public class ManagerBehaviour : MonoBehaviour
	{
		[SerializeField] protected MeshFilter m_MeshFilter;

		public CaveGenerator.Input caveInput = CaveGenerator.Input.Default;
		public CaveMeshGenerator.Input caveMeshInput = CaveMeshGenerator.Input.Default;

		public void Build()
		{
			if (m_MeshFilter == null)
				return;
			if (m_MeshFilter.sharedMesh == null)
				m_MeshFilter.sharedMesh = new Mesh();

			var caveMap = CaveGenerator.Generate(in caveInput);
			var caveMeshBuilder = CaveMeshGenerator.Generate(caveMap, in caveMeshInput);

			caveMeshBuilder.Overwrite(m_MeshFilter.sharedMesh);
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