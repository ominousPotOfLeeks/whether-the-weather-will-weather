using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorScript : MonoBehaviour
{
    public Vector3 mouseWorldPosition;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    public void UpdateCursorPosition()
    {
        Vector3 mouseOnScreen = Input.mousePosition;
        mouseWorldPosition = cam.ScreenToWorldPoint(mouseOnScreen);
        transform.position = new Vector3(Mathf.RoundToInt(mouseWorldPosition.x),
            Mathf.RoundToInt(mouseWorldPosition.y),
            transform.position.z);
    }
}
