// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.XR.ARFoundation;

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class GetNearbyAnchors : DemoScriptBase
    {

        internal enum AppState
        {
            DemoStepCreateSession = 0,
            DemoStepConfigSession,
            DemoStepStartSession,
            DemoStepCreateLocationProvider,
            DemoStepConfigureSensors,
            DemoStepCreateLocalAnchor,
            DemoStepSaveCloudAnchor,
            DemoStepSavingCloudAnchor,
            DemoStepStopSession,
            DemoStepDestroySession,
            DemoStepCreateSessionForQuery,
            DemoStepStartSessionForQuery,
            DemoStepLookForAnchorsNearDevice,
            DemoStepLookingForAnchorsNearDevice,
            DemoStepStopWatcher,
            DemoStepStopSessionForQuery,
            DemoStepComplete,

            CreateSessionState,


            QueryState



        }

        private readonly Dictionary<AppState, DemoStepParams> stateParams = new Dictionary<AppState, DemoStepParams>
        {
            { AppState.DemoStepCreateSession,new DemoStepParams() { StepMessage = "Next: Create Azure Spatial Anchors Session", StepColor = Color.clear }},
            { AppState.DemoStepConfigSession,new DemoStepParams() { StepMessage = "Next: Configure Azure Spatial Anchors Session", StepColor = Color.clear }},
            { AppState.DemoStepStartSession,new DemoStepParams() { StepMessage = "Next: Start Azure Spatial Anchors Session", StepColor = Color.clear }},
            { AppState.DemoStepCreateLocationProvider,new DemoStepParams() { StepMessage = "Next: Create Location Provider", StepColor = Color.clear }},
            { AppState.DemoStepConfigureSensors,new DemoStepParams() { StepMessage = "Next: Configure Sensors", StepColor = Color.clear }},
            { AppState.DemoStepCreateLocalAnchor,new DemoStepParams() { StepMessage = "Tap a surface to add the Local Anchor.", StepColor = Color.blue }},
            { AppState.DemoStepSaveCloudAnchor,new DemoStepParams() { StepMessage = "Next: Save Local Anchor to cloud", StepColor = Color.yellow }},
            { AppState.DemoStepSavingCloudAnchor,new DemoStepParams() { StepMessage = "Saving local Anchor to cloud...", StepColor = Color.yellow }},
            { AppState.DemoStepStopSession,new DemoStepParams() { StepMessage = "Next: Stop Azure Spatial Anchors Session", StepColor = Color.green }},
            { AppState.DemoStepCreateSessionForQuery,new DemoStepParams() { StepMessage = "Next: Create Azure Spatial Anchors Session for query", StepColor = Color.clear }},
            { AppState.DemoStepStartSessionForQuery,new DemoStepParams() { StepMessage = "Next: Start Azure Spatial Anchors Session for query", StepColor = Color.clear }},
            { AppState.DemoStepLookForAnchorsNearDevice,new DemoStepParams() { StepMessage = "Next: Look for Anchors near device", StepColor = Color.clear }},
            { AppState.DemoStepLookingForAnchorsNearDevice,new DemoStepParams() { StepMessage = "Looking for Anchors near device...", StepColor = Color.clear }},
            { AppState.DemoStepStopWatcher,new DemoStepParams() { StepMessage = "Next: Stop Watcher", StepColor = Color.yellow }},
            { AppState.DemoStepStopSessionForQuery,new DemoStepParams() { StepMessage = "Next: Stop Azure Spatial Anchors Session for query", StepColor = Color.grey }},
            { AppState.DemoStepComplete,new DemoStepParams() { StepMessage = "Next: Restart demo", StepColor = Color.clear }},

            //New States
            { AppState.CreateSessionState, new DemoStepParams() { StepMessage = "Next: Scan surrounding area and then press next to find anchors", StepColor = Color.green}},
            { AppState.QueryState, new DemoStepParams() { StepMessage = "Next: Finding Anchors", StepColor = Color.yellow}}
        };

        // private AppState _currentAppState = AppState.DemoStepCreateSession;
        private AppState _currentAppState = AppState.CreateSessionState;
        

        AppState currentAppState
        {
            get
            {
                return _currentAppState;
            }
            set
            {
                if (_currentAppState != value)
                {
                    Debug.LogFormat("State from {0} to {1}", _currentAppState, value);
                    _currentAppState = value;
                    if (spawnedObjectMat != null)
                    {
                        spawnedObjectMat.color = stateParams[_currentAppState].StepColor;
                    }

                    if (!isErrorActive)
                    {
                        feedbackBox.text = stateParams[_currentAppState].StepMessage;
                    }
                    // EnableCorrectUIControls();
                }
            }
        }

        private PlatformLocationProvider locationProvider;
        private List<GameObject> allDiscoveredAnchors = new List<GameObject>();


        public void SavePreferences()
        {
            // PlayerPrefs.SaveString;
        }
        private void EnableCorrectUIControls()
        {
            int nextButtonIndex = 0;
            int enumerateButtonIndex = 2;

            switch (currentAppState)
            {
                case AppState.DemoStepCreateLocalAnchor:
                case AppState.DemoStepSavingCloudAnchor:
                case AppState.DemoStepLookingForAnchorsNearDevice:
#if WINDOWS_UWP || UNITY_WSA
                    // Sample disables "Next step" button on Hololens, so it doesn't overlay with placing the anchor and async operations, 
                    // which are not affected by user input.
                    // This is also part of a workaround for placing anchor interaction, which doesn't receive callback when air tapping for placement
                    // This is not applicable to Android/iOS versions.
                    XRUXPicker.Instance.GetDemoButtons()[nextButtonIndex].gameObject.SetActive(false);
#endif
                    break;
                case AppState.DemoStepStopSessionForQuery:
                    XRUXPicker.Instance.GetDemoButtons()[enumerateButtonIndex].gameObject.SetActive(true);
                    break;
                default:
                    XRUXPicker.Instance.GetDemoButtons()[nextButtonIndex].gameObject.SetActive(true);
                    XRUXPicker.Instance.GetDemoButtons()[enumerateButtonIndex].gameObject.SetActive(false);
                    break;
            }
        }

        public SensorStatus GeoLocationStatus
        {
            get
            {
                if (locationProvider == null)
                    return SensorStatus.MissingSensorFingerprintProvider;
                if (!locationProvider.Sensors.GeoLocationEnabled)
                    return SensorStatus.DisabledCapability;
                switch (locationProvider.GeoLocationStatus)
                {
                    case GeoLocationStatusResult.Available:
                        return SensorStatus.Available;
                    case GeoLocationStatusResult.DisabledCapability:
                        return SensorStatus.DisabledCapability;
                    case GeoLocationStatusResult.MissingSensorFingerprintProvider:
                        return SensorStatus.MissingSensorFingerprintProvider;
                    case GeoLocationStatusResult.NoGPSData:
                        return SensorStatus.NoData;
                    default:
                        return SensorStatus.MissingSensorFingerprintProvider;
                }
            }
        }

        public SensorStatus WifiStatus
        {
            get
            {
                if (locationProvider == null)
                    return SensorStatus.MissingSensorFingerprintProvider;
                if (!locationProvider.Sensors.WifiEnabled)
                    return SensorStatus.DisabledCapability;
                switch (locationProvider.WifiStatus)
                {
                    case WifiStatusResult.Available:
                        return SensorStatus.Available;
                    case WifiStatusResult.DisabledCapability:
                        return SensorStatus.DisabledCapability;
                    case WifiStatusResult.MissingSensorFingerprintProvider:
                        return SensorStatus.MissingSensorFingerprintProvider;
                    case WifiStatusResult.NoAccessPointsFound:
                        return SensorStatus.NoData;
                    default:
                        return SensorStatus.MissingSensorFingerprintProvider;
                }
            }
        }

        public SensorStatus BluetoothStatus
        {
            get
            {
                if (locationProvider == null)
                    return SensorStatus.MissingSensorFingerprintProvider;
                if (!locationProvider.Sensors.BluetoothEnabled)
                    return SensorStatus.DisabledCapability;
                switch (locationProvider.BluetoothStatus)
                {
                    case BluetoothStatusResult.Available:
                        return SensorStatus.Available;
                    case BluetoothStatusResult.DisabledCapability:
                        return SensorStatus.DisabledCapability;
                    case BluetoothStatusResult.MissingSensorFingerprintProvider:
                        return SensorStatus.MissingSensorFingerprintProvider;
                    case BluetoothStatusResult.NoBeaconsFound:
                        return SensorStatus.NoData;
                    default:
                        return SensorStatus.MissingSensorFingerprintProvider;
                }
            }
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any
        /// of the Update methods are called the first time.
        /// </summary>


        protected Text debugText;
        protected Text anchorIDText;
        protected Text planeVisualizerText;
        protected Text scanText;
        protected Text startText;
        protected GameObject moveButton;
        protected Text moveButtonText;
        protected Image moveButtonImage;


        public GameObject ARPlaneVisualizer;



        public override void Start()
        {
            Debug.Log(">>Azure Spatial Anchors Demo Script Start");

            try {
            anchorIDText = GameObject.Find("/UXParent/MobileUX/anchorIdText").GetComponent<Text>();
            scanText = GameObject.Find("/UXParent/MobileUX/scanText").GetComponent<Text>();
            debugText = GameObject.Find("/UXParent/MobileUX/debugText").GetComponent<Text>();
            planeVisualizerText = GameObject.Find("/UXParent/MobileUX/planeVisualizerText").GetComponent<Text>();
            startText = GameObject.Find("/UXParent/MobileUX/startText").GetComponent<Text>();
            moveButton = GameObject.Find("/UXParent/MobileUX/Button");
            moveButtonText = GameObject.Find("/UXParent/MobileUX/moveButtonText").GetComponent<Text>();
            // moveButtonImage = GameObject.Find("/UXParent/MobileUX/Button").GetComponent<Image>();
            } catch (Exception e) {
                debugText.text = e.ToString();
            }


            debugText.text = "Get Nearby Anchors Script";

            base.Start();

            if (!SanityCheckAccessConfiguration())
            {
                return;
            }
            feedbackBox.text = stateParams[currentAppState].StepMessage;

            Debug.Log("Azure Spatial Anchors Demo script started");

            enableAdvancingOnSelect = false;

            // EnableCorrectUIControls();

            

            planeVisualizerText.text = currentAppState.ToString();

            debugText.text = "finished start";

            scanText.enabled = false;
            // moveButton.enabled = true;
            // moveButtonText.enabled = true;
            // moveButtonImage.enabled = true;


        }

        protected override void OnCloudAnchorLocated(AnchorLocatedEventArgs args)
        {

            base.OnCloudAnchorLocated(args);

            //######### SOME DEBUGGING
            CloudSpatialAnchor currentCloudAnchor = args.Anchor;
            String currentAnchorId = currentCloudAnchor.Identifier;
            // debugText.text = ("ANCHOR LOCATED: " + currentAnchorId);

            anchorIDText.text = ("Located Anchor ID: " + currentAnchorId);

            if (args.Status == LocateAnchorStatus.Located)
            {
                Debug.Log("args.Stats == LocateAnchorStatus.Located");
                CloudSpatialAnchor cloudAnchor = args.Anchor;

                // debugText.text = "Pre Destroy Plane";
                // debugText.text = "Post Destroy Plane";

                UnityDispatcher.InvokeOnAppThread(() =>
                {
                    currentAppState = AppState.DemoStepStopWatcher;
                    debugText.text = "current state DemoStopWatcher";
                    Pose anchorPose = Pose.identity;

#if UNITY_ANDROID || UNITY_IOS
                    anchorPose = currentCloudAnchor.GetPose();
#endif

                    debugText.text = "Spawning Anchor";
                    // HoloLens: The position will be set based on the unityARUserAnchor that was located.
                    GameObject spawnedObject = SpawnNewAnchoredObject(anchorPose.position, anchorPose.rotation, currentCloudAnchor);
                    allDiscoveredAnchors.Add(spawnedObject);
                });
            }




            // Despawn stuff from scene

            planeVisualizerText.enabled = false;
            debugText.enabled = false;
            scanText.enabled = false;



        }

        public void OnApplicationFocus(bool focusStatus)
        {
#if UNITY_ANDROID
            // We may get additional permissions at runtime. Enable the sensors once app is resumed
            if (focusStatus && locationProvider != null)
            {
                ConfigureSensors();
            }
#endif
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (spawnedObjectMat != null)
            {
                float rat = 0.1f;
                float createProgress = 0f;
                if (CloudManager.SessionStatus != null)
                {
                    createProgress = CloudManager.SessionStatus.RecommendedForCreateProgress;
                }
                rat += (Mathf.Min(createProgress, 1) * 0.9f);
                spawnedObjectMat.color = GetStepColor() * rat;
            }
        }

        protected override bool IsPlacingObject()
        {
            return currentAppState == AppState.DemoStepCreateLocalAnchor;
        }

        protected override Color GetStepColor()
        {
            return stateParams[currentAppState].StepColor;
        }

        //         protected override async Task OnSaveCloudAnchorSuccessfulAsync()
        //         {
        //             await base.OnSaveCloudAnchorSuccessfulAsync();

        //             Debug.Log("Anchor created, yay!");

        //             // Sanity check that the object is still where we expect
        //             Pose anchorPose = Pose.identity;

        // #if UNITY_ANDROID || UNITY_IOS
        //             anchorPose = currentCloudAnchor.GetPose();
        // #endif
        //             // HoloLens: The position will be set based on the unityARUserAnchor that was located.

        //             SpawnOrMoveCurrentAnchoredObject(anchorPose.position, anchorPose.rotation);

        //             currentAppState = AppState.DemoStepStopSession;
        //         }

        protected override void OnSaveCloudAnchorFailed(Exception exception)
        {
            base.OnSaveCloudAnchorFailed(exception);
        }


        public void UpdateUISearch()
        {
            debugText.text = "Enabling scan text";
            scanText.enabled = true;
            startText.enabled = false;
            moveButton.SetActive(false);
            // moveButton.enabled = false;
            // moveButtonText.enabled = false;
            // moveButtonImage.enabled = false;
            debugText.text = "closing ui update";
        }

        public async override Task AdvanceDemoAsync()
        {
            switch (currentAppState)
            {

                case AppState.CreateSessionState:


                    planeVisualizerText.text = currentAppState.ToString();


                    debugText.text = "Creating Session";
                    //Create Session
                    if (CloudManager.Session == null)
                    {
                        await CloudManager.CreateSessionAsync();
                    }
                    currentCloudAnchor = null;
                    
                    // scanText.enabled = true;
                    // moveButton.enabled = false;

                    UpdateUISearch();

                    debugText.text = "Configure Session";
                    //Config Session
                    ConfigureSession();

                    debugText.text = "Demo Step Start Session";
                    // Demo Step Start Session
                    await CloudManager.StartSessionAsync();
                    currentAppState = AppState.DemoStepCreateLocationProvider;


                    debugText.text = "Create Location Provider";
                    // Demo Step Create Location Provider
                    locationProvider = new PlatformLocationProvider();
                    CloudManager.Session.LocationProvider = locationProvider;

                    debugText.text = "Configuring Sensors";
                    // Demo Step Configure Sensors
                    SensorPermissionHelper.RequestSensorPermissions();
                    ConfigureSensors();

                    // Enable advancing to next step on Air Tap, which is an easier interaction for placing the anchor.
                    // (placing the anchor with Air tap automatically advances the demo).
                    enableAdvancingOnSelect = true;



                    debugText.text = "Creating sessions for query";
                    //Create Session
                    ConfigureSession();
                    locationProvider = new PlatformLocationProvider();
                    CloudManager.Session.LocationProvider = locationProvider;
                    ConfigureSensors();

                    debugText.text = "Starting session for query";
                    //Start session for query
                    await CloudManager.StartSessionAsync();

                    debugText.text = "looking for anchors";
                    //Looking for anchors
                    currentWatcher = CreateWatcher();

                    debugText.text = "Trying to remove plane";
                    RemoveARPlane();

                    debugText.text = "Plane should be gone";


                    currentAppState = AppState.QueryState;

                    currentAppState = AppState.DemoStepLookingForAnchorsNearDevice;

                    currentAppState = AppState.DemoStepStopSessionForQuery;
                    planeVisualizerText.text = currentAppState.ToString();






                    break;




                // case AppState.QueryState:
                



                //     break;

                // case AppState.DemoStepLookForAnchorsNearDevice:
                //     currentAppState = AppState.DemoStepLookingForAnchorsNearDevice;
                //     break;
                // case AppState.DemoStepLookingForAnchorsNearDevice:
                //     break;
                // case AppState.DemoStepStopWatcher:
                //     if (currentWatcher != null)
                //     {
                //         currentWatcher.Stop();
                //         currentWatcher = null;
                //     }
                //     currentAppState = AppState.DemoStepStopSessionForQuery;
                //     break;
                case AppState.DemoStepStopSessionForQuery:

                    planeVisualizerText.text = currentAppState.ToString();

                    CloudManager.StopSession();
                    currentWatcher = null;
                    locationProvider = null;
                    currentAppState = AppState.DemoStepComplete;
                    break;
                case AppState.DemoStepComplete:
                    currentCloudAnchor = null;
                    currentAppState = AppState.DemoStepCreateSession;
                    CleanupSpawnedObjects();
                    break;
                default:
                    Debug.Log("Shouldn't get here for app state " + currentAppState.ToString());
                    break;
            }
        }

        public void RemoveARPlane()
        {
            // ARPlaneVisualizer.Destroy();
            // Destroy(ARPlaneVisualizer);
            // var meshRenderer = ARPlaneVisualizer.GetComponent<MeshRenderer>();

            // meshRenderer.enabled = false;


            // var lineRenderer = ARPlaneVisualizer.GetComponent<LineRenderer>();
            // lineRenderer.enabled = false;


            // var planeMeshVisualizerScript = ARPlaneVisualizer.GetComponent<ARPlaneMeshVisualizer>();

            // planeMeshVisualizerScript.DisableComponents();


            // var arPlane = ARPlaneVisualizer.GetComponent<ARPlane>();

            // Destroy(arPlane);

            // ARPlaneVisualizer.SetActive(false);
            // planeVisualizerText.text = "check numero 2 (false): " + ARPlaneVisualizer.activeSelf.ToString();

            // AR Plane Mesh Visualizer Script
            // ARPlaneVisualizer.GetComponent<ARPlaneMeshVisualizer>().enabled = false;

            // // // Line Renderer
            // ARPlaneVisualizer.GetComponent<LineRenderer>().enabled = false;

            // // // Mesh Renderer
            // ARPlaneVisualizer.GetComponent<MeshRenderer>().enabled = false;


        }
        


        public async override Task EnumerateAllNearbyAnchorsAsync()
        {
            Debug.Log("Enumerating near-device spatial anchors in the cloud");

            NearDeviceCriteria criteria = new NearDeviceCriteria();
            criteria.DistanceInMeters = 5;
            criteria.MaxResultCount = 20;

            var cloudAnchorSession = CloudManager.Session;

            var spatialAnchorIds = await cloudAnchorSession.GetNearbyAnchorIdsAsync(criteria);

            Debug.LogFormat("Got ids for {0} anchors", spatialAnchorIds.Count);
            debugText.text = "Got ids for {0} anchors" + spatialAnchorIds.Count;

            List<CloudSpatialAnchor> spatialAnchors = new List<CloudSpatialAnchor>();

            foreach (string anchorId in spatialAnchorIds)
            {
                var anchor = await cloudAnchorSession.GetAnchorPropertiesAsync(anchorId);
                Debug.LogFormat("Received information about spatial anchor {0}", anchor.Identifier);
                spatialAnchors.Add(anchor);
            }

            feedbackBox.text = $"Found {spatialAnchors.Count} anchors nearby";
        }

        protected override void CleanupSpawnedObjects()
        {
            base.CleanupSpawnedObjects();

            foreach (GameObject anchor in allDiscoveredAnchors)
            {
                Destroy(anchor);
            }
            allDiscoveredAnchors.Clear();
        }

        private void ConfigureSession()
        {
            const float distanceInMeters = 8.0f;
            const int maxAnchorsToFind = 25;
            SetNearDevice(distanceInMeters, maxAnchorsToFind);
        }

        private void ConfigureSensors()
        {
            locationProvider.Sensors.GeoLocationEnabled = SensorPermissionHelper.HasGeoLocationPermission();

            locationProvider.Sensors.WifiEnabled = SensorPermissionHelper.HasWifiPermission();

            locationProvider.Sensors.BluetoothEnabled = SensorPermissionHelper.HasBluetoothPermission();
            locationProvider.Sensors.KnownBeaconProximityUuids = CoarseRelocSettings.KnownBluetoothProximityUuids;
        }
    }
}
