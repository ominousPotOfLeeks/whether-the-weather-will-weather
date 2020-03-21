using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InventoryController : MonoBehaviour
{
    public TerrainController terrainController;

    private TilemapRenderer tilemapRenderer;
    private Tilemap tilemap;

    public int width; // in items
    public int height;
    int halfWidth;
    int halfHeight;

    public Tile grassTile;
    public Tile dirtTile;
    public Tile coalTile;
    public Tile ironTile;
    public Tile rockTile;
    public Tile sheepTile;
    public Tile minerTile;

    readonly int numTileTypes = 7;
    Tile[] IDtiles;

    private InventoryData inventoryData;

    private void Start()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        tilemapRenderer.enabled = false;
        tilemap = GetComponent<Tilemap>();

        halfWidth = width / 2;
        halfHeight = height / 2;

        IDtiles = new Tile[numTileTypes];
        IDtiles[0] = dirtTile;
        IDtiles[1] = grassTile;
        IDtiles[2] = coalTile;
        IDtiles[3] = ironTile;
        IDtiles[4] = rockTile;
        IDtiles[5] = sheepTile;
        IDtiles[6] = minerTile;

        inventoryData = new InventoryData(width, height, tilemap, ref IDtiles);
    }
    
    public void ToggleInventory()
    {
        tilemapRenderer.enabled = !tilemapRenderer.enabled;
    }

    public bool AddItem(int itemID, int numItems)
    {
        return inventoryData.AddItem(itemID, numItems);
    }

    public bool RemoveItem(int itemID)
    {
        return inventoryData.RemoveItem(itemID);
    }

    public class InventoryData
    {
        readonly int width;
        readonly int height;
        readonly int halfWidth;
        readonly int halfHeight;
        Tilemap tilemap;

        private Tile[] IDtiles;

        public List<int[]> itemAmounts = new List<int[]>();
        public Dictionary<int, int> itemPositions = new Dictionary<int, int>();

        public InventoryData(int _width, int _height, Tilemap _tilemap, ref Tile[] _IDtiles)
        {
            width = _width;
            height = _height;
            halfWidth = width / 2;
            halfHeight = height / 2;
            tilemap = _tilemap;
            IDtiles = _IDtiles;
        }

        private Vector3Int LinearToRectanglePosition(int linearPosition)
        {
            int rowNo = linearPosition / width;
            int colNo = linearPosition - rowNo * width;
            return new Vector3Int(colNo - halfWidth, rowNo - halfHeight, 0);
        }

        public bool AddItem(int itemID, int numItems)
        {
            if (itemPositions.ContainsKey(itemID))
            {
                itemAmounts[itemPositions[itemID]][1] += numItems;
            }
            else
            {
                itemPositions[itemID] = itemAmounts.Count;
                itemAmounts.Add(new int[] { itemID, numItems });
            }
            tilemap.SetTile(LinearToRectanglePosition(itemPositions[itemID]), IDtiles[itemID]);

            return true;//returns true if there is room in inventory
        }

        public bool RemoveItem(int itemID)
        {
            if (itemPositions.ContainsKey(itemID))
            {
                int itemPositionInList = itemPositions[itemID];
                itemAmounts[itemPositionInList][1]--;

                if (itemAmounts[itemPositionInList][1] <= 0)
                {
                    //shift everything back by one
                    for (int i=itemPositionInList; i < itemAmounts.Count; i++)
                    {
                        int thisItemID = itemAmounts[i][0];
                        itemPositions[thisItemID]--;
                    }
                    itemAmounts.RemoveAt(itemPositionInList);
                    itemPositions.Remove(itemID);

                    //refresh all affected items
                    for (int i = itemPositionInList; i < itemAmounts.Count; i++)
                    {
                        int thisItemID = itemAmounts[i][0];
                        tilemap.SetTile(LinearToRectanglePosition(i), IDtiles[thisItemID]);
                    }
                    tilemap.SetTile(LinearToRectanglePosition(itemAmounts.Count), null);
                }

                return true;
            }
            else
            {
                return false;
            }

            
        }
    }
}
