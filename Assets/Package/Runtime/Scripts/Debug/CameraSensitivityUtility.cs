using System.Text;
using UnityEngine;

namespace VARLab.Navigation.PointClick.Debugging
{

    /// <summary>
    ///     A debug utility for testing camera panning settings in Editor vs WebGL.
    ///     Uses IMGUI to draw a simple draggable debug window.
    /// </summary>
    public class CameraSensitivityUtility : MonoBehaviour
    {
        private static readonly Rect DefaultRect = new(8, 8, 220, 0);

        private readonly int id = 1024;
        
        public PointClickNavigation Controller;
        public bool Active = false;

        private Rect windowRect = DefaultRect;

        public void OnGUI()
        {
            if (!Active) { return; }

            windowRect = GUILayout.Window(id,
                windowRect,
                WindowCallback,
                "Camera Pan Settings",
                GUILayout.ExpandWidth(true), 
                GUILayout.ExpandHeight(true));
        }

        private void WindowCallback(int id)
        {
            if (id != this.id) { return; }

            float platform = Controller.CameraPanSensitivity * Controller.PlatformSensitivityModifier;

            float xInput = Input.GetAxis("Mouse X");
            float yInput = Input.GetAxis("Mouse Y");

            StringBuilder builder = new();
            builder.AppendLine("Sensitivity:");
            builder.AppendLine($"  platform\t{platform}");
            builder.AppendLine($"  raw\t{Controller.CameraPanSensitivity}");
            builder.AppendLine($"  modifier\t{Controller.PlatformSensitivityModifier}");
            builder.AppendLine($"X Input:\t{xInput}");
            builder.AppendLine($"Y Input:\t{yInput}");
            builder.AppendLine($"X Applied:\t{xInput * platform}");
            builder.AppendLine($"Y Applied:\t{yInput * platform}");

            // Camera sensitivity ranges from 0.1 to 4 so this slider will use the same.
            // This manipulates the underlying sensitivity value directly
            Controller.CameraPanSensitivity = GUILayout.HorizontalSlider(Controller.CameraPanSensitivity, 0.1f, 4f);

            GUILayout.Label(builder.ToString());

            Active = !GUILayout.Button("Close");

            // Allows the window to be dragged around the screen to be repositioned
            GUI.DragWindow();
        }
    }
}
