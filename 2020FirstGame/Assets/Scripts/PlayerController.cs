using System.Collections;
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
        myRigidbody2D = GetComponent<Rigidbody2D>();
        //myTransform = GetComponent<Transform>();
        cursorScript = cursorSelection.GetComponent<CursorScript>();
        cam = Camera.main;
        camThresholdLeft = camThresholdPercentHorizontal * Screen.width * 0.01f;
        camThresholdRight = (100 - camThresholdPercentHorizontal) * Screen.width * 0.01f;
        camThresholdTop = camThresholdPercentVertical * Screen.height * 0.01f;
        camThresholdBottom = (100 - camThresholdPercentVertical) * Screen.height * 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        //game state controls
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log("space pressed");
            terrainController.GenerateTerrain(0, 0);
            terrainController.isGenerated = true;
            currentChunkCoords = terrainController.terrainArray.GetChunkCoords(Mathf.RoundToInt(myRigidbody2D.position.x), Mathf.RoundToInt(myRigidbody2D.position.y));
            hotbarController.ToggleVisible();
            GetComponent<SpriteRenderer>().enabled = true;
            startText.GetComponent<Text>().enabled = false;
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            //this probably doesn't work and I don't use it
            terrainController.isGenerated = false;
            terrainController.ClearMap(true);
        }
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

    private void OnClickContinuous()
    {
        //for events like dragging or colouring in where the mouse is still down
        if (inInventory)
        {
            //do inventory controls
        }
        else
        {
            hotbarController.UseContinuousSelection();
        }

    }

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
        
        Vector3 targetVelocity = new Vector2(horizontalInputValue * runSpeed * Time.fixedDeltaTime,
            verticalInputValue * runSpeed * Time.fixedDeltaTime);

        myRigidbody2D.velocity = Vector3.SmoothDamp(myRigidbody2D.velocity, targetVelocity, ref myVelocity, movementSmoothing);
        //myRigidbody2D.velocity = targetVelocity;
        //myTransform.transform.Translate(targetVelocity);

        MoveCamera();
        //Check if player has entered different chunk
        if (terrainController.isGenerated && targetVelocity != new Vector3(0f, 0f, 0f))
        {
            Tuple<int, int> nextChunkCoords = terrainController.terrainArray.GetChunkCoords(Mathf.RoundToInt(myRigidbody2D.position.x), Mathf.RoundToInt(myRigidbody2D.position.y));
            if (!(nextChunkCoords.Item1 == currentChunkCoords.Item1 && nextChunkCoords.Item2 == currentChunkCoords.Item2))
            {
                //update terrain
                //terrainController.GenerateTerrain(x, y);
                //Debug.LogFormat("loading some terrain. coords from {0} to {1}", currentChunkCoords, nextChunkCoords);
                terrainController.GenerateTerrain(nextChunkCoords.Item1, nextChunkCoords.Item2);
                currentChunkCoords = nextChunkCoords;
            }
        }
    }

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


    /*bool AreWeOnGrid()
    {
        return IsThisVectorOnTheGrid(transform.position - new Vector3(0.5f, 0.5f, 0.0f));
    }

    bool IsThisVectorOnTheGrid(Vector3 myVector3)
    {
        return IsThisInteger(myVector3.x) && IsThisInteger(myVector3.y);
    }

    bool IsThisInteger(float myFloat)
    {
        return Mathf.Approximately(Mathf.RoundToInt(myFloat * gridPrecision) / gridPrecision, Mathf.RoundToInt(myFloat));
    }*/
}
