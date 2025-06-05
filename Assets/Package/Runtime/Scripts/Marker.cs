using System;
using UnityEngine;

namespace VARLab.Navigation.PointClick
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Marker : MonoBehaviour
    {
        [SerializeField]
        private Color defaultColor = new(1f, 1f, 1f, 0.4f);
        public Color DefaultColor
        {
            get => defaultColor;
            set { defaultColor = value; }
        }

        [SerializeField]
        private Color highlightColor = new(1f, 1f, 1f, 1f);
        public Color HighlightColor
        {
            get => highlightColor;
            set { highlightColor = value; }
        }

        internal Action<Waypoint> MouseOverOnWaypoint;
        internal Action MouseExitOnWaypoint;

        private SpriteRenderer spriteRenderer = null;

        private void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        internal void Highlight()
        {
            spriteRenderer.color = highlightColor;
        }

        internal void Unhighlight()
        {
            spriteRenderer.color = defaultColor;
        }

        private void OnMouseEnter()
        {
            Highlight();
            MouseOverOnWaypoint(transform.parent.GetComponent<Waypoint>());
        }

        private void OnMouseExit()
        {
            Unhighlight();
            MouseExitOnWaypoint();
        }
    }
}
