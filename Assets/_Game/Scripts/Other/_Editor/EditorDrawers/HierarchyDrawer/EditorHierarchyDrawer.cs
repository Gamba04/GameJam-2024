using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

namespace Editor
{
    [InitializeOnLoad]
    public static class EditorHierarchyDrawer
    {
        private const float offset = 21;
        private const float levelSeparation = 14;

        private const float parentHeight = 12;
        private const float parentWidth = 6;

        private const float childWidth = 2;

        static EditorHierarchyDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyItemGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
        }

        #region Processing

        private static void OnHierarchyItemGUI(int instanceID, Rect area)
        {
            Transform transform = (EditorUtility.InstanceIDToObject(instanceID) as GameObject)?.transform;

            int level = 0;

            while (transform != null)
            {
                HierarchyFolder folder = transform.GetComponent<HierarchyFolder>();

                if (folder != null)
                {
                    DrawFolder(area, folder.Color, level);
                }

                transform = transform.parent;
                level++;
            }
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------------------------------

        #region Drawing

        private static void DrawFolder(Rect area, Color color, int level)
        {
            if (level == 0) DrawParent(area, color);
            else DrawChild(area, color, level);
        }

        private static void DrawParent(Rect area, Color color)
        {
            float centeringOffset = (area.height - parentHeight) * 0.5f;

            area.height = parentHeight;
            area.y += centeringOffset;

            area.width = parentWidth;
            area.x -= offset;

            EditorGUI.DrawRect(area, color);
        }

        private static void DrawChild(Rect area, Color color, int level)
        {
            float centeringOffset = (parentWidth - childWidth) * 0.5f;

            area.width = childWidth;
            area.x -= offset - centeringOffset + level * levelSeparation;

            EditorGUI.DrawRect(area, color);
        }

        #endregion

    }
}

#endif