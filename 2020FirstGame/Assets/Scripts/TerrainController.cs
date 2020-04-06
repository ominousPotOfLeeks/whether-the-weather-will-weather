using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System;
using System.Linq.Expressions;

public class TerrainController : MonoBehaviour
{
    [Range(0, 100)]
    public int sheepWeight;
    [Range(0,100)]
    public int grassWeight;
    [Range(0, 100)]
    public int coalWeight;
    [Range(0, 100)]
    public int rockWeight;

    [Range(1, 8)]
    public int grassBirthThreshold;
    [Range(1, 8)]
    public int grassDeathThreshold;
    [Range(1, 8)]
    public int coalBirthThreshold;
    [Range(1, 8)]
    public int coalDeathThreshold;
    [Range(1, 8)]
    public int rockBirthThreshold;
    [Range(1, 8)]
    public int rockDeathThreshold;

    [Range(0, 10)]
    public int numRepetitions;

    public float scale;
    public float weight;
    public float scale2;
    public float weight2;
    int xOffset = 0;
    int yOffset = 0;
    //method:   0 -> random
    //          1 -> perlin noise
    //          2 -> perlin noise + random offset

    public TerrainArray terrainArray;
    public Vector3Int tilemapSize;
    public int chunkLength;
    public int LoadedChunksRadius;

    public Tilemap tilesNotSolid;
    public Tilemap tilesSolid;
    public Tilemap tilesUnloadedWall;//requires its own grid so it can be moved easily
    public Tile grassTile;
    public Tile dirtTile;
    public Tile coalTile;
    public Tile ironTile;
    public Tile rockTile;
    public Tile sheepTile;
    public Tile minerTile;
    public Tile wheelTile;

    public int coalResourceAmount;

    [HideInInspector]
    public bool isGenerated = false;

    readonly int numTileTypes = 8;
    readonly Dictionary<string, int> tileIDs = new Dictionary<string, int>();
    Tile[] IDtiles;
    bool[] tileTypeIsSolid;

    public EntityController entityController;

    /// <summary>
    /// A dictionary of "chunks" which are 2D square arrays of some given size. Acts to the outside like an infinite 
    /// 2D array, but provides information about its chunk positions if required.
    /// </summary>
    public class TerrainArray
    {
        readonly int chunkSize;

        public HashSet<Tuple<int, int>> loadedChunks = new HashSet<Tuple<int, int>>();
        public HashSet<Tuple<int, int>> nextLoadedChunks = new HashSet<Tuple<int, int>>();
        public Dictionary<Tuple<int, int>, int[,]> chunkLookUp = new Dictionary<Tuple<int, int>, int[,]>();
        public Dictionary<Tuple<int, int>, int[,]> chunkDataLookUp = new Dictionary<Tuple<int, int>, int[,]>();
        public TerrainArray(int chunkLength)
        {
            chunkSize = chunkLength;
        }

        public int GetChunkSize()
        {
            return chunkSize;
        }

        /// <summary>
        /// Returns the key which corresponds to the chunk containing the coordinate (x, y). This key is pretty much
        /// (x/chunkSize, y/chunkSize), but shifted for negatives.
        /// </summary>
        /// <param name="x">global x coordinate</param>
        /// <param name="y">global y coordinate</param>
        /// <returns>the key which corresponds to the chunk containing the coordinate (x, y)</returns>
        public Tuple<int, int> GetChunkCoords(int x, int y)
        {
            int chunkX;
            int chunkY;

            if (x < 0)
            {
                chunkX = (x - (chunkSize - 1)) / chunkSize;
            }
            else
            {
                chunkX = x / chunkSize;
            }
            if (y < 0)
            {
                chunkY = (y - (chunkSize - 1)) / chunkSize;
            }
            else
            {
                chunkY = y / chunkSize;
            }
            return new Tuple<int, int>(chunkX, chunkY);
        }

        /// <summary>
        /// Returns the key which corresponds to the chunk containing the coordinate (x, y), 
        /// and the local coordinate of (x, y) within the chunk. This key is pretty much
        /// (x/chunkSize, y/chunkSize), but shifted for negatives, and the local coordinate
        /// is the remainder from this division.
        /// </summary>
        /// <param name="x">global x coordinate</param>
        /// <param name="y">global y coordinate</param>
        /// <returns>the key which corresponds to the chunk containing the coordinate (x, y), 
        /// and the local coordinate of (x, y) within the chunk</returns>
        public Tuple<int, int, int, int> GetChunkAndLocalCoords(int x, int y)
        {
            int chunkX;
            int chunkY;
            int localX; // position of tile relative to chunk
            int localY;

            if (x < 0)
            {
                chunkX = (x - (chunkSize - 1)) / chunkSize;
                localX = x - chunkX * chunkSize;// shift by -1 so that array starts at zero instead of 1
            }
            else
            {
                chunkX = x / chunkSize;
                localX = x - chunkX * chunkSize;
            }
            if (y < 0)
            {
                chunkY = (y - (chunkSize - 1)) / chunkSize;
                localY = y - chunkY * chunkSize;
            }
            else
            {
                chunkY = y / chunkSize;
                localY = y - chunkY * chunkSize;
            }

            return new Tuple<int, int, int, int>(chunkX, chunkY, localX, localY);
        }

        /// <summary>
        /// Accesses terrain array and returns the value at the given global coordinates,
        /// or -1 if the chunk containing those coordinates has not been generated.
        /// </summary>
        /// <param name="x">global x coordinate</param>
        /// <param name="y">global y coordinate</param>
        /// <returns>the value at the given global coordinates,
        /// or -1 if the chunk containing those coordinates has not been generated</returns>
        public int Get(int x, int y)
        {
            //always assumes chunk had been generated
            Tuple<int, int, int, int> nums = GetChunkAndLocalCoords(x, y);
            Tuple<int, int> chunkCoords = new Tuple<int, int>(nums.Item1, nums.Item2);
            int localX = nums.Item3; // position of tile relative to chunk
            int localY = nums.Item4;
            /*
            chunkCoords.Item1      -1             0
            localX                  0 1 2 3 ...   0 1 2 3 ...
            //*/

            if (!chunkLookUp.ContainsKey(chunkCoords))
            {
                return -1;
            }
            return chunkLookUp[chunkCoords][localX, localY];
        }

        public void Set(int x, int y, int value)
        {
            Tuple<int, int, int, int> nums = GetChunkAndLocalCoords(x, y);
            Tuple<int, int> chunkCoords = new Tuple<int, int>(nums.Item1, nums.Item2);
            int localX = nums.Item3; // position of tile relative to chunk
            int localY = nums.Item4;

            if (!chunkLookUp.ContainsKey(chunkCoords))
            {
                //Debug.Log("adding chunk");
                chunkLookUp.Add(chunkCoords, new int[chunkSize, chunkSize]);
            }
            chunkLookUp[chunkCoords][localX, localY] = value;
            
        }

        /// <summary>
        /// Accesses extra data about a tile. This extra data is for tiles that have changing behaviour
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int GetTileData(int x, int y)
        {
            Tuple<int, int, int, int> nums = GetChunkAndLocalCoords(x, y);
            Tuple<int, int> chunkCoords = new Tuple<int, int>(nums.Item1, nums.Item2);
            int localX = nums.Item3; // position of tile relative to chunk
            int localY = nums.Item4;

            if (!chunkDataLookUp.ContainsKey(chunkCoords))
            {
                //Debug.Log("adding chunk");
                return -1;
            }
            return chunkDataLookUp[chunkCoords][localX, localY];
        }

        public void SetTileData(int x, int y, int value)
        {
            Tuple<int, int, int, int> nums = GetChunkAndLocalCoords(x, y);
            Tuple<int, int> chunkCoords = new Tuple<int, int>(nums.Item1, nums.Item2);
            int localX = nums.Item3; // position of tile relative to chunk
            int localY = nums.Item4;

            if (!chunkDataLookUp.ContainsKey(chunkCoords))
            {
                //Debug.Log("adding chunk");
                chunkDataLookUp.Add(chunkCoords, new int[chunkSize, chunkSize]);
                for (int i = 0; i < chunkSize; i++)
                {
                    for (int j = 0; j < chunkSize; j++)
                    {
                        chunkDataLookUp[chunkCoords][i, j] = -1;
                    }
                }
            }
            chunkDataLookUp[chunkCoords][localX, localY] = value;
        }
    }

    public void Awake()
    {
        tileIDs.Add("dirt", 0);
        tileIDs.Add("grass", 1);
        tileIDs.Add("coal", 2);
        tileIDs.Add("iron", 3);
        tileIDs.Add("rock", 4);
        tileIDs.Add("sheep", 5);
        tileIDs.Add("miner", 6);
        tileIDs.Add("wheel", 7);
        IDtiles = new Tile[] { dirtTile, grassTile, coalTile, ironTile, rockTile, sheepTile, minerTile, wheelTile};
        tileTypeIsSolid = new bool[numTileTypes];
        tileTypeIsSolid[0] = false;
        tileTypeIsSolid[1] = false;
        tileTypeIsSolid[2] = false;
        tileTypeIsSolid[3] = false;
        tileTypeIsSolid[4] = true;
        tileTypeIsSolid[5] = true;
        tileTypeIsSolid[6] = false;
        tileTypeIsSolid[7] = true;

        xOffset = UnityEngine.Random.Range(1, 99999);
        yOffset = UnityEngine.Random.Range(1, 99999);
    }
    /// <summary>
    /// Rounds a vector to integers
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public int[] VectorToGrid(Vector3 position)
    {
        Vector3Int roundedPosition = new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), 0);

        return new int[2] { roundedPosition.x, roundedPosition.y };
    }

    public int GetTileAtPosition(Vector3 position)
    {
        if (isGenerated)
        {
            int[] gridPosition = VectorToGrid(position);

            if (terrainArray.chunkLookUp.ContainsKey(terrainArray.GetChunkCoords(gridPosition[0], gridPosition[1])))
            {
                return terrainArray.Get(gridPosition[0], gridPosition[1]);
            } 
            else
            {
                return -1;
            }
        } else
        {
            return -1;
        }
        
    }

    public int GetTileID(string tileName)
    {
        return tileIDs[tileName];
    }

    public Tile GetTile(int tileID)
    {
        return IDtiles[tileID];
    }

    public bool IsTileTypeSolid(int tileID)
    {
        return IsTileTypeSolid(tileID);
    }

    public void SetTileAtPosition(Vector3 position, string tileName)
    {
        Vector3Int roundedPosition = new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), 0);
        int[] gridPosition = VectorToGrid(position);
        int newTileID = tileIDs[tileName];
        int oldTileID = terrainArray.Get(gridPosition[0], gridPosition[1]);
        terrainArray.Set(gridPosition[0], gridPosition[1], newTileID);

        if (tileTypeIsSolid[newTileID])
        {
            tilesSolid.SetTile(roundedPosition, IDtiles[newTileID]);
            if (!tileTypeIsSolid[oldTileID])
            {
                tilesNotSolid.SetTile(roundedPosition, null);
            }
        } 
        else
        {
            tilesNotSolid.SetTile(roundedPosition, IDtiles[newTileID]);
            if (tileTypeIsSolid[oldTileID])
            {
                tilesSolid.SetTile(roundedPosition, null);
            }
        }
    }



    public void GenerateTerrain(int centrex, int centrey)
    {

        if (terrainArray == null)
        {
            //first generation
            terrainArray = new TerrainArray(chunkLength);
        }

        terrainArray.nextLoadedChunks.Clear();

        for (int i = centrex - LoadedChunksRadius; i < centrex + LoadedChunksRadius; i++)
        {
            for (int j = centrey - LoadedChunksRadius; j < centrey + LoadedChunksRadius; j++)
            {
                Tuple<int, int> chunkCoords = new Tuple<int, int>(i, j);
                terrainArray.nextLoadedChunks.Add(chunkCoords);

                //check if chunk is fresh (never generated)
                if (!terrainArray.chunkLookUp.ContainsKey(chunkCoords))
                {
                    //generate and display
                    GenerateChunk(i, j, true);
                } 
                else if (!terrainArray.loadedChunks.Contains(chunkCoords))
                {
                    //display without generation if previously generated but not loaded
                    GenerateChunk(i, j, false);
                }
            }
        }

        //unload chunks that should no longer be loaded
        foreach (Tuple<int, int> chunk in terrainArray.loadedChunks)
        {
            if (!terrainArray.nextLoadedChunks.Contains(chunk))
            {
                UnloadChunk(chunk.Item1, chunk.Item2);
                //Debug.Log("unloaded a chunk");
            }
        }
        terrainArray.loadedChunks = new HashSet<Tuple<int, int>>(terrainArray.nextLoadedChunks);

        if (isGenerated)
        {
            UpdateUnloadedWallPosition(centrex, centrey);
        }
        else
        {
            GenerateUnloadedWall(centrex, centrey);
        }
    }

    public void UpdateUnloadedWallPosition(int centrex, int centrey)
    {
        tilesUnloadedWall.transform.parent.position = new Vector3(centrex * chunkLength, centrey * chunkLength);
    }

    public void GenerateUnloadedWall(int centrex, int centrey)
    {
        tilesUnloadedWall.transform.parent.position = new Vector3(centrex * chunkLength, centrey * chunkLength);
        int chunkSize = terrainArray.GetChunkSize();
        int x;
        int y = (centrey - LoadedChunksRadius) * chunkSize - 1;
        //top wall
        for (x=(centrex - LoadedChunksRadius) * chunkSize - 1; x < (centrex + LoadedChunksRadius) * chunkSize + 1; x++)
        {
            tilesUnloadedWall.SetTile(new Vector3Int(x, y, 0), grassTile);
        }
        y = (centrey + LoadedChunksRadius) * chunkSize;
        //bottom wall
        for (x = (centrex - LoadedChunksRadius) * chunkSize - 1; x < (centrex + LoadedChunksRadius) * chunkSize + 1; x++)
        {
            tilesUnloadedWall.SetTile(new Vector3Int(x, y, 0), grassTile);
        }


        x = (centrex - LoadedChunksRadius) * chunkSize - 1;
        //left wall
        for (y = (centrey - LoadedChunksRadius) * chunkSize; y < (centrey + LoadedChunksRadius) * chunkSize; y++)
        {
            tilesUnloadedWall.SetTile(new Vector3Int(x, y, 0), grassTile);
        }
        x = (centrex + LoadedChunksRadius) * chunkSize;
        //right wall
        for (y = (centrey - LoadedChunksRadius) * chunkSize; y < (centrey + LoadedChunksRadius) * chunkSize; y++)
        {
            tilesUnloadedWall.SetTile(new Vector3Int(x, y, 0), grassTile);
        }
    }

    public void UnloadChunk(int chunkX, int chunkY)
    {
        //unloads all tiles in a chunk

        int chunkSize = terrainArray.GetChunkSize();

        int startx = chunkX * chunkSize;
        int endx = (chunkX + 1) * chunkSize;
        int starty = chunkY * chunkSize;
        int endy = (chunkY + 1) * chunkSize;

        for (int x = startx; x < endx; x++)
        {
            for (int y = starty; y < endy; y++)
            {
                UnloadTile(x, y);
            }
        }
        entityController.UnloadChunkEntities(new Tuple<int, int>(chunkX, chunkY));
    }

    public void GenerateChunk(int chunkX, int chunkY, bool doGeneration)
    {
        //method:   0 -> random
        //          1 -> perlin noise
        //          2 -> perlin noise + random offset

        entityController.LoadChunkEntities(new Tuple<int, int>(chunkX, chunkY));//load anything generated previously first

        //Debug.Log("generating terrain...");
        int chunkSize = terrainArray.GetChunkSize();

        int startx = chunkX * chunkSize;
        int endx = (chunkX + 1) * chunkSize;
        int starty = chunkY * chunkSize;
        int endy = (chunkY + 1) * chunkSize;

        for (int x = startx; x < endx; x++)
        {
            for (int y = starty; y < endy; y++)
            {
                int value = GenerateTile(x, y, doGeneration);

                LoadTile(x, y, value);
            }
        }

        //Debug.Log("terrain generation complete");

    }

    public float PointSeed(int x, int y)
    {
        return weight * Mathf.PerlinNoise(((float)x + xOffset) * scale, ((float)y + yOffset) * scale)
            + weight2 * Mathf.PerlinNoise(((float)x + xOffset) * scale2, ((float)y + yOffset) * scale2);
    }

    public int GenerateTile(int x, int y, bool doGeneration)
    {
        int tileID;
        if (!doGeneration)
        {
            tileID = terrainArray.Get(x, y);
            if (tileID != -1)
            {
                return tileID;
            }
            else
            {
                //not sure if this ever still happens
                Debug.LogFormat("FAILED generation at {0}, {1}", x, y);
            }
        }
        //Debug.LogFormat("x: {0}, y: {1}", x, y);
        float notSeed = PointSeed(x, y);

        /*if (x == 11 && y == 6)
        {
            entityController.AddBunchOfEntities(x, y, "miner", 6f, 0.1f);
        }//*/

        //use output of perlin noise to determine tile type
        if (notSeed < grassWeight)
        {
            tileID = 1;//Grass
            if (notSeed < sheepWeight && UnityEngine.Random.Range(0, 15) == 0)
            {
                entityController.AddBunchOfEntities(x, y, "sheep", 6f, 0.1f);
            }
        }
        else if (notSeed < grassWeight + coalWeight)
        {
            tileID = 2;//Coal
        }
        else if (notSeed < grassWeight + coalWeight + rockWeight)
        {
            tileID = 4;//Rock
        }
        else
        {
            tileID = 0;//Dirt
        }//*/

        terrainArray.Set(x, y, tileID);
        return tileID;
    }

    public void UnloadTile(int x, int y)
    {
        Vector3Int position = new Vector3Int(x, y, 0);
        tilesNotSolid.SetTile(position, null);
        tilesSolid.SetTile(position, null);
    }

    public void LoadTile(int x, int y, int tileID)
    {
        
        Vector3Int position = new Vector3Int(x, y, 0);

        if (tileTypeIsSolid[tileID])
        {
            //make tile solid and visible
            tilesSolid.SetTile(position, IDtiles[tileID]);
        } 
        else
        {
            //make tile visible
            tilesNotSolid.SetTile(position, IDtiles[tileID]);
        }
    }

    public bool ChunksEqual(Tuple<int, int> chunk1, Tuple<int, int> chunk2)
    {
        return chunk1.Item1 == chunk2.Item1 && chunk1.Item2 == chunk2.Item2;
    }
}
