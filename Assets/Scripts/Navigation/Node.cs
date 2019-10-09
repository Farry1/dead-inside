using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node : MonoBehaviour
{
    public List<Node> edges = new List<Node>();

    public float movementCost = 1;
    public bool canBeEntered = true;
    public TextMesh stepCounterText;
    public Renderer surfaceIndicator;
    public Renderer visibleSurfaceIndicator;

    public Material[] materials;


    private void Start()
    {
        stepCounterText.text = "0";
    }

    public void IndicateNavigation(int stepsRequired, int maxSteps)
    {
        stepCounterText.text = stepsRequired.ToString();

        if (stepsRequired <= maxSteps)
        {
            surfaceIndicator.gameObject.SetActive(true);            
            surfaceIndicator.material.color = Color.green;

            visibleSurfaceIndicator.gameObject.SetActive(true);
            visibleSurfaceIndicator.material = materials[0];
        }
        else
        {
            surfaceIndicator.gameObject.SetActive(false);
            visibleSurfaceIndicator.gameObject.SetActive(false);
        }
    }

    public void HighlightField(Color color)
    {
        surfaceIndicator.gameObject.SetActive(true);
        visibleSurfaceIndicator.material.color = color;

        visibleSurfaceIndicator.gameObject.SetActive(true);
        visibleSurfaceIndicator.material = materials[1];
    }

    public void HideNavigationIndicator()
    {
        surfaceIndicator.gameObject.SetActive(false);
        visibleSurfaceIndicator.gameObject.SetActive(false);

    }

    public float DistanceTo(Vector3 position)
    {
        return Vector3.Distance(transform.position, position);
    }
}
