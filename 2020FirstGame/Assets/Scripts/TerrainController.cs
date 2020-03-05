﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System;

public class TerrainController : MonoBehaviour
{

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
    public Tile grassTile;
    public Tile dirtTile;
    public Tile coalTile;
    public Tile ironTile;
    public Tile rockTile;

    int terrainWidth;
    int terrainHeight;

    [HideInInspector]
    public bool isGenerated = false;

    readonly Dictionary<string, int> tileIDs = new Dictionary<string, int>();
    readonly Tile[] IDtiles = new Tile[5];
    readonly bool[] tileTypeIsSolid = new bool[5];

    public EntityController entityController;

    public class TerrainArray
    {
        readonly int chunkSize;

        public HashSet<Tuple<int, int>> loadedChunks = new HashSet<Tuple<int, int>>();
        public Dictionary<Tuple<int, int>, int[,]> chunkLookUp = new Dictionary<Tuple<int, int>, int[,]>();
        public TerrainArray(int chunkLength)
        {
            chunkSize = chunkLength;
            Debug.Log("made terrain array");
        }

        public int GetChunkSize()
        {
            return chunkSize;
        }

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

        public int Get(int x, int y)
        {
            //always assumes chunk had been generated
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

            /*
            chunkX    -1            0
            localX    0 1 2 3 ...   0 1 2 3 ...
            //*/
            if (!chunkLookUp.ContainsKey(new Tuple<int, int>(chunkX, chunkY)))
            {
                //Debug.Log("keys:");
                /*string keys = "";
                foreach (Tuple<int, int> key in chunkLookUp.Keys)
                {
                    keys += (key.ToString());
                }//*/
                //Debug.Log(keys);
                //Debug.Log("looking for:");
                //Debug.Log(new Tuple<int, int>(chunkX, chunkY));
                return -1;
            }
            int[,] chunk = chunkLookUp[new Tuple<int, int>(chunkX, chunkY)];

            return chunk[localX, localY];
        }

        public void Set(int x, int y, int value)
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

            /*
            chunkX    -1            0
            localX    0 1 2 3 ...   0 1 2 3 ...
            //*/

            if (!chunkLookUp.ContainsKey(new Tuple<int, int>(chunkX, chunkY)))
            {
                //Debug.Log("adding chunk");
                chunkLookUp.Add(new Tuple<int, int>(chunkX, chunkY), new int[chunkSize, chunkSize]);
            }
            int[,] chunk = chunkLookUp[new Tuple<int, int>(chunkX, chunkY)];


            //Debug.LogFormat("setting terrain at {0}, {1} (chunk at {2}, {3})", x, y, chunkX, chunkY);
            chunk[localX, localY] = value;
            
        }
    }

    public void Start()
    {
        tileIDs.Add("dirt", 0);
        tileIDs.Add("grass", 1);
        tileIDs.Add("coal", 2);
        tileIDs.Add("iron", 3);
        tileIDs.Add("rock", 4);
        IDtiles[0] = dirtTile;
        IDtiles[1] = grassTile;
        IDtiles[2] = coalTile;
        IDtiles[3] = ironTile;
        IDtiles[4] = rockTile;
        tileTypeIsSolid[0] = false;
        tileTypeIsSolid[1] = false;
        tileTypeIsSolid[2] = false;
        tileTypeIsSolid[3] = false;
        tileTypeIsSolid[4] = true;

        terrainWidth = tilemapSize.x;
        terrainHeight = tilemapSize.y;

        xOffset = UnityEngine.Random.Range(1, 99999);
        yOffset = UnityEngine.Random.Range(1, 99999);
    }

    public bool IsItSolidAt(Vector3 position)
    {
        int targetTile = GetTileAtPosition(position);
        if (targetTile >= 0)
        {
            return tileTypeIsSolid[targetTile];
        }
        else
        {
            return false; // don't let player move out of bounds
        }
        
    }

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
            ClearMap(false);
            terrainArray = new TerrainArray(chunkLength);
        }

        //first find chunks to load
        HashSet<Tuple<int, int>> nextLoadedChunks = new HashSet<Tuple<int, int>>();

        //put a for loop through chunks so that below function runs on one chunk at a time
        for (int i = centrex - LoadedChunksRadius; i < centrex + LoadedChunksRadius; i++)
        {
            for (int j = centrey - LoadedChunksRadius; j < centrey + LoadedChunksRadius; j++)
            {
                Tuple<int, int> chunkCoords = new Tuple<int, int>(i, j);
                nextLoadedChunks.Add(chunkCoords);

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
            if (!nextLoadedChunks.Contains(chunk))
            {
                UnloadChunk(chunk.Item1, chunk.Item2);
                //Debug.Log("unloaded a chunk");
            }
        }
        terrainArray.loadedChunks = nextLoadedChunks;


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
        entityController.LoadChunkEntities(new Tuple<int, int>(chunkX, chunkY));

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

        //use output of perlin noise to determine tile type
        if (notSeed < grassWeight)
        {
            tileID = 1;//Grass
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

        if (x == 0 && y == 0)
        {
            entityController.AddEntity(x, y, "sheep");
        }

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

    public int[,] GenerateIteration(int[,] oldMap)
    {
        int[,] newMap = new int[terrainWidth, terrainHeight];
        int numNeighbours;
        int numRocks;
        BoundsInt neighbourBounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        for (int x = 0; x < terrainWidth; x++)
        {
            for (int y = 0; y < terrainHeight; y++)
            {
                numNeighbours = 0;
                numRocks = 0;
                foreach (var neighbour in neighbourBounds.allPositionsWithin)
                {
                    if (neighbour.x == 0 && neighbour.y == 0) { continue; }
                    else
                    {
                        Vector3Int neighbourOnMap = new Vector3Int(neighbour.x + x, neighbour.y + y, neighbour.z);
                        if (neighbourOnMap.x >= 0 && neighbourOnMap.x < terrainWidth && neighbourOnMap.y >= 0 && neighbourOnMap.y < terrainHeight)
                        {
                            if (oldMap[neighbourOnMap.x, neighbourOnMap.y] == 1)
                            {
                                numNeighbours += 1;
                            }
                            else if (oldMap[neighbourOnMap.x, neighbourOnMap.y] == 2)
                            {
                                numNeighbours -= 2;
                            }
                            else if (oldMap[neighbourOnMap.x, neighbourOnMap.y] == 4)
                            {
                                numRocks += 1;
                                numNeighbours += 2;
                            }
                        }
                        else
                        {
                            numNeighbours += 1;
                        }
                    }
                }

                if (oldMap[x, y] == 1)
                {
                    if (numNeighbours < grassDeathThreshold)
                    {
                        newMap[x, y] = 0;
                    }
                    else if (numRocks > rockBirthThreshold)
                    {
                        newMap[x, y] = 4;
                    }
                    else
                    {
                        newMap[x, y] = 1;
                    }
                }
                else if (oldMap[x, y] == 0)
                {
                    if (numNeighbours > grassBirthThreshold)
                    {
                        newMap[x, y] = 1;
                    }
                    else if (numNeighbours < -coalBirthThreshold)
                    {
                        newMap[x, y] = 2;
                    }
                    else
                    {
                        newMap[x, y] = 0;
                    }
                }
                else if (oldMap[x, y] == 2)
                {
                    newMap[x, y] = numNeighbours > -coalDeathThreshold ? 0 : 2;
                }
                else if (oldMap[x, y] == 4)
                {
                    //if not enough rocks around, or there is no grass around and we aren't surrounded by rock
                    newMap[x, y] = numRocks < rockDeathThreshold || (numNeighbours < 1 && numRocks < 8) ? 1 : 4;
                }
            }
        }

        return newMap;
    }

    public void ClearMap(bool complete)
    {
        tilesNotSolid.ClearAllTiles();
        tilesSolid.ClearAllTiles();

        if (complete)
        {
            terrainArray = null;
        }
    }
}