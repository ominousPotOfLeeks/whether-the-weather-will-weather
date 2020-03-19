using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarController : MonoBehaviour
{
    private int selection = 0;
    private int numSelections = 5;

    private SpriteRenderer spriteRenderer;
    public EntityController entityController;
    public TerrainController terrainController;
    public GameObject cursorSelection;

    public Sprite[] selectionSprites;
    private Tuple<Action, Action>[] selectionActions;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = selectionSprites[0];

        //Item1 is on entering mousedown state, Item2 is repeated every frame while the mouse is down
        selectionActions = new Tuple<Action, Action>[]
        {
            new Tuple<Action, Action> (ToggleUnderCursor,           DoNothing),
            new Tuple<Action, Action> (DoNothing,                   () => ReplaceTile("dirt")),
            new Tuple<Action, Action> (() => PlaceEntity("miner"),  DoNothing),
            new Tuple<Action, Action> (DoNothing,                   () => PlaceEntity("sheep")),
            new Tuple<Action, Action> (DoNothing,                   () => ReplaceTile("rock"))
        };
    }

    public void DoNothing()
    {
        //just easier than a bunch of nulls and if statements
    }

    public void ToggleUnderCursor()
    {
        GameObject obj = entityController.GetEntityAtPosition(cursorSelection.transform.position);
        if (obj != null)
        {
            ToggleableScript ts;
            if ((ts = obj.GetComponent<ToggleableScript>()) != null)
            {
                ts.Toggle();
            }
        }
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

    public void UseDiscontinuousSelection()
    {
        selectionActions[selection].Item1();
    }

    public void UseContinuousSelection()
    {
        selectionActions[selection].Item2();
    }
}
