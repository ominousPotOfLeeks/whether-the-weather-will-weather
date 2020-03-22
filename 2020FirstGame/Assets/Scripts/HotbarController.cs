using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HotbarController : MonoBehaviour
{
    private int selection = 0;
    private int numSelections = 5;

    public PlayerController playerController;
    public EntityController entityController;
    public TerrainController terrainController;
    public GameObject cursorSelection;
    public GameObject hotbarSelection;

    public Tile[] selectionTiles;
    private Tuple<Action, Action, Action, Action>[] selectionActions;

    private CursorScript cursorScript;
    private Tilemap tilesHotbar;

    public Vector3Int hotbarCenter;
    public int hotbarLength;

    [Range(0, 100)]
    public int horizontalPositionPercentage;//as percentage of screen size
    [Range(0, 100)]
    public int verticalPositionPercentage;
    private float horizontalPosition;
    private float verticalPosition;

    private Camera cam;

    private void Start()
    {
        tilesHotbar = GetComponent<Tilemap>();
        hotbarCenter = new Vector3Int(0, 0, 0);
        ChangeSelection(0);

        cam = Camera.main;
        horizontalPosition = horizontalPositionPercentage * Screen.width * 0.01f;
        verticalPosition = verticalPositionPercentage * Screen.height * 0.01f;
        float zPosition = transform.position.z;
        Vector3 position = cam.ScreenToWorldPoint(new Vector3(horizontalPosition, verticalPosition, 0));
        transform.position = new Vector3(position.x, position.y, zPosition);
        hotbarSelection.transform.position = transform.position;

        //Item1 is on entering mousedown state, Item2 is repeated every frame while the mouse is down
        selectionActions = new Tuple<Action, Action, Action, Action>[]
        {
            new Tuple<Action, Action, Action, Action> (() => ToggleIfAbleElseDo(DoNothing),                     () => ReplaceTile("dirt"),  DoNothing,          RemoveUnderCursor),
            new Tuple<Action, Action, Action, Action> (() => ToggleIfAbleElseDo(DoNothing),                     () => ReplaceTile("coal"),  DoNothing,          RemoveUnderCursor),
            new Tuple<Action, Action, Action, Action> (() => ToggleIfAbleElseDo(() => PlaceEntity("wheel")),    DoNothing,                  DoNothing,          RemoveUnderCursor),
            new Tuple<Action, Action, Action, Action> (() => ToggleIfAbleElseDo(() => PlaceEntity("miner")),    DoNothing,                  DoNothing,          RemoveUnderCursor),
            new Tuple<Action, Action, Action, Action> (() => ToggleIfAbleElseDo(DoNothing),                     () => ReplaceTile("rock"),  DoNothing,          RemoveUnderCursor)
        };

        cursorScript = cursorSelection.GetComponent<CursorScript>();
    }

    public void DoNothing()
    {
        //just easier than a bunch of nulls and if statements
    }

    public void RemoveUnderCursor()
    {
        entityController.RemoveEntityAtPosition(cursorScript.mouseWorldPosition);
    }

    public void ToggleIfAbleElseDo(Action action)
    {
        if (entityController.ToggleEntityAtPosition(cursorScript.mouseWorldPosition))
        {
            playerController.doContinuousMouseActions = false;
        } 
        else
        {
            action();
        }
    }

    public void ToggleUnderCursor()
    {
        entityController.ToggleEntityAtPosition(cursorScript.mouseWorldPosition);
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
        int halfLength = hotbarLength / 2;
        float offset = hotbarLength / ((float) hotbarLength + 1);
        float hfs = Mathf.Max(1.0f, halfLength * halfLength);
        for (int i=-halfLength; i < hotbarLength - halfLength; i++)
        {
            Vector3Int position = new Vector3Int(i, 0, 0);
            float alpha = 1.0f - offset * (i * i) / hfs;
            tilesHotbar.SetTile(position, selectionTiles[(selection + i + numSelections) % numSelections]);
            tilesHotbar.SetColor(position, new Color(1.0f, 1.0f, 1.0f, alpha));
        }
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

    public void UseDiscontinuousSelection()
    {
        selectionActions[selection].Item1();
    }

    public void UseContinuousSelection()
    {
        selectionActions[selection].Item2();
    }

    public void UseRightSelection()
    {
        selectionActions[selection].Item3();
    }

    public void UseRightContinuousSelection()
    {
        selectionActions[selection].Item4();
    }
}
