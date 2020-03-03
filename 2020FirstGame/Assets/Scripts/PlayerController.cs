using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System;

public class PlayerController : MonoBehaviour
{
    float horizontalInputValue = 0f;
    float verticalInputValue = 0f;
    bool mouseDown = false;

    public float runSpeed;
    public float cameraSpeed;

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
            mouseDown = true;
            if (Input.GetMouseButtonUp(0))
            {
                mouseDown = false;
            }

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
                Debug.LogFormat("loading some terrain. coords from {0} to {1}", currentChunkCoords, nextChunkCoords);
                terrainController.GenerateTerrain(nextChunkCoords.Item1, nextChunkCoords.Item2);
                currentChunkCoords = nextChunkCoords;
            }
        }
    }

    private void MoveCamera()
    {
        Vector3 camPosition = cam.WorldToScreenPoint(myRigidbody2D.position);
        int horizontalCameraMove = 0;
        int verticalCameraMove = 0;
        bool cameraIsMove = false;

        if (camPosition.x < camThresholdLeft)
        {
            horizontalCameraMove = -1;
            //Debug.Log("cam move left");
            cameraIsMove = true;
        } 
        else if (camPosition.x > camThresholdRight)
        {
            horizontalCameraMove = 1;
            //Debug.Log("cam move right");
            cameraIsMove = true;
        }
        if (camPosition.y < camThresholdTop)
        {
            verticalCameraMove = -1;
            //Debug.Log("cam move down");
            cameraIsMove = true;
        }
        else if (camPosition.y > camThresholdBottom)
        {
            verticalCameraMove = 1;
            //Debug.Log("cam move up");
            cameraIsMove = true;
        }
        if (cameraIsMove)
        {
            cam.transform.position = new Vector3(cam.transform.position.x + horizontalCameraMove * cameraSpeed * 0.0004f, 
                                                cam.transform.position.y + verticalCameraMove * cameraSpeed * 0.0004f, -10);
        }
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
