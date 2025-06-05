using UnityEditor;
using UnityEngine;

namespace VARLab.Navigation.PointClick
{

    /// <summary>
    ///     Provides context menu entries to instantiate Point & Click Navigation prefabs in a scene
    /// </summary>
    public class MenuItems : MonoBehaviour
    {
        private const string MenuRootPath = "GameObject/VARLab/";
        private const string PackageRootPath = "Packages/com.varlab.navigation.pointclick/";

        private const string WaypointMenuPath = MenuRootPath + "Navigation Waypoint";
        private const string WaypointPrefabAssetPath = PackageRootPath + "Prefabs/Waypoint.prefab";

        private const string PointClickNavMenuPath = MenuRootPath + "Point Click Navigation Handler";
        private const string PointClickNavPrefabAssetPath = PackageRootPath + "Prefabs/Point Click Navigation.prefab";

        [MenuItem(WaypointMenuPath)]
        private static void InstantiateWaypoint()
        {
            InstantiateInSceneHierarchy(WaypointPrefabAssetPath);
        }

        [MenuItem(PointClickNavMenuPath)]
        private static void InstantiatePlayer()
        {
            if (FindAnyObjectByType(typeof(PointClickNavigation), FindObjectsInactive.Include) != null)
            {
                Debug.LogError("Failed to create Point Click Navigation Handler. Only a single instance of PointClickNavigation is allowed!");
                return;
            }

            InstantiateInSceneHierarchy(PointClickNavPrefabAssetPath);
        }

        /// <summary>
        /// Instantiate a GameObject of a given prefab in the scene hierarchy using the asset path
        /// </summary>
        /// <param name="prefabAssetPath">Asset path of the prefab to be loaded</param>
        private static void InstantiateInSceneHierarchy(string prefabAssetPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
            if (prefab == null)
            {
                Debug.LogError($"Failed to load {prefabAssetPath}!");
                return;
            }

            GameObject prefabInstance = Instantiate(prefab);
            prefabInstance.name = prefab.name;
            prefabInstance.transform.SetParent(Selection.activeTransform, worldPositionStays: false);
        }
    }
}
