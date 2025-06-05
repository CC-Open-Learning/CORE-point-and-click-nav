using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using VARLab.CORECinema;

namespace VARLab.Navigation.PointClick
{
    /// <summary>
    /// This class is responsible for controlling the player movement and player related cameras.
    /// </summary>
    [RequireComponent(typeof(WaypointHandler))]
    public class PointClickNavigation : MonoBehaviour
    {
        /// <summary>
        ///     Defines a platform-specific camera panning sensitivity modifier.
        /// </summary>
        /// <remarks>
        ///     This is due to the fact that the WebGL player seems to nearly quadruple 
        ///     the magnitude of mouse X and Y inputs as the cursor moves, so the 
        ///     sensitivity needs to be reduced by that factor.
        /// </remarks>
        public float PlatformSensitivityModifier =>
            Application.platform == RuntimePlatform.WebGLPlayer
            ? 0.25f
            : 1f;

        private const float NoInput = 0f;
        private const float ZeroDistance = 0f;

        // Fields

        [SerializeField, Tooltip("The target to look at after reaching a waypoint. " +
            "The player will look straight upon reaching a waypoint if this field is null.")]
        private Transform lookAtTarget;

        [SerializeField, Tooltip("Start looking at the given target when the player is this " +
            "many units away from the destination waypoint.")]
        [Min(ZeroDistance)]
        private float lookAtTransitionDistance = 1f;

        [field: SerializeField, Tooltip("Look at this many units above from the destination while walking.")]
        public float CameraRecenteringHeight { get; private set; } = 1.4f;

        [Range(0.1f, 4f)] 
        public float CameraPanSensitivity = 1f;


        [field: Header("Events")]

        /// <summary>
        /// Invoked when the player begins to walk. Passes the destination Waypoint as 
        /// an argument.
        /// </summary>
        [field: SerializeField]
        public UnityEvent<Waypoint> WalkStarted { get; private set; } = new();

        /// <summary>
        /// Invoked when the player reached the destination/waypoint. Passes the destination 
        /// Waypoint as an argument.
        /// </summary>
        [field: SerializeField]
        public UnityEvent<Waypoint> WalkCompleted { get; private set; } = new();


        [field: Header("Internal Variables")]

        /// <summary>
        /// The player can determine an optimal path to walk and it can avoid obstacles 
        /// while walking.
        /// </summary>
        [field: SerializeField]
        public NavMeshAgent Player { get; private set; }

        /// <summary>
        /// This camera drives the Main Camera when navigating and looking around.
        /// </summary>
        [field: SerializeField]
        public CinemachineVirtualCamera PovCam { get; private set; }

        /// <summary>
        /// True while the player is walking
        /// </summary>
        public bool IsPlayerWalking { get; private set; } = false;

        /// <summary>
        /// The maximum left mouse button hold duration (seconds) to consider 
        /// a short click. The player will walk to a waypoint if the waypoint 
        /// gets clicked within this duration.
        /// </summary>
        public float ShortClickDuration
        {
            get => waypointHandler.MouseHandler.ShortClickDuration;
            set
            {
                waypointHandler.MouseHandler.ShortClickDuration = value;
            }
        }

        public Transform LookAtTarget
        {
            get => lookAtTarget;
            set
            {
                lookAtTarget = value;
                PovCam.LookAt = value;
            }
        }

        public float LookAtTransitionDistance
        {
            get => lookAtTransitionDistance;
            set
            {
                if (value < ZeroDistance)
                {
                    Debug.LogWarning("Minimum distance to look at should be a positive value!");
                    return;
                }
                lookAtTransitionDistance = value;
            }
        }


        // Private fields
        private WaypointHandler waypointHandler;

        private CinemachinePOV cmPov;
        private CinemachineZoom cmZoom;

        private bool isNavigationEnabled = true;
        private bool isPanningEnabled = true;

        /// <summary>
        ///     Used to indicate the frame that the application gains focus again.
        ///     Helps to avoid camera jitter due to strange cached mouse inputs.
        /// </summary>
        /// <remarks>
        ///     Eventually would be good to replace the use of the <see cref="Input"/> 
        ///     system in favour of the newer Input Action system. It seems the issue
        ///     that this fixes may be resolved naturally.
        /// </remarks>
        private bool focusGainedFrame = false;
        

        private void Awake()
        {
            cmPov = PovCam.GetCinemachineComponent<CinemachinePOV>();
            cmZoom = Camera.main.GetComponent<CinemachineZoom>();
            waypointHandler = GetComponent<WaypointHandler>();
        }

        private void Start()
        {
            PrepareCamera();
        }

        private void OnEnable()
        {
            waypointHandler.Navigate += Walk;
            waypointHandler.MouseHandler.MouseHold += HandleCameraPanning;
            waypointHandler.MouseHandler.ShortClick += StopCameraPanning;
            waypointHandler.MouseHandler.LongClick += StopCameraPanning;
        }

        private void OnDisable()
        {
            waypointHandler.Navigate -= Walk;
            waypointHandler.MouseHandler.MouseHold -= HandleCameraPanning;
            waypointHandler.MouseHandler.ShortClick -= StopCameraPanning;
            waypointHandler.MouseHandler.LongClick -= StopCameraPanning;
        }

        /// <summary>
        ///     Sets the <see cref="focusGainedFrame"/> flag whenever the application
        ///     gains focus
        /// </summary>
        /// <param name="focus">
        ///     Indicates whether focus is gained (<c>true</c>) or lost (<c>false</c>)
        /// </param>
        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                focusGainedFrame = true;
            }
        }

        /// <summary>
        /// Look at a given target without moving.
        /// </summary>
        /// <param name="target">Target to look at</param>
        public void LookAt(Transform target)
        {
            PovCam.LookAt = target;

            SetPovCamRecenterVector(target.position, PovCam.transform.position);
            RecenterCamera(true);
            StartCoroutine(WaitUntilCameraRecenteres());
        }

        /// <summary>
        /// Walk to a given destination while looking straight. It will look at
        /// lookAtAfterWalking at the end if it is not null.
        /// </summary>
        /// <param name="destination">
        /// Walking destination. This can be a point on the floor. CameraRecenteringHeight
        /// field will control how high the camera should look at from the destination.
        /// </param>
        public void Walk(Waypoint destination)
        {
            if (IsPlayerWalking) { return; }
            StartCoroutine(LookStraightAheadAndWalk(destination));
        }

        /// <summary>
        /// Enable or disable navigation by toggling interactability of waypoints.
        /// </summary>
        /// <param name="enable">true = Navigation enabled.</param>
        public void EnableNavigation(bool enable)
        {
            isNavigationEnabled = enable;
            waypointHandler.InteractableWaypoints(enable);
        }

        /// <summary>
        /// Enable or disable camera panning and zooming.
        /// </summary>
        /// <param name="enable">true = Camera pan and zoom enabled.</param>
        public void EnableCameraPanAndZoom(bool enable)
        {
            if(!enable) StopCameraPanning();
            isPanningEnabled = enable;
            cmZoom.enabled = enable;
        }

        private void PrepareCamera()
        {
            if (lookAtTarget == null)
            {
                RecenterCamera(false);
                return;
            }
            LookAt(lookAtTarget);
        }

        private IEnumerator LookStraightAheadAndWalk(Waypoint destination)
        {
            IsPlayerWalking = true;
            waypointHandler.InteractableWaypoints(false);
            WalkStarted?.Invoke(destination);

            Player.SetDestination(destination.transform.position);
            yield return null; // Wait a frame for the NavMesh agent to set internal values

            yield return StartCoroutine(ControlCamerasWhileWalking(destination.transform.position));
            
            WalkCompleted?.Invoke(destination);
            if (isNavigationEnabled) { waypointHandler.InteractableWaypoints(true); }
            IsPlayerWalking = false;
        }

        private IEnumerator ControlCamerasWhileWalking(Vector3 destination)
        {
            EnableCameraPanAndZoom(false);

            // This is an elevated point from the destination. The camera will recenter on this point
            // when walking. CameraRecenteringHeight controls the elevation.
            Vector3 elevatedDestination = new(destination.x, CameraRecenteringHeight, destination.z);

            RecenterCamera(true);

            while (Player.hasPath)
            {
                if (Player.remainingDistance <= lookAtTransitionDistance)
                {
                    if (lookAtTarget != null)
                    {
                        SetPovCamRecenterVector(lookAtTarget.position, elevatedDestination);
                        RecenterCamera(true);
                        yield return StartCoroutine(WaitUntilCameraRecenteres());
                    }
                    break;
                }

                // Uses the current velocity from the nav agent to determine "forward" direction and
                // look in that direction while moving
                SetPovCamRecenterVector(elevatedDestination + Player.desiredVelocity, elevatedDestination);
                yield return null;
            }

            RecenterCamera(false);
            if (isNavigationEnabled) { EnableCameraPanAndZoom(true); }
        }

        private IEnumerator WaitUntilCameraRecenteres()
        {
            const float Threshold = 0.02f;
            yield return null; // Wait a frame for Cinemachine to set its recentering flags
            yield return new WaitUntil(
                () => MathHelper.CompareVectors(PovCam.Follow.forward, PovCam.transform.forward, Threshold));

            RecenterCamera(false);
        }

        /// <summary>
        /// Calculate and assign a vector to the POV camera to recenter the camera. 
        /// Use <see cref="RecenterCamera(bool)"/> method to recenter the camera once the vector is set. 
        /// As an example, if we want to recenter camera on point A and our current position is point B, 
        /// use A as the endpoint and B as origin. When traveling from one waypoint to another, we use the 
        /// previous waypoint as origin and the destination waypoint as endpoint.
        /// </summary>
        /// <param name="endpoint">Vector end</param>
        /// <param name="origin">Vector start</param>
        private void SetPovCamRecenterVector(Vector3 endpoint, Vector3 origin)
        {
            PovCam.Follow.forward = endpoint - origin;
        }

        /// <summary>
        /// Recenter camera's forward vector parallel to a given vector.
        /// This vector can be set by using <see cref="SetPovCamRecenterVector(Vector3, Vector3)"/> method.
        /// </summary>
        /// <param name="recenter">true = Keep recentering</param>
        private void RecenterCamera(bool recenter)
        {
            cmPov.m_HorizontalRecentering.m_enabled = recenter;
            cmPov.m_VerticalRecentering.m_enabled = recenter;
        }

        /// <summary>
        ///     Handles camera panning as the mouse is moved across the screen. Will
        ///     be triggered when the mouse is held down.
        /// </summary>
        /// <remarks>
        ///     Camera panning should be separated out into its own behaviour,
        ///     like CORE Cinema but using these camera control patterns
        /// </remarks>
        private void HandleCameraPanning()
        {
            if (!isPanningEnabled) { return; }

            // Skip this frame to avoid the camera jumping when focus is regained
            if (focusGainedFrame)
            {
                focusGainedFrame = false;
                return;
            }

            float modifier = CameraPanSensitivity * PlatformSensitivityModifier;

            cmPov.m_HorizontalAxis.Value -= Input.GetAxis("Mouse X") * modifier;
            cmPov.m_VerticalAxis.Value += Input.GetAxis("Mouse Y") * modifier;
        }

        /// <summary>
        ///     Resets the input on the POV camera when the mouse is 
        ///     no longer being dragged across the screen.
        /// </summary>
        private void StopCameraPanning()
        {
            cmPov.m_HorizontalAxis.m_InputAxisValue = NoInput;
            cmPov.m_VerticalAxis.m_InputAxisValue = NoInput;
        }
    }
}
