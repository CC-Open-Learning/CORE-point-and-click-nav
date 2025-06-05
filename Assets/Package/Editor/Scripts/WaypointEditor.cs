using UnityEditor;

namespace VARLab.Navigation.PointClick
{
    [CustomEditor(typeof(Waypoint))]
    public class WaypointEditor : Editor
    {
        private Waypoint waypoint;

        private void OnEnable()
        {
            waypoint = target as Waypoint;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            UpdateMarkerInEditor();
        }

        public void UpdateMarkerInEditor()
        {
            if (waypoint.ShowMarker != waypoint.Marker.gameObject.activeSelf)
            {
                waypoint.Marker.gameObject.SetActive(waypoint.ShowMarker);
            }
        }

    }
}
