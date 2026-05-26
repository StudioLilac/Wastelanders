using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputChecker : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckUIElementsUnderMouse();
        }
    }

    private void CheckUIElementsUnderMouse()
    {
        if (EventSystem.current == null)
        {
            Debug.LogError("[InputChecker] NO EVENT SYSTEM IN SCENE! UI clicks will not work until you add one (GameObject -> UI -> Event System).");
            return;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            Debug.Log($"<color=cyan>[InputChecker]</color> Found {results.Count} UI elements under the mouse at {Input.mousePosition}:");

            for (int i = 0; i < results.Count; i++)
            {
                // The first item (i=0) is the one actively blocking everything behind it.
                string warning = (i == 0) ? " <color=red><-- THIS IS EATING THE CLICK!</color>" : "";

                Debug.Log($"   {i + 1}. <b>{results[i].gameObject.name}</b> (Depth: {results[i].depth}){warning}");
            }
        }
        else
        {
            Debug.Log("<color=yellow>[InputChecker]</color> Clicked, but hit 0 UI elements. (Click went straight into the 3D/2D game world).");
        }
    }
}