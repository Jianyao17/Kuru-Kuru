using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenToWorldPoint : MonoBehaviour
{
    private Camera _camera;

    private void Start()
    {
       _camera = Camera.main;
    }

    void OnGUI()
    {
        Vector3 point = new Vector3();
        Event currentEvent = Event.current;
        Vector2 mousePos = new Vector2();

        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        mousePos.x = currentEvent.mousePosition.x;
        mousePos.y = _camera.pixelHeight - currentEvent.mousePosition.y;
        
        point = _camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _camera.nearClipPlane));

        GUILayout.BeginArea(new Rect(20, 20, 250, 120));
        GUILayout.Label("Screen pixels: " + _camera.pixelWidth + ":" + _camera.pixelHeight);
        GUILayout.Label("Mouse position: " + mousePos);
        GUILayout.Label("World position: " + point.ToString("F3"));
        GUILayout.EndArea();
    }
}
