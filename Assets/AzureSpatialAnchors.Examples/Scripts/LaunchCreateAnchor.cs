// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class LaunchCreateAnchor : MonoBehaviour
    {
        public Text SelectedSceneNameText;

        List<int> SceneBuildIndices = new List<int>();
        private int _SceneIndex = -1;
        int SceneIndex
        {
            get
            {
                return _SceneIndex;
            }
            set
            {
                if (_SceneIndex != value)
                {
                    _SceneIndex = value;
                }
            }
        }

#pragma warning disable CS1998 // Conditional compile statements are removing await
        async void Start()
#pragma warning restore CS1998
        {

            Debug.Log("STARTED LaunchGetNearbyAnchors");



            // if (SelectedSceneNameText == null)
            // {
            //     Debug.Log("Missing text field");
            //     return;
            // }

#if !UNITY_EDITOR && (UNITY_WSA || WINDOWS_UWP)
            // Ensure that the device is running a suported build with the spatialperception capability declared.
            bool accessGranted = false;
            try
            {
                Windows.Perception.Spatial.SpatialPerceptionAccessStatus accessStatus = await Windows.Perception.Spatial.SpatialAnchorExporter.RequestAccessAsync();
                accessGranted = (accessStatus == Windows.Perception.Spatial.SpatialPerceptionAccessStatus.Allowed);
            }
            catch {}

            if (!accessGranted)
            {
                Button[] buttons = GetComponentsInChildren<Button>();
                foreach (Button b in buttons)
                {
                    b.gameObject.SetActive(false);
                }

                SelectedSceneNameText.resizeTextForBestFit = true;
                SelectedSceneNameText.verticalOverflow = VerticalWrapMode.Overflow;
                SelectedSceneNameText.text = "Access denied to spatial anchor exporter.  Ensure your OS build is up to date and the spatialperception capability is set.";
                return;
            }
#endif

            // Debug.Log("calling get scenes");

            // GetScenes();

            // if (SceneBuildIndices.Count == 0)
            // {
            //     SelectedSceneNameText.text = "No scenes";
            //     Debug.Log("Not enough scenes in the build");
            //     return;
            // }

            // SceneIndex = 0;
        }

        public void LaunchScene()
        {


            string path = "Assets/AzureSpatialAnchors.Examples/Scenes/CreateAnchor.unity";
            SceneManager.LoadScene(path);


        }

    }
}
