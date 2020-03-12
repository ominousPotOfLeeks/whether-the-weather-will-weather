using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System;

public class PlayerController : MonoBehaviour
{
    private float horizontalInputValue = 0f;
    private float verticalInputValue = 0f;
    private bool mouseDown = false;
    private bool placingEntity = false;

    public float runSpeed;

    private Rigidbody2D myRigidbody2D;
    //private Transform myTransform;
    private float movementSmoothing = .02f;
    private Vector3 myVelocity = Vector3.zero;
    private Tuple<int, int> currentChunkCoords = new Tuple<int, int>(0, 0);

    public GameObject selection;
    public TerrainController terrainController;
    public Tilemap map;

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

    private void Start()
    {
        camThresholdLeft = camThresholdPercentHorizontal * Screen.width * 0.01f;
        camThresholdRight = (100 - camThresholdPercentHorizontal) * Screen.width * 0.01f;
        camThresholdTop = camThresholdPercentVertical * Screen.height * 0.01f;
        camThresholdBottom = (100 - camThresholdPercentVertical) * Screen.height * 0.01f;
    }

    void Awake()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        //myTransform = GetComponent<Transform>();
        cam = Camera.main;
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

        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            terrainController.isGenerated = false;
            terrainController.ClearMap(true);
        }

        //get movement inputs
        horizontalInputValue = Input.GetAxisRaw("Horizontal");
        verticalInputValue = Input.GetAxisRaw("Vertical");

        //mouse selection
        Vector3 mouseOnScreen = Input.mousePosition;
        Vector3 mouseInWorld = cam.ScreenToWorldPoint(mouseOnScreen);
        selection.transform.position = new Vector3(Mathf.RoundToInt(mouseInWorld.x),
            Mathf.RoundToInt(mouseInWorld.y), 
            selection.transform.position.z);

        //mouse click on coal
        if (Input.GetMouseButtonDown(0) || mouseDown)
        {
            //click on entities
            if (mouseDown == false)
            {
                OnClick();
            }

            mouseDown = true;
            if (Input.GetMouseButtonUp(0))
            {
                mouseDown = false;
            }

            OnClickContinuous(mouseInWorld);
        }
        
    }

    private void OnClickContinuous(Vector3 mouseInWorld)
    {
        //for events like dragging or colouring in where the mouse is still down

        int selectedTile = terrainController.GetTileAtPosition(mouseInWorld);

        if (selectedTile == terrainController.GetTileID("coal"))
        {
            terrainController.SetTileAtPosition(mouseInWorld, "dirt");
            //Debug.Log("coal");
        }
        else if (selectedTile == terrainController.GetTileID("rock"))
        {
            terrainController.SetTileAtPosition(mouseInWorld, "dirt");
            //Debug.Log("rock");
        }
    }

    private void OnClick()
    {
        //

        //Code to make clicking on something show its info
        /*Vector2 mousePos2D = new Vector2(mouseInWorld.x, mouseInWorld.y);
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        if (hit.collider != null)
        {
            Debug.Log(hit.collider.gameObject.name);
            if (hit.collider.gameObject.name == "Sheep(Clone)")
            {
                SheepScript sh = hit.collider.gameObject.GetComponent<EntityScript>();
                EntityController.Entity entity = sh.selfEntity;
                Debug.LogFormat("chunk: {0}, x:{1}, y:{2}, xychunk: {3}",
                    entity.chunk, hit.collider.gameObject.transform.position.x,
                    hit.collider.gameObject.transform.position.y,
                    terrainController.terrainArray.GetChunkCoords(Mathf.RoundToInt(hit.collider.gameObject.transform.position.x),
                    Mathf.RoundToInt(hit.collider.gameObject.transform.position.y)));
            }
        }//*/
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
