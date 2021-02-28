using Custom.CaveGeneration;
using Custom.Pathfinding;
using System.Collections;
using UnityEngine;

namespace Custom
{
	public class ManagerBehaviour : MonoBehaviour
	{
#if UNITY_EDITOR
		[SerializeField] protected bool m_ShowTargetGizmo;
		[SerializeField] protected bool m_ShowCaveMapGizmo;
#endif

		[SerializeField] protected MeshFilter m_CaveMeshFilter;
		[SerializeField] protected Transform m_GroundTransform;

		public UnitBehaviour unitBehaviour;

		public CaveGenerator.Input caveInput = CaveGenerator.Input.Default;
		public CaveMeshGenerator.Input caveMeshInput = CaveMeshGenerator.Input.Default;
		[Space(10)]
		public bool auto = true;
		public Vector2Int targetPosition;

		private bool[,] m_CaveMap;

		private void BuildCaveMap()
		{
			m_CaveMap = CaveGenerator.Generate(in caveInput);
		}

		private void BuildCaveMapMesh()
		{
			if (m_CaveMeshFilter == null)
				return;
			if (m_CaveMeshFilter.sharedMesh == null)
				m_CaveMeshFilter.sharedMesh = new Mesh();

			var caveMeshBuilder = CaveMeshGenerator.Generate(m_CaveMap, in caveMeshInput);

			caveMeshBuilder.Overwrite(m_CaveMeshFilter.sharedMesh);
		}
		
		private void RepositionGround()
		{
			if (m_GroundTransform != null)
			{
				var width = (caveInput.width - 1) * caveMeshInput.squareSize;
				var height = (caveInput.height - 1) * caveMeshInput.squareSize;
				m_GroundTransform.localPosition = new Vector3(width * 0.5f, height * 0.5f, 0.0f);
				m_GroundTransform.localScale = new Vector3(width, height, 1.0f);
			}
		}

		public void Build()
		{
			BuildCaveMap();
			BuildCaveMapMesh();
			RepositionGround();
		}
		
		public void SetUnitPath()
		{
			var startPosition = Vector2Int.RoundToInt(unitBehaviour.transform.position);
			var path = PathGenerator.Generate(m_CaveMap, startPosition, targetPosition);
			unitBehaviour.SetPath(path, Vector2.one * caveMeshInput.squareSize);
		}

		private void AutoUnitPath()
		{
			var startPosition = Vector2Int.RoundToInt(unitBehaviour.transform.position);

			int caveWidth = m_CaveMap.GetLength(0);
			int caveHeight = m_CaveMap.GetLength(1);
			int x, y;
			do
			{
				x = Random.Range(0, caveWidth);
				y = Random.Range(0, caveHeight);
			}
			while (m_CaveMap[x, y]);

			targetPosition = new Vector2Int(x, y);

			var path = PathGenerator.Generate(m_CaveMap, startPosition, targetPosition);
			unitBehaviour.SetPath(path, Vector2.one * caveMeshInput.squareSize);
			unitBehaviour.StartFollowPath();
		}

		private void Start()
		{
			Build();
		}

		private void Update()
		{
			if (auto && unitBehaviour != null && unitBehaviour.HasReachedDestination)
			{
				AutoUnitPath();
			}
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (m_ShowCaveMapGizmo)
			{
				var caveMapWidth = m_CaveMap.GetLength(0);
				var caveMapHeight = m_CaveMap.GetLength(1);

				for (int y = 0; y < caveMapHeight; y++)
				{
					for (int x = 0; x < caveMapWidth; x++)
					{
						bool isWall = m_CaveMap[x, y];
						Gizmos.color = isWall ? Color.black : Color.white;
						Gizmos.DrawWireSphere(new Vector3(x * caveMeshInput.squareSize, y * caveMeshInput.squareSize, -1.0f), 0.2f);
					}
				}
			}
			if (m_ShowTargetGizmo)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(new Vector3(targetPosition.x * caveMeshInput.squareSize, targetPosition.y * caveMeshInput.squareSize, -1.0f), 0.33f);
			}
		}

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