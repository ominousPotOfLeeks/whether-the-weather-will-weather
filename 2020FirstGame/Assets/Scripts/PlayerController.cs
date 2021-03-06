﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System;

public class PlayerController : MonoBehaviour
{
    private float horizontalInputValue = 0f;
    private float verticalInputValue = 0f;
    bool mouseDown = false;
    bool mouseRightDown = false;
    public bool doContinuousMouseActions = true;
    bool inInventory = false;

    public float runSpeed;

    private Rigidbody2D myRigidbody2D;
    //private Transform myTransform;
    private float movementSmoothing = .02f;
    private Vector3 myVelocity = Vector3.zero;
    private Tuple<int, int> currentChunkCoords = new Tuple<int, int>(0, 0);

    public GameObject cursorSelection;
    public TerrainController terrainController;
    public HotbarController hotbarController;
    public InventoryController inventoryController;
    public Tilemap tilesInventory;
    public Tilemap map;
    public Text startText;

    [Range(0, 100)]
    public int camThresholdPercentHorizontal; //percentage of screen width from left edge
    [Range(0, 100)]
    public int camThresholdPercentVertical; //percentage of screen height from top edge which the camera will move before the player enters

    private float camThresholdLeft;
    private float camThresholdRight;
    private float camThresholdTop;
    private float camThresholdBottom;

    private Camera cam;

    public int gridUnit;

    private CursorScript cursorScript;

    private void Start()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        cursorSelection.GetComponent<SpriteRenderer>().enabled = false;

        myRigidbody2D = GetComponent<Rigidbody2D>();
        //myTransform = GetComponent<Transform>();
        cursorScript = cursorSelection.GetComponent<CursorScript>();
        InitializeCamera();
    }

    /// <summary>
    /// Converts settings for camera into positions on screen to make a bounding box, (which the player is kept within
    /// by moving the camera)
    /// </summary>
    void InitializeCamera()
    {
        cam = Camera.main;
        camThresholdLeft = camThresholdPercentHorizontal * Screen.width * 0.01f;
        camThresholdRight = (100 - camThresholdPercentHorizontal) * Screen.width * 0.01f;
        camThresholdTop = camThresholdPercentVertical * Screen.height * 0.01f;
        camThresholdBottom = (100 - camThresholdPercentVertical) * Screen.height * 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        //start game
        if (Input.GetKeyDown(KeyCode.Space))
        {
            terrainController.GenerateTerrain(0, 0);
            terrainController.isGenerated = true;
            currentChunkCoords = terrainController.terrainArray.GetChunkCoords(Mathf.RoundToInt(myRigidbody2D.position.x), Mathf.RoundToInt(myRigidbody2D.position.y));
            hotbarController.ToggleVisible();
            GetComponent<SpriteRenderer>().enabled = true;
            cursorSelection.GetComponent<SpriteRenderer>().enabled = true;
            startText.GetComponent<Text>().enabled = false;
        }

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

        if (terrainController.isGenerated)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                inventoryController.ToggleInventory();
                inInventory = !inInventory;
            }

            //get movement inputs
            horizontalInputValue = Input.GetAxisRaw("Horizontal");
            verticalInputValue = Input.GetAxisRaw("Vertical");

            //mouse selection
            cursorScript.UpdateCursorPosition();

            //mouse left click
            if (Input.GetMouseButtonDown(0) || mouseDown)
            {
                if (mouseDown == false)
                {
                    doContinuousMouseActions = true;
                    OnClick();
                }

                mouseDown = true;
                if (Input.GetMouseButtonUp(0))
                {
                    mouseDown = false;
                }

                if (doContinuousMouseActions)
                {
                    OnClickContinuous();
                }
            }

            //mouse right click
            if (Input.GetMouseButtonDown(1) || mouseRightDown)
            {
                if (mouseRightDown == false)
                {
                    OnRightClick();
                }

                mouseRightDown = true;
                if (Input.GetMouseButtonUp(1))
                {
                    mouseRightDown = false;
                }

                OnRightClickContinuous();
            }

            float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
            if (scrollAmount > 0f)
            {
                hotbarController.ScrollSelection(false);
            }
            else if (scrollAmount < 0f)
            {
                hotbarController.ScrollSelection(true);
            }
        }

        //Check if player has entered different chunk
        if (myRigidbody2D.velocity != Vector2.zero)
        {
            MoveCamera();
            Tuple<int, int> nextChunkCoords = terrainController.terrainArray.GetChunkCoords(Mathf.RoundToInt(myRigidbody2D.position.x), Mathf.RoundToInt(myRigidbody2D.position.y));
            if (!terrainController.ChunksEqual(nextChunkCoords, currentChunkCoords))
            {
                //update terrain
                //terrainController.GenerateTerrain(x, y);
                //Debug.LogFormat("loading some terrain. coords from {0} to {1}", currentChunkCoords, nextChunkCoords);
                terrainController.GenerateTerrain(nextChunkCoords.Item1, nextChunkCoords.Item2);
                currentChunkCoords = nextChunkCoords;
            }
        }
    }

    /// <summary>
    /// When right mouse becomes clicked, this function is triggered for one frame
    /// </summary>
    private void OnRightClick()
    {
        if (inInventory)
        {

        } 
        else
        {
            hotbarController.UseRightSelection();
        }
        
    }

    /// <summary>
    /// While right mouse is down, this function is triggered once per frame
    /// </summary>
    private void OnRightClickContinuous()
    {
        if (inInventory)
        {

        }
        else
        {
            hotbarController.UseRightContinuousSelection();
        }
    }

    /// <summary>
    /// When left mouse becomes clicked, this function is triggered for one frame. 
    /// For events like dragging or colouring in where the mouse is still down
    /// </summary>
    private void OnClickContinuous()
    {
        if (inInventory)
        {
            //do inventory controls
        }
        else
        {
            hotbarController.UseContinuousSelection();
        }

    }
    /// <summary>
    /// When left mouse becomes clicked, this function is triggered for one frame
    /// </summary>
    private void OnClick()
    {
        if (inInventory)
        {

        }
        else
        {
            hotbarController.UseDiscontinuousSelection();
        }
    }

    void FixedUpdate()
    {
        if (terrainController.isGenerated)
        {
            Vector3 targetVelocity = new Vector2(horizontalInputValue * runSpeed * Time.fixedDeltaTime,
            verticalInputValue * runSpeed * Time.fixedDeltaTime);

            myRigidbody2D.velocity = Vector3.SmoothDamp(myRigidbody2D.velocity, targetVelocity, ref myVelocity, movementSmoothing);
            //myRigidbody2D.velocity = targetVelocity;
            //myTransform.transform.Translate(targetVelocity);
        }
    }

    /// <summary>
    /// Moves camera if player is outside of camera bounding box
    /// </summary>
    private void MoveCamera()
    {
        Vector3 playerPosition = cam.WorldToScreenPoint(myRigidbody2D.position);
        Vector3 newPosition = cam.WorldToScreenPoint(cam.transform.position);

        if (playerPosition.x < camThresholdLeft)
        {
            //Debug.Log("cam move left");
            newPosition.x -= camThresholdLeft - playerPosition.x;
        } 
        else if (playerPosition.x > camThresholdRight)
        {
            //Debug.Log("cam move right");
            newPosition.x -= camThresholdRight - playerPosition.x;
        }
        if (playerPosition.y < camThresholdTop)
        {
            //Debug.Log("cam move down");
            newPosition.y -= camThresholdTop - playerPosition.y;
        }
        else if (playerPosition.y > camThresholdBottom)
        {
            //Debug.Log("cam move up");
            newPosition.y -= camThresholdBottom - playerPosition.y;
        }
        cam.transform.position = cam.ScreenToWorldPoint(newPosition);
    }
}
