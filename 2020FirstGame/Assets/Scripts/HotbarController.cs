using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarController : MonoBehaviour
{
    private int selection = 0;
    private int numSelections = 4;

    private SpriteRenderer spriteRenderer;
    public EntityController entityController;
    public TerrainController terrainController;
    public GameObject cursorSelection;

    public Sprite[] selectionSprites;
    private Action[] selectionActions;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = selectionSprites[0];

        selectionActions = new Action[]
        {
            () => ReplaceTile("dirt"),
            () => PlaceEntity("miner"),
            () => PlaceEntity("sheep"),
            () => ReplaceTile("rock")
        };
    }

    public void ScrollSelection(bool directionIsRight)
    {
        int change = -1;
        if (directionIsRight)
        {
            change = 1;
        }
        ChangeSelection((numSelections + selection + change) % numSelections);
    }

    public void ChangeSelection(int newSelection)
    {
        selection = newSelection;
        spriteRenderer.sprite = selectionSprites[selection];
    }

    public void PlaceEntity(string objName)
    {
        Vector3 position = cursorSelection.transform.position;
        entityController.AddEntity(position.x, position.y, objName);
    }

    public void ReplaceTile(string tileName)
    {
        Vector3 position = cursorSelection.transform.position;

        terrainController.SetTileAtPosition(position, tileName);
    }

    public void UseSelection()
    {
        selectionActions[selection]();
    }
}
