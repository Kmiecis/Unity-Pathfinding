using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
	public bool displayGrid;
	public LayerMask unwalkableMask;

	public Vector2 gridPlane;

	public float gridNodeRadius = .5f;

    public Grid grid;

    public int obstacleProximityPenalty = 10;
    public TerrainType[] walkableRegions;

    LayerMask walkableMask;
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();

    int penaltyMin = int.MaxValue;
	int penaltyMax = int.MinValue;

	private void Awake()
	{
        grid = new Grid(gridPlane.x, gridPlane.y, gridNodeRadius);
    }

    private void Start()
    {
        foreach (TerrainType region in walkableRegions)
        {
            walkableMask.value |= region.terrainMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
        }

        FillGrid();
        BlurPenaltyMap(3);
    }

    private void Update()
    {
        
    }

    /// <summary>
    /// Function to fill grid with nodes based on actual terrain meshes.
    /// </summary>
    public void FillGrid()
	{
        Vector3 worldBottomLeft = transform.position - new Vector3(gridPlane.x / 2, 0, gridPlane.y / 2);

		for (int x = 0; x < grid.Size.X; ++x)
		{
			for (int y = 0; y < grid.Size.Y; ++y)
			{
                Vector3 gridPoint = new Vector3(
                    x * grid.Node.Diameter + grid.Node.Radius,
                    0,
                    y * grid.Node.Diameter + grid.Node.Radius
                );
                Vector3 worldPoint = worldBottomLeft + gridPoint;

				// If there is collision with object of Unwalkable Mask, walkable should equal false.
				bool walkable = !(Physics.CheckSphere(worldPoint, gridNodeRadius, unwalkableMask));

                // If there is a collision with top of object, thus indicating that target is within some object, walkable should be false.
                Ray walkableRay = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                if (Physics.Raycast(walkableRay, 100.0f, unwalkableMask))
                {
                    walkable = false;
                }

				int movementPenalty = 0;
				// Smoothing walkable
				Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);  // Cast ray from the sky to the plane to check mask.
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, 100, walkableMask))
				{
					walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    walkable = false;
				}

				// Increasing penalty if going near obstacles
				if (!walkable)
				{
					movementPenalty += obstacleProximityPenalty;
				}

				grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
			}
		}
	}

    /// <summary>
    /// Function to blur penalties values between walkable / unwalkable mask.
    /// </summary>
    /// <param name="blurSize"></param>
    public void BlurPenaltyMap(int blurSize)
	{
		// kernelSize has to be odd number
		int kernelSize = blurSize * 2 + 1;
		// how many squares are there between central square and the edge of the kernel
		int kernelExtents = (kernelSize - 1) / 2;

		int[,] penaltiesHorizontalPass = new int[grid.Size.X, grid.Size.Y];
		int[,] penaltiesVerticalPass = new int[grid.Size.X, grid.Size.Y];

		// Rows
		for (int y = 0; y < grid.Size.Y; ++y)
		{
			for (int x = -kernelExtents; x <= kernelExtents; ++x)
			{	
				// preventing value to going off the grid
				int sampleX = Mathf.Clamp(x, 0, kernelExtents);
				penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
			}

			for (int x = 1; x < grid.Size.X; ++x)
			{
				// index of the node which is no longer inside of the kernel
				int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, grid.Size.X);
				// index of node that just entered the kernel
				int addIndex = Mathf.Clamp(x + kernelExtents, 0, grid.Size.Y - 1);

				penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
			}
		}

		// Columns
		for (int x = 0; x < grid.Size.X; ++x)
		{
			for (int y = -kernelExtents; y <= kernelExtents; ++y)
			{
				// preventing value to going off the grid
				int sampleY = Mathf.Clamp(y, 0, kernelExtents);
				penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
			}

			int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
			grid[x, 0].movementPenalty = blurredPenalty;

			for (int y = 1; y < grid.Size.Y; ++y)
			{
				// index of the node which is no longer inside of the kernel
				int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, grid.Size.Y);
				// index of node that just entered the kernel
				int addIndex = Mathf.Clamp(y + kernelExtents, 0, grid.Size.Y - 1);

				penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
				blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
				grid[x, y].movementPenalty = blurredPenalty;

				if (blurredPenalty > penaltyMax)
				{
					penaltyMax = blurredPenalty;
				}
				if (blurredPenalty < penaltyMin)
				{
					penaltyMin = blurredPenalty;
				}
			}
		}
	}

    private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position, new Vector3(gridPlane.x, 1, gridPlane.y));

		if (grid != null && displayGrid)
		{
			for (int x = 0; x < grid.Size.X; ++x)
            {
                for (int y = 0; y < grid.Size.Y; ++y)
                {
                    Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, grid[x,y].movementPenalty));

                    Gizmos.color = (grid[x, y].walkable) ? Gizmos.color : Color.red;
                    Gizmos.DrawCube(grid[x, y].position, Vector3.one * (grid.Node.Diameter - .1f));
                }
            }
		}
	}

	[System.Serializable]
	public class TerrainType
	{
		public LayerMask terrainMask;
		public int terrainPenalty;
	}
}
