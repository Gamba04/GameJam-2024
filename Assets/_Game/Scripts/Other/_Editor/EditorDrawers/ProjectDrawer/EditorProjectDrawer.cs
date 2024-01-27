using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

namespace Editor
{
    [InitializeOnLoad]
    public static class EditorProjectDrawer
    {

        #region Serializable

        [Serializable]
        private struct FoldersData
        {
            public List<Folder> foldersData;
        }

        [Serializable]
        private struct Folder
        {
            public string path;

            [SerializeField]
            private string color;
            
            public Color Color
            {
                get
                {
                    if (ColorUtility.TryParseHtmlString(color, out Color result))
                    {
                        return result;
                    }
            
                    return default;
                }
            }
        }

        #endregion

        private const string foldersDataPath = "Assets/_Game/Scripts/Other/_Editor/EditorDrawers/ProjectDrawer/FoldersData.json";

        private static readonly char separator = Path.AltDirectorySeparatorChar;

        private const float oneColumnHeight = 16;

        private const float offset = 21;
        private const float levelSeparation = 14;

        private const float folderHeight = 12;
        private const float folderWidth = 6;

        private const float subFolderWidth = 2;

        static EditorProjectDrawer()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemOnGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
        }

        #region Processing

        private static void OnProjectWindowItemOnGUI(string guid, Rect area)
        {
            if (area.height > oneColumnHeight) return;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            List<Folder> foldersData = GetFoldersData();

            if (foldersData == null) return;

            ProcessPath(path, foldersData, area);
        }

        private static List<Folder> GetFoldersData()
        {
            try
            {
                string json = AssetDatabase.LoadAssetAtPath<TextAsset>(foldersDataPath).text;

                FoldersData foldersData = JsonUtility.FromJson<FoldersData>(json);

                return foldersData.foldersData;
            }
            catch
            {
                Debug.LogError($"Failed to read JSON file at '{foldersDataPath}'\n");

                return null;
            }
        }

        private static void ProcessPath(string path, List<Folder> foldersData, Rect area)
        {
            if (!CheckSeparator(path)) return;

            Folder folder;

            if (CheckFolder(path, foldersData, out folder)) // Folder
            {
                Color color = folder.Color;

                DrawFolder(area, color);
            }

            int level = 1;

            while (CheckSeparator(path))
            {
                ReducePath(ref path);

                if (CheckFolder(path, foldersData, out folder)) // Sub-folder
                {
                    Color color = folder.Color;

                    DrawSubFolder(area, color, level);
                }

                level++;
            }
        }

        private static bool CheckSeparator(string path) => path.Contains(separator.ToString());

        private static bool CheckFolder(string path, List<Folder> foldersData, out Folder folder)
        {
            int folderIndex = foldersData.FindIndex(folder => folder.path == path);

            if (folderIndex != -1)
            {
                folder = foldersData[folderIndex];
                return true;
            }

            folder = default;
            return false;
        }

        private static void ReducePath(ref string path)
        {
            int lastSeparator = path.LastIndexOf(separator);

            if (lastSeparator == -1) path = "";
            else path = path.Remove(lastSeparator);
        }

        #endregion

        // ----------------------------------------------------------------------------------------------------------------------------

        #region Drawing

        private static void DrawFolder(Rect area, Color color)
        {
            float centeringOffset = (area.height - folderHeight) * 0.5f;

            area.height = folderHeight;
            area.y += centeringOffset;

            area.width = folderWidth;
            area.x -= offset;

            EditorGUI.DrawRect(area, color);
        }

        private static void DrawSubFolder(Rect area, Color color, int level)
        {
            float centeringOffset = (folderWidth - subFolderWidth) * 0.5f;

            area.width = subFolderWidth;
            area.x -= offset - centeringOffset + level * levelSeparation;

            EditorGUI.DrawRect(area, color);
        }

        #endregion

    }
}

#endif