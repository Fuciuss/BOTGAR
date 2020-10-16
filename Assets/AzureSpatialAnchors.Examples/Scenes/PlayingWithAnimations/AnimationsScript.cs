using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationsScript : MonoBehaviour
{
    // Start is called before the first frame update

    // public Canvas prefab;
    void Start()
    {
        // prefab.CanvasRenderer.SetAlpha(0.0f);


    }

    // Update is called once per frame
    void Update()
    {

    }

    public void fadeIn()
    {
        // prefab.CrossFadeAlpha(1, 2, false);
    }
    public Transform spawnPrefab;
    public GameObject go;
    public void spawnObject()
    {

        // Debug.Log("Gonna do an animation, gonna be rad");

        // GameObject newThing = Instantiate(spawnPrefab, new Vector3(0,0,0));
        Instantiate(go);


    }
}
