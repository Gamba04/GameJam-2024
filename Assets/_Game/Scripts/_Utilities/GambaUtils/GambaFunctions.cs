using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEditor;

public static class GambaFunctions
{

    #region Math

    public static float GetAngle(Vector2 point)
    {
        const float pi = Mathf.PI;

        float x = point.x;
        float y = point.y;

        float r;

        if (x > 0)
        {
            if (y > 0) // Cuadrant: 1
            {
                r = Mathf.Atan(y / x);
            }
            else if (y < 0) // Cuadrant: 4
            {
                r = pi * 3 / 2f + (pi * 3 / 2f - (pi - Mathf.Atan(y / x)));
            }
            else // Right
            {
                r = 0;
            }
        }
        else if (x < 0)
        {
            if (y > 0) // Cuadrant: 2
            {
                r = pi * 1 / 2f + (pi * 1 / 2f + Mathf.Atan(y / x));

            }
            else if (y < 0) // Cuadrant: 3
            {
                r = pi + Mathf.Atan(y / x);

            }
            else // Left
            {
                r = pi;
            }
        }
        else
        {
            if (y > 0) // Up
            {
                r = pi * 1 / 2f;
            }
            else if (y < 0) // Down
            {
                r = pi * 3 / 2f;
            }
            else // Zero
            {
                r = 0;
            }
        }

        return r;
    }

    public static Vector2 AngleToVector(float angle) => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

    public static Vector2 Perpendicular(this Vector2 vector) => new Vector2(vector.y, -vector.x);

    public static Vector3 Perpendicular(this Vector3 a, Vector3 b) => Vector3.Cross(a, b);

    public static Vector2 MultipliedBy(this Vector2 a, Vector2 b) => a.MultipliedBy(b.x, b.y);

    public static Vector2 MultipliedBy(this Vector2 a, float x, float y) => new Vector2(a.x * x, a.y * y);

    public static Vector3 MultipliedBy(this Vector3 a, Vector3 b) => a.MultipliedBy(b.x, b.y, b.z);

    public static Vector3 MultipliedBy(this Vector3 a, float x, float y, float z) => new Vector3(a.x * x, a.y * y, a.z * z);

    public static Vector2 DividedBy(this Vector2 a, Vector2 b) => a.DividedBy(b.x, b.y);

    public static Vector2 DividedBy(this Vector2 a, float x, float y) => new Vector2(a.x / x, a.y / y);

    public static Vector3 DividedBy(this Vector3 a, Vector3 b) => a.DividedBy(b.x, b.y, b.z);

    public static Vector3 DividedBy(this Vector3 a, float x, float y, float z) => new Vector3(a.x / x, a.y / y, a.z / z);

    public static Color GetAlpha(this Color color, float value) => new Color(color.r, color.g, color.b, value);

    /// <summary> Linearly interpolates between colors <paramref name="a"/> and <paramref name="b"/> by <paramref name="t"/> in HSV mode. </summary>
    public static Color ColorLerp(Color a, Color b, float t) // Change to LCH
    {
        Vector3 aHSV = GetHSV(a);
        Vector3 bHSV = GetHSV(b);

        Vector3 targetHSV = Vector3.Lerp(aHSV, bHSV, t);

        return Color.HSVToRGB(targetHSV.x, targetHSV.y, targetHSV.z);

        Vector3 GetHSV(Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);

            return new Vector3(h, s, v);
        }
    }

    public static Vector3 GetScaleOf(float size)
    {
        Vector3 scale = Vector3.one * size;
        scale.z = 1;

        return scale;
    }

    /// <summary> Rounds value to the nearest multiple of <paramref name="step"/>. </summary>
    public static void Step(this ref float value, float step)
    {
        value /= step;
        value = Mathf.Round(value);
        value *= step;
    }

    public static bool RandomBool() => UnityEngine.Random.Range(0, 2) == 0;

    public static float VolumeToDB(float volume) => volume > 0 ? Mathf.Log10(volume) * 20 : float.MinValue;

    public static float DBToVolume(float dB) => Mathf.Pow(10, dB / 20);

    public static bool Contains(this LayerMask mask, int layer) => mask == (mask | (1 << layer));

    #endregion

    // -------------------------------------------------------------------------------------------------------------------

    #region Lists & Related

    /// <summary> Checks if Type has a parameterless constructor. </summary>
    public static bool TryDefaultConstructor<T>(out T result)
    {
        result = default;

        if (typeof(T).IsValueType) return false;

        var constructor = typeof(T).GetConstructor(Type.EmptyTypes);

        if (constructor == null) return false;

        result = (T)constructor.Invoke(new object[0]);
        return true;
    }

    private static void ResizeInternal<T>(this List<T> list, int length, Func<T> defaultElement)
    {
        if (list == null || list.Count == length) return;

        List<T> newList = new List<T>();

        for (int i = 0; i < length; i++)
        {
            if (i < list.Count)
            {
                newList.Add(list[i]);
            }
            else
            {
                T element = defaultElement != null ? defaultElement() : default;

                newList.Add(element);
            }
        }

        list.Clear();
        list.AddRange(newList);
    }

    /// <summary> Resize list to length without losing data. </summary>
    public static void Resize<T>(this List<T> list, int length)
        where T : new()
    {
        list.ResizeInternal(length, () => new T());
    }

    /// <summary> Resize list to length without losing data. New elements will be set to default value. </summary>
    public static void ResizeEmpty<T>(this List<T> list, int length)
    {
        list.ResizeInternal(length, () => default);
    }

    /// <summary> Resize list to enum length without losing data. </summary>
    public static void Resize<T>(this List<T> list, Type enumType)
        where T : new()
    {
        int length = GetEnumLenght(enumType);

        list.Resize(length);
    }

    /// <summary> Resize list to enum length without losing data. New elements will be set to default value. </summary>
    public static void ResizeEmpty<T>(this List<T> list, Type enumType)
        where T : new()
    {
        int length = GetEnumLenght(enumType);

        list.ResizeEmpty(length);
    }

    public static int GetEnumLenght(Type type) => Enum.GetValues(type).Length;

    public static int GetEnumLenght<E>() where E : Enum => GetEnumLenght(typeof(E));

    public static void ForEach<T>(this List<T> list, Action<T, int> action)
    {
        if (list == null) return;

        for (int i = 0; i < list.Count; i++) action?.Invoke(list[i], i);
    }

    /// <summary> QuickSort a List of any type with a custom comparison method. </summary>
    /// <param name="comparison"> Custom comparison method which defines if element1 <, <=, ==, !=, >= or > element2. Returns an int which corresponds to an equivalent comparison with 0. </param>
    public static void QuickSort<T>(this List<T> list, Comparison<T> comparison)
    {
        if (list.Count <= 1) return;

        int pivot = list.Count - 1;

        // Create partitions
        List<T> left = new List<T>();
        List<T> right = new List<T>();

        for (int i = 0; i < list.Count - 1; i++)
        {
            List<T> partition = comparison(list[i], list[pivot]) <= 0 ? left : right;

            partition.Add(list[i]);
        }

        // Recurse
        left.QuickSort(comparison);
        right.QuickSort(comparison);

        // Join partitions
        T pivotElement = list[pivot];

        list.Clear();

        list.AddRange(left);
        list.Add(pivotElement);
        list.AddRange(right);
    }

    /// <summary> QuickSort a <see cref="List{T}"/> with its default comparison. </summary>
    public static void QuickSort<T>(this List<T> list) where T : IComparable<T>
    {
        list.QuickSort((e1, e2) => e1.CompareTo(e2));
    }

    public static void ShuffleList<T>(this List<T> list, bool forceChanging = false)
    {
        if (list == null || list.Count < 1) return;

        List<T> result = new List<T>();

        // Fill indexes
        List<int> indexes = new List<int>();
        for (int i = 0; i < list.Count; i++) indexes.Add(i);

        for (int i = 0; i < list.Count; i++)
        {
            List<int> newIndexes = new List<int>(indexes);
            if (forceChanging && newIndexes.Count > 1) newIndexes.Remove(i);

            int index = newIndexes[UnityEngine.Random.Range(0, newIndexes.Count)];
            T element = list[index];

            indexes.Remove(index);

            result.Add(element);
        }

        list.Clear();
        list.AddRange(result);
    }

    /// <summary> Destroys all element's <see cref="GameObject"/> and clears the <see cref="List{T}"/>. </summary>
    public static void DestroyAll<C>(this List<C> list)
        where C : Component
    {
        list.ForEach(component => UnityEngine.Object.Destroy(component.gameObject));
        list.Clear();
    }

    public static void Clear<T>(this T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = default;
        }
    }

    #endregion

    // -------------------------------------------------------------------------------------------------------------------

    #region Physics2D

    /// <summary> Checks for component attached to Rigidbody. </summary>
    public static T CheckForComponent2D<T>(Vector2 worldPos, int layerMask = ~0) where T : Component
    {
        return CheckForComponent2D(worldPos, collider => collider.attachedRigidbody?.GetComponent<T>(), layerMask);
    }

    public static T CheckForComponent2D<T>(Vector2 worldPos, Converter<Collider2D, T> getComponentMethod, int layerMask = ~0) where T : Component
    {
        if (getComponentMethod == null) throw new ArgumentNullException("getComponentMethod cannot be null");

        Collider2D collider = Physics2D.OverlapPoint(worldPos, layerMask);

        if (collider == null) return null;

        T target = getComponentMethod(collider);

        return target;
    }

    public static T CheckForNearestComponent2D<T>(Vector2 worldPos, int layerMask = ~0, List<T> ignoreList = null) where T : Component
    {
        return CheckForNearestComponent2D(worldPos, collider => collider.attachedRigidbody?.GetComponent<T>(), layerMask, ignoreList);
    }

    public static T CheckForNearestComponent2D<T>(Vector2 worldPos, Converter<Collider2D, T> getComponentMethod, int layerMask = ~0, List<T> ignoreList = null) where T : Component
    {
        List<T> components = CheckForComponents2D(worldPos, getComponentMethod, layerMask);

        T target = null;

        float currentDistance = Mathf.Infinity;

        foreach (T component in components)
        {
            if (ignoreList != null && ignoreList.Contains(component)) continue;

            float distance = ((Vector2)component.transform.position - worldPos).sqrMagnitude;

            if (distance < currentDistance)
            {
                target = component;

                currentDistance = distance;
            }
        }

        return target;
    }

    public static List<T> CheckForComponents2D<T>(Vector2 worldPos, int layerMask = ~0, List<T> ignoreList = null) where T : Component
    {
        return CheckForComponents2D(worldPos, collider => collider.attachedRigidbody?.GetComponent<T>(), layerMask, ignoreList);
    }

    public static List<T> CheckForComponents2D<T>(Vector2 worldPos, Converter<Collider2D, T> getComponentMethod, int layerMask = ~0, List<T> ignoreList = null) where T : Component
    {
        if (getComponentMethod == null) throw new ArgumentNullException("getComponentMethod cannot be null");

        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPos, layerMask);

        List<T> targets = new List<T>();

        foreach (Collider2D collider in colliders)
        {
            T target = getComponentMethod(collider);

            if (target != null)
            {
                if (ignoreList != null && ignoreList.Contains(target)) continue;

                targets.Add(target);
            }
        }

        return targets;
    }

    public static T CheckForComponentRaycast2D<T>(Vector2 origin, Vector2 direction, int layerMask = ~0) where T : Component
    {
        return CheckForComponentRaycast2D(origin, direction, collider => collider.attachedRigidbody?.GetComponent<T>(), layerMask);
    }

    public static T CheckForComponentRaycast2D<T>(Vector2 origin, Vector2 direction, Converter<Collider2D, T> getComponentMethod, int layerMask = ~0) where T : Component
    {
        RaycastHit2D result = Physics2D.Raycast(origin, direction, direction.magnitude, layerMask);

        if (result)
        {
            T target = getComponentMethod(result.collider);

            return target;
        }

        return null;
    }

    #endregion

    // -------------------------------------------------------------------------------------------------------------------

    #region Singleton

    public static T GetSingleton<T>(ref T instance)
        where T : Component
    {
        if (instance == null)
        {
            T sceneResult = UnityEngine.Object.FindObjectOfType<T>();

            if (sceneResult != null)
            {
                instance = sceneResult;
            }
            else
            {
                // Create instance
                GameObject obj = new GameObject($"{typeof(T).Name}_Instance");

                instance = obj.AddComponent<T>();
            }
        }

        return instance;
    }

    public static void OnSingletonInit<T>(ref T instance, T @this)
        where T : Component
    {
        if (instance == null)
        {
            instance = @this;
        }
        else if (instance != @this)
        {
            UnityEngine.Object.Destroy(@this.gameObject);
        }
    }

    #endregion

    // -------------------------------------------------------------------------------------------------------------------

    #region Scene Management

    public static string GetActiveSceneName() => GetSceneName(SceneManager.GetActiveScene().buildIndex);

    public static string GetSceneName(int buildIndex)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(buildIndex);

        return Path.GetFileNameWithoutExtension(path);
    }

    public static void LoadScene(string name, bool async = false)
    {
        if (async) SceneManager.LoadSceneAsync(name);
        else SceneManager.LoadScene(name);
    }

    public static void LoadScene(int buildIndex, bool async = false)
    {
        if (async) SceneManager.LoadSceneAsync(buildIndex);
        else SceneManager.LoadScene(buildIndex);
    }

    public static void ReloadScene() => LoadScene(SceneManager.GetActiveScene().buildIndex);

    #endregion

    // -------------------------------------------------------------------------------------------------------------------

    #region Reflection

    public static T GetValueOfType<T>(this SerializedProperty property)
    {
        if (property == null) throw new ArgumentNullException(nameof(property));

        string path = property.propertyPath.Replace("Array.data", "").Replace("]", "");
        string[] names = path.Split('.');

        object currentObject = property.serializedObject.targetObject;

        foreach (string name in names)
        {
            if (name[0] == '[') // Array
            {
                int index = int.Parse(name.Substring(1));

                List<object> list = new List<object>(currentObject as IEnumerable<object>);

                currentObject = list[index];
            }
            else // Object
            {
                FieldInfo field = GetField(currentObject.GetType(), name);

                if (field == null) return default;

                currentObject = field.GetValue(currentObject);
            }
        }

        return (T)currentObject;
    }

    private static FieldInfo GetField(Type type, string name)
    {
        FieldInfo field;

        do
        {
            field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            type = type.BaseType;
        }
        while (field == null && type != null);

        return field;
    }

    #endregion

    // -------------------------------------------------------------------------------------------------------------------

    #region Other

    public static void Reset(this Transform transform)
    {
        if (transform.position == Vector3.zero && transform.rotation == Quaternion.identity && transform.localScale == Vector3.one) return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // Re-apply modifications
            Undo.RecordObject(transform, "Transform modification");

            transform.SetPositionAndRotation(transform.position, transform.rotation);
            transform.localScale = transform.localScale;

            Undo.FlushUndoRecordObjects();
        }
#endif

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        if (transform is RectTransform rt)
        {
            rt.anchoredPosition3D = Vector3.zero;
            rt.sizeDelta = Vector2.zero;
        }
    }

    #endregion

    // -------------------------------------------------------------------------------------------------------------------

    #region Editor

#if UNITY_EDITOR

    #region Gizmos

    public static void GizmosDraw2DArrow(Vector2 origin, Vector2 direction)
    {
        Vector2 head = origin + direction;
        Gizmos.DrawLine(origin, head);

        Vector2 perpendicular = new Vector2(direction.y, -direction.x);
        Gizmos.DrawLine(head, head + perpendicular * 0.1f - direction * 0.2f);
        Gizmos.DrawLine(head, head - perpendicular * 0.1f - direction * 0.2f);
    }

    public static void GizmosDraw2DArrow(Vector2 origin, Vector2 direction, Vector2 headSize)
    {
        Vector2 head = origin + direction;
        Gizmos.DrawLine(origin, head);

        Vector2 perpendicular = direction.Perpendicular();

        Gizmos.DrawLine(head, head + perpendicular.normalized * headSize / 2f - direction.normalized * headSize);
        Gizmos.DrawLine(head, head - perpendicular.normalized * headSize / 2f - direction.normalized * headSize);
    }

    public static void GizmosDrawDashedLine(Vector3 from, Vector3 to, float separation, int maxIters = 500)
    {
        float distance = (to - from).magnitude;
        float distanceToA = 0;
        float distanceToB = 0;
        int iter = 0;

        while (distanceToB < distance && iter < maxIters)
        {
            Vector3 offset = Vector3.zero;
            Vector3 pointA = from + (to - from).normalized * (iter) * separation;
            Vector3 pointB = from + (to - from).normalized * (iter + 0.5f) * separation;

            distanceToA = (pointA - from).magnitude;
            distanceToB = (pointB - from).magnitude;

            if (distanceToA < distance)
            {
                if (distanceToB > distance)
                {
                    offset = to - pointB;
                }

                Gizmos.DrawLine(pointA, pointB + offset);
            }

            iter++;
        }
    }

    public static void GizmosDrawCube(Vector3 center, Vector3 scale) => GizmosDrawCube(center, scale, Color.white);

    public static void GizmosDrawCube(Vector3 center, Vector3 scale, Color color, float thickness = -1, CompareFunction compareFunction = CompareFunction.Always)
    {
        Handles.color = color;
        Handles.zTest = compareFunction;

        if (thickness == -1)
        {
            Handles.DrawWireCube(center, scale);

            return;
        }

        scale *= 1f / 2;

        DrawVertex( 1,  1,  1);
        DrawVertex( 1,  1, -1);
        DrawVertex( 1, -1,  1);
        DrawVertex( 1, -1, -1);
        DrawVertex(-1,  1,  1);
        DrawVertex(-1,  1, -1);
        DrawVertex(-1, -1,  1);
        DrawVertex(-1, -1, -1);

        void DrawVertex(float x, float y, float z)
        {
            Vector3 coord = new Vector3(x, y, z);

            DrawEdge(coord, coord.MultipliedBy( 1,  1, -1));
            DrawEdge(coord, coord.MultipliedBy( 1, -1,  1));
            DrawEdge(coord, coord.MultipliedBy(-1,  1,  1));
        }

        void DrawEdge(Vector3 coordsA, Vector3 coordsB)
        {
            Handles.DrawLine(center + scale.MultipliedBy(coordsA), center + scale.MultipliedBy(coordsB), thickness);
        }
    }

    #endregion

    // -------------------------------------------------------------------------------------------------------------------

    #region Other

    public static void DestroyInEditor(UnityEngine.Object obj)
    {
        EditorApplication.delayCall += () => MonoBehaviour.DestroyImmediate(obj);
    }

    public static void DestroyInEditor(Component component, GameObject prefab)
    {
        if (component.gameObject != prefab) return;
        if (PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.NotAPrefab) return;

        MonoBehaviour.DestroyImmediate(component, true);

        PrefabUtility.SavePrefabAsset(prefab);
        AssetDatabase.Refresh();
    }

    #endregion

#endif

    #endregion

}