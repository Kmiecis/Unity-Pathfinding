using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;   // Grid of Squares.
    public MeshFilter cave;         // Cave mesh filter to be filled with data.
    public MeshFilter walls;        // Walls mesh filter to be filled with data.

    public float wallHeight = 5.0f;

    List<Vector3> meshVertices;
    List<int> meshTriangles;

    // Each vertex contains list of triangles it is attached to.
    Dictionary<int, List<Triangle>> trianglesDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();   // Outline vertices that create a wall mesh.
    HashSet<int> checkedVertices = new HashSet<int>();  // Vertices that can not be outline vertices.

    public void GenerateMesh(int[,] map, float squareSize)
    {
        checkedVertices.Clear();
        trianglesDictionary.Clear();
        outlines.Clear();

        squareGrid = new SquareGrid(map, squareSize);

        // CREATE CAVE MESH
        if (cave != null)
        {
            CreateCaveMesh(map, squareSize);
        }
        else
        {
            Debug.Log("MeshGenerator: Cave Mesh Filter is not attached.");
        }

        // CREATE WALL MESH
        if (walls != null)
        {
            CreateWallMesh();
        }
        else
        {
            Debug.Log("MeshGenerator: Walls Mesh Filter is not attached.");
        }
    }

    /// <summary>
    /// Function to create cave mesh from map and with specific square size.
    /// </summary>
    void CreateCaveMesh(int[,] map, float squareSize)
    {
        meshVertices = new List<Vector3>();
        meshTriangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); ++x)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); ++y)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh caveMesh = new Mesh();

        caveMesh.vertices = meshVertices.ToArray();
        caveMesh.triangles = meshTriangles.ToArray();
        caveMesh.RecalculateNormals();

        cave.mesh = caveMesh;

        MeshCollider caveCollider = cave.gameObject.GetComponent<MeshCollider>();
        caveCollider.sharedMesh = caveMesh;

        // ADD UVs
        /*int tileAmount = 10;
        Vector2[] uvs = new Vector2[meshVertices.Count];
        for (int i = 0; i < uvs.Length; ++i)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) * .5f * squareSize, map.GetLength(0) * .5f * squareSize, meshVertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-map.GetLength(0) * .5f * squareSize, map.GetLength(0) * .5f * squareSize, meshVertices[i].z) * tileAmount;
            uvs[i] = new Vector2(percentX, percentY);
        }
        caveMesh.uv = uvs;*/
    }

    /// <summary>
    /// Function to properly use function Mesh From Points, based on input square.
    /// </summary>
    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;
            // 1 point
            case 1:
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;

            // 2 points
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            // Diagonal
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 3 points
            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 4 points
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                // Because none of those can be outline vertices, we add them straight away.
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }
    }

    /// <summary>
    /// Function to properly use function Create Triangle, based on input Node points.
    /// </summary>
    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3) CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4) CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5) CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6) CreateTriangle(points[0], points[4], points[5]);
    }

    /// <summary>
    /// Function to properly assign vertices to each node point and to vertices list.
    /// </summary>
    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; ++i)
        {
            // If not already assigned.
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = meshVertices.Count; // Assign next value.
                meshVertices.Add(points[i].position);       // Add next value.
            }
        }
    }

    /// <summary>
    /// Function to add mesh triangle from three node vertices.
    /// </summary>
    void CreateTriangle(Node a, Node b, Node c)
    {
        meshTriangles.Add(a.vertexIndex);
        meshTriangles.Add(b.vertexIndex);
        meshTriangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    /// <summary>
    /// Function to add Triangle structs connected to specified vertex.
    /// </summary>
    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (trianglesDictionary.ContainsKey(vertexIndexKey))
        {
            trianglesDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            trianglesDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    /// <summary>
    /// Function to create wall mesh.
    /// </summary>
    void CreateWallMesh()
    {
        CalculateMeshOutlines(); 

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; ++i)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(meshVertices[outline[i]]); // left vertex
                wallVertices.Add(meshVertices[outline[i + 1]]); // right vertex
                wallVertices.Add(meshVertices[outline[i]] - Vector3.up * wallHeight); // bottom left vertex
                wallVertices.Add(meshVertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right vertex

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }

        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();

        walls.mesh = wallMesh;

        MeshCollider wallCollider = walls.gameObject.GetComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }

    /// <summary>
    /// Function to calculate inside outlines of cave mesh.
    /// </summary>
    void CalculateMeshOutlines()
    {
        // Loop through vertices on the map to find outline vertex and then follow the outline around to meet itself again.
        for (int vertexIndex = 0; vertexIndex < meshVertices.Count; ++vertexIndex)
        {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);

                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);

                    outlines.Add(newOutline);

                    FollowOutlines(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    /// <summary>
    /// Function to return connected outline vertex to passed one.
    /// </summary>
    int GetConnectedOutlineVertex(int vertexIndex)
    {
        foreach (Triangle triangle in trianglesDictionary[vertexIndex])
        {
            for (int j = 0; j < 3; ++j)
            {
                int vertexB = triangle[j];

                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Function to check whether two vertices create an outline edge.
    /// </summary>
    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        int sharedTriangleCount = 0;
        // Check how many triangles for vertexA contains vertexB.
        foreach (Triangle triangleA in trianglesDictionary[vertexA])
        {
            if (triangleA.Contains(vertexB))
            {
                ++sharedTriangleCount;
                if (sharedTriangleCount > 1) break;
            }
        }

        return sharedTriangleCount == 1;
    }

    /// <summary>
    /// Function to follow outlines and add their indexes.
    /// </summary>
    void FollowOutlines(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);

        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);
        if (nextVertexIndex != -1)
        {
            FollowOutlines(nextVertexIndex, outlineIndex);
        }
    }

    /// <summary>
    /// Triangle struct. Holds information about three vertex indexes and their values.
    /// </summary>
    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle(int A, int B, int C)
        {
            vertexIndexA = A;
            vertexIndexB = B;
            vertexIndexC = C;

            vertices = new int[3];
            vertices[0] = A;
            vertices[1] = B;
            vertices[2] = C;
        }

        public int this[int i]
        {
            get
            {
                return vertices[i];
            }
        }

        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    /// <summary>
    /// Square Grid class. Holds Square representation of map.
    /// </summary>
    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            // Create Control Nodes for whole map.
            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];
            for (int x = 0; x < nodeCountX; ++x)
            {
                for (int y = 0; y < nodeCountY; ++y)
                {
                    Vector3 position = new Vector3(
                        -mapWidth * .5f + x * squareSize + squareSize * .5f,
                        0,
                        -mapHeight * .5f + y * squareSize + squareSize * .5f
                        );
                    controlNodes[x, y] = new ControlNode(position, map[x, y] == 1, squareSize);
                }
            }

            // Create Squares for whole map.
            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; ++x)
            {
                for (int y = 0; y < nodeCountY - 1; ++y)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }

    /// <summary>
    /// Square class. Holds 4 Nodes and 4 Control Nodes. Combination of those and their settings defines how square will be rendered.
    /// </summary>
    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            if (topLeft.active)
                configuration += 8;
            if (topRight.active)
                configuration += 4;
            if (bottomRight.active)
                configuration += 2;
            if (bottomLeft.active)
                configuration += 1;
        }
    }

    /// <summary>
    /// Node class. Holds information about position and vertex index.
    /// </summary>
    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _position)
        {
            position = _position;
        }
    }
    
    /// <summary>
    /// Control Node class. Extends Node class with information about activity and reference to Nodes above and right.
    /// </summary>
    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _position, bool _active, float squareSize) :
            base(_position)
        {
            active = _active;
            above = new Node(position + Vector3.forward * squareSize * .5f);
            right = new Node(position + Vector3.right * squareSize * .5f);
        }
    }
}
