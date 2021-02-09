using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour
{
    public int width;               // Width of generated map.
    public int height;              // Height of generated map.
    public int passageWidth = 3;    // Width of created passage between cave rooms.
    public int borderSize = 2;      // Default map border increment.

    public string seed;         // User defined seed.
    public bool randomSeed;     // Random seed flag.

    [Range(45, 55)]
    public int fillPercent;     // Specifies the percentage of fill the generated map.

    const int wallThresholdSize = 0; // Wall regions with less than this walls will be removed.
    const int roomThresholdSize = 50; // Room regions with less than this walls will be removed.

    private int[,] map;

    private void Start()
    {
        GenerateMap();
    }

    private void Update()
    {
        
    }

    /// <summary>
    /// Function invoked to generate the map.
    /// </summary>
    public void GenerateMap()
    {
        map = new int[width, height];

        RandomFillMap();

        SmoothMap();

        ProcessMap();

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(map, 1);
    }

    /// <summary>
    /// Function to fill map with random, 0 (empty space) or 1 (wall), values.
    /// </summary>
    void RandomFillMap()
    {
        if (randomSeed)
        {
            seed = Time.time.ToString();
        }

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                // Every value outside of bordered size equals 1.
                if (x <= borderSize || x >= width - 1 - borderSize || y <= borderSize || y >= height - 1 - borderSize)
                {
                    map[x, y] = 1;
                }
                // In other case randomly fill map.
                else
                {
                    map[x, y] = (UnityEngine.Random.Range(0, 100) < fillPercent) ? 1 : 0;
                }
            }
        }
    }

    /// <summary>
    /// Function to create rooms from total randomness.
    /// </summary>
    void SmoothMap()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                {
                    map[x, y] = 1;
                }
                else if (neighbourWallTiles < 4)
                {
                    map[x, y] = 0;
                }
            }
        }
    }

    /// <summary>
    /// Function to return number of walls around passed 'grid_x' and 'grid_y' on the map.
    /// </summary>
    int GetSurroundingWallCount(int grid_x, int grid_y)
    {
        int wallCount = 0;

        for (int x = grid_x - 1; x <= grid_x + 1; ++x)
        {
            for (int y = grid_y - 1; y <= grid_y + 1; ++y)
            {
                // Check if wall or empty space only if within map range.
                if (IsInMap(x, y))
                {
                    // Don't count itself, only neighbours.
                    if (x != grid_x || y != grid_y)
                    {
                        wallCount += map[x, y];   // Map[x,y] only equals 0 or 1.
                    }
                }
                // Otherwise count always as wall.
                else
                {
                    ++wallCount;
                }
            }
        }

        return wallCount;
    }

    /// <summary>
    /// Simple check if input is within map range.
    /// </summary>
    bool IsInMap(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    /// <summary>
    /// Function to further process the map by deleting small wall regions, small room regions and connecting rooms.
    /// </summary>
    void ProcessMap()
    {
        List<List<Tile>> wallRegions = GetRegions(1);
        foreach (List<Tile> wallRegion in wallRegions)
        {
            // Get rid of all wall regions with wall count smaller than specified threshold.
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Tile tile in wallRegion)
                {
                    map[tile.x, tile.y] = 0;
                }
            }
        }

        List<Room> survivingRooms = new List<Room>();
        
        List<List<Tile>> roomRegions = GetRegions(0);
        foreach (List<Tile> roomRegion in roomRegions)
        {
            // Get rid of all room regions with wall count smaller than specified threshold.
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Tile tile in roomRegion)
                {
                    map[tile.x, tile.y] = 1;
                }
            }
            // Add all other room regions to list of leftover rooms for further process.
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }

        survivingRooms.Sort();
        // After sort, set biggest room as main room.
        survivingRooms[0].isMain = true;
        survivingRooms[0].isConnectedToMain = true;

        // Connect all rooms from those that are left within map.
        ConnectClosestRooms(survivingRooms);
    }

    /// <summary>
    /// Function to return List of regions of given tile type, 0 or 1.
    /// </summary>
    List<List<Tile>> GetRegions(int tileType)
    {
        List<List<Tile>> regions = new List<List<Tile>>();

        // Indicator if tile has been checked.
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    // Get new region from given tile.
                    List<Tile> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    // Assign tiles so they will not be taken into future consideration of possible regions.
                    foreach (Tile tile in newRegion)
                    {
                        mapFlags[tile.x, tile.y] = 1;
                    }
                }
            }
        }

        return regions;
    }

    /// <summary>
    /// Function to return number of tiles counted as all neighbours of the same type as starting tile, until only other types are near.
    /// </summary>
    List<Tile> GetRegionTiles(int startX, int startY)
    {
        List<Tile> tiles = new List<Tile>();

        // Indicator if tile has been checked.
        int[,] mapFlags = new int[width, height];
        mapFlags[startX, startY] = 1;

        // Tile type based on starting position.
        int tileType = map[startX, startY];

        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(new Tile(startX, startY));

        // Count tiles until there is none of the same type.
        while (queue.Count > 0)
        {
            Tile tile = queue.Dequeue();
            tiles.Add(tile);

            Action<int, int> ifEnqueue = (x, y) =>
            {
                if (IsInMap(x, y) && mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    queue.Enqueue(new Tile(x, y));
                    mapFlags[x, y] = 1;
                }
            };

            ifEnqueue.Invoke(tile.x - 1, tile.y);
            ifEnqueue.Invoke(tile.x, tile.y - 1);
            ifEnqueue.Invoke(tile.x + 1, tile.y);
            ifEnqueue.Invoke(tile.x, tile.y + 1);
        }

        return tiles;
    }

    /// <summary>
    /// Function to connect closest rooms from the list of rooms.
    /// </summary>
    void ConnectClosestRooms(List<Room> rooms, bool forceConnectionToMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();    // Rooms to be connected.
        List<Room> roomListB = new List<Room>();    // Rooms connected.

        if (forceConnectionToMainRoom)
        {
            foreach(Room room in rooms)
            {
                if (room.isConnectedToMain)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = rooms;
            roomListB = rooms;
        }

        int bestDistance = 0;
        // Best tiles to be connected between two rooms.
        Tile bestTileA = new Tile();
        Tile bestTileB = new Tile();
        // Best rooms to be connected.
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        // For every toom to be connected.
        foreach(Room roomA in roomListA)
        {
            if (!forceConnectionToMainRoom)
            {
                // For each room A, skip connection and go to next room.
                possibleConnectionFound = false;        
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach(Room roomB in roomListB)
            {
                // Don't compare same rooms.
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }
                
                // For each edge tiles within rooms search for smallest distance between them.
                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; ++tileIndexA)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; ++tileIndexB)
                    {
                        Tile tileA = roomA.edgeTiles[tileIndexA];
                        Tile tileB = roomB.edgeTiles[tileIndexB];

                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.x - tileB.x, 2) + Mathf.Pow(tileA.y - tileB.y, 2));

                        // Indicate that possible connection has been found and always aim for best one.
                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            // Connect rooms if connection found.
            if (possibleConnectionFound && !forceConnectionToMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceConnectionToMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(rooms, true);
        }

        if (!forceConnectionToMainRoom)
        {
            ConnectClosestRooms(rooms, true);
        }
    }

    /// <summary>
    /// Function to connect two room regions from two tiles, each for one.
    /// </summary>
    void CreatePassage(Room roomA, Room roomB, Tile tileA, Tile tileB)
    {
        Room.ConnectRooms(roomA, roomB);

        List<Tile> line = GetLine(tileA, tileB);
        foreach (Tile c in line)
        {
            DrawCircle(c, passageWidth);
        }
    }

    /// <summary>
    /// Function to create circle of empty room around given tile and radius.
    /// </summary>
    void DrawCircle(Tile c, int r)
    {
        for (int x = -r; x <= r; ++x)
        {
            for (int y = -r; y <= r; ++y)
            {
                if (x * x + y * y <= r * r)     // Inside the circle.
                {
                    int drawX = c.x + x;
                    int drawY = c.y + y;

                    if (IsInMap(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Function to get list of tiles that create line from one tile to another.
    /// </summary>
    List<Tile> GetLine(Tile from, Tile to)
    {
        List<Tile> line = new List<Tile>();

        int x = from.x;
        int y = from.y;

        int dx = to.x - from.x;
        int dy = to.y - from.y;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);   // y value change

        int longest = Math.Abs(dx);
        int shortest = Math.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Math.Abs(dy);
            shortest = Math.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; ++i)
        {
            line.Add(new Tile(x, y));
            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    /// <summary>
    /// Function to return world position from tile position.
    /// </summary>
    Vector3 CoordToWorldPoint(Tile tile)
    {
        return new Vector3(-width * .5f + .5f + tile.x, 2, -height * .5f + .5f + tile.y);
    }

    /// <summary>
    /// Simple struct to hold information about tile position.
    /// </summary>
    struct Tile
    {
        public int x;
        public int y;

        public Tile(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    class Room : IComparable<Room>
    {
        public List<Tile> tiles;
        public List<Tile> edgeTiles;
        public List<Room> connectedRooms;
        public int size;

        public bool isConnectedToMain;
        public bool isMain;

        public Room()
        {
            // No data.
        }

        public Room(List<Tile> tiles, int[,] map)
        {
            this.tiles = tiles;
            size = tiles.Count;

            connectedRooms = new List<Room>();

            edgeTiles = new List<Tile>();
            foreach (Tile tile in tiles)
            {
                Action<int, int> addEdgeTile = (x, y) =>
                {
                    if (map[x, y] == 1)
                        edgeTiles.Add(tile);
                };
                addEdgeTile.Invoke(tile.x - 1, tile.y);
                addEdgeTile.Invoke(tile.x, tile.y - 1);
                addEdgeTile.Invoke(tile.x + 1, tile.y);
                addEdgeTile.Invoke(tile.x, tile.y + 1);
                addEdgeTile.Invoke(tile.x, tile.y);
            }
        }

        /// <summary>
        /// Function to change flag to 'connected to main room' for this room and every connected to it.
        /// </summary>
        public void SetConnectedToMainRoom()
        {
            if (!isConnectedToMain)
            {
                isConnectedToMain = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetConnectedToMainRoom();
                }
            }
        }

        /// <summary>
        /// Function to connect rooms by references.
        /// </summary>
        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isConnectedToMain)
            {
                roomB.SetConnectedToMainRoom();
            }
            else if (roomB.isConnectedToMain)
            {
                roomA.SetConnectedToMainRoom();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        /// <summary>
        /// Function to check whether room is connected to other room.
        /// </summary>
        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        /// <summary>
        /// CompareTo function overload.
        /// </summary>
        public int CompareTo(Room otherRoom)
        {
            return otherRoom.size.CompareTo(size);
        }
    }
}
