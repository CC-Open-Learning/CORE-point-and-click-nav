using System;
using UnityEngine;

namespace VARLab.Navigation.PointClick
{
    /// <summary>
    /// This class is responsible for controlling and acting on waypoints. Users of the PointClick 
    /// package should not access contents of this class. Refer to PointClickNavigation.cs for 
    /// public APIs.
    /// </summary>
    public class WaypointHandler : MonoBehaviour
    {
        internal Action<Waypoint> Navigate;

        internal MouseEventHandler MouseHandler { get; } = new();

        private Waypoint[] waypoints;
        private Waypoint currentlySelectedWaypoint;

        private void Awake()
        {
            waypoints = FindObjectsByType<Waypoint>(
                sortMode: FindObjectsSortMode.None,
                findObjectsInactive: FindObjectsInactive.Exclude);
        }

        private void OnEnable()
        {
            MouseHandler.ShortClick += OnMouseClick;
        }

        private void Start()
        {
            foreach (var waypoint in waypoints)
            {
                var marker = waypoint.Marker;
                if (marker == null) { continue; }

                marker.Unhighlight();
                marker.MouseOverOnWaypoint += (waypoint) => currentlySelectedWaypoint = waypoint;
                marker.MouseExitOnWaypoint += () => currentlySelectedWaypoint = null;
            }
        }

        private void Update()
        {
            MouseHandler.HandleMouseInteractions();
        }

        private void OnDisable()
        {
            MouseHandler.ShortClick -= OnMouseClick;
        }

        /// <summary>
        /// Enable or disable waypoints by toggling their interactability.
        /// </summary>
        /// <param name="interactable">true = All the waypoints are interactable</param>
        internal void InteractableWaypoints(bool interactable)
        {
            foreach (var waypoint in waypoints)
            {
                var marker = waypoint.Marker;
                marker.GetComponent<Collider>().enabled = interactable;
            }
        }

        private void OnMouseClick()
        {
            if (currentlySelectedWaypoint == null) { return; }
            Navigate(currentlySelectedWaypoint);
        }
    }
}
