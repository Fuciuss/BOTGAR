using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.ARFoundation;
public class MainScript : MonoBehaviour
{


    public GameObject ARPlane;


    // public GameObject ARPlaneVisualizer;


    // Start is called before the first frame update
    void Start()
    {
        

        //Grab the prefab somehow

        Debug.Log("Starting Main Script");

        // ARPlane.GetComponentsInChildren();


                    // AR Plane Mesh Visualizer Script
            var mesh = GameObject.Find("ARPlaneVisualizer").GetComponent<ARPlaneMeshVisualizer>();
            // mesh.enabled = false;
            mesh.enabled = true;

            // Line Renderer
            // var lines = ARPlaneVisualizer.GetComponent<LineRenderer>();
            // // lines.enabled = false;
            // lines.enabled = true;

            // // Mesh Renderer
            // ARPlaneVisualizer.GetComponent<MeshRenderer>().enabled = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
