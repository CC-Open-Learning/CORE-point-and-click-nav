using UnityEngine;

namespace VARLab.Navigation.PointClick
{
    public class Waypoint : MonoBehaviour
    {
        [SerializeField]
        private Marker marker;

        [SerializeField]
        private bool showMarker;

        /// <summary>
        ///     Use this property to access the marker instead of 
        ///     iterating children of waypoint GameObject
        /// </summary>
        public Marker Marker => marker;

        /// <summary>
        ///     Indicates whether the sprite marker should be 
        ///     shown for this waypoint
        /// </summary>
        public bool ShowMarker => showMarker;
    }
}
