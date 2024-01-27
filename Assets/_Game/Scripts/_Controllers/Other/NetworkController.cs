using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

public class NetworkController : MonoBehaviour
{
    [Serializable]
    private class StringEvent : UnityEvent<string> { }

    #region Serializable

    [Serializable]
    public class TargetData
    {
        [SerializeField, HideInInspector] private string title;

        public string key;
        public string url;

        [Space]
        public List<RequestHeader> getHeaders;
        public List<RequestHeader> putHeaders;

        public void SetName()
        {
            if (key == "") title = "None";
            else title = key;

            for (int i = 0; i < getHeaders.Count; i++) getHeaders[i].SetName();
            for (int i = 0; i < putHeaders.Count; i++) putHeaders[i].SetName();
        }
    }

    [Serializable]
    public class RequestHeader
    {
        [SerializeField, HideInInspector] private string title;

        public string name;
        public string value;

        public void SetName()
        {
            if (name == "") title = "None";
            else title = name;
        }
    }

    #endregion

    [SerializeField]
    private List<TargetData> targetData = new List<TargetData>();

    [Space(20)]
    [SerializeField]
    private UnityEvent onNetworkLoading;
    [SerializeField]
    private UnityEvent onNetworkSuccess;
    [SerializeField]
    private StringEvent onNetworkError;

    #region Singleton

    private static NetworkController instance = null;

    public static NetworkController Instance
    {
        get
        {
            if (instance == null)
            {
                var sceneResult = FindObjectOfType<NetworkController>();

                if (sceneResult != null)
                {
                    instance = sceneResult;
                }
                else
                {
                    GameObject obj = new GameObject($"{GetTypeName(instance)}_Instance");
                    instance = obj.AddComponent<NetworkController>();
                }
            }

            return instance;
        }
    }

    private static string GetTypeName<T>(T obj) => typeof(T).Name;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    #endregion

    #region Requests

    private IEnumerator Get(string key, Action<string> onSuccess = null, Action<string> onError = null)
    {
        int targetIndex = targetData.FindIndex(t => t.key == key);

        if (targetIndex != -1)
        {
            onNetworkLoading?.Invoke();

            TargetData target = targetData[targetIndex];

            // Setup Request
            UnityWebRequest request = UnityWebRequest.Get(target.url);

            foreach (RequestHeader header in target.getHeaders) request.SetRequestHeader(header.name, header.value);

            // Send Request
            yield return request.SendWebRequest();

            // Receive Request
            if (request.error != "")
            {
                if (onError == null) Debug.LogError(request.error);
                else onError(request.error);

                onNetworkError?.Invoke(request.error);
            }
            else
            {
                onSuccess?.Invoke(request.downloadHandler.text);

                onNetworkSuccess?.Invoke();
            }
        }
        else Debug.LogError($"Key '{key}' not found. Please add it on the inspector.");
    }

    private IEnumerator Put(string key, string data, Action onSuccess = null, Action<string> onError = null)
    {
        int targetIndex = targetData.FindIndex(t => t.key == key);

        if (targetIndex != -1)
        {
            onNetworkLoading?.Invoke();

            TargetData target = targetData[targetIndex];

            // Setup Request
            UnityWebRequest request = UnityWebRequest.Put(target.url, data);

            foreach (RequestHeader header in target.putHeaders) request.SetRequestHeader(header.name, header.value);

            // Send Request
            yield return request.SendWebRequest();

            // Receive Request
            if (request.error != "")
            {
                if (onError == null) Debug.LogError(request.error);
                else onError(request.error);

                onNetworkError?.Invoke(request.error);
            }
            else
            {
                onSuccess?.Invoke();

                onNetworkSuccess?.Invoke();
            }
        }
        else Debug.LogError($"Key '{key}' not found. Please add it on the inspector.");
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Static Methods

    /// <summary> Request data using HTTP GET. </summary>
    public static void GetData<T>(string key, Action<T> onSuccess = null, Action<string> onError = null)
    {
        Instance.StartCoroutine(Instance.Get(key, data => onSuccess?.Invoke(JsonUtility.FromJson<T>(data)), onError));
    }

    /// <summary> Update data using HTTP PUT. </summary>
    public static void PutData<T>(string key, T data, Action onSuccess = null, Action<string> onError = null)
    {
        Instance.StartCoroutine(Instance.Put(key, JsonUtility.ToJson(data, true), onSuccess, onError));
    }

    public static void CancelAllRequests()
    {
        Instance.StopAllCoroutines();
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        for (int i = 0; i < targetData.Count; i++)
        {
            targetData[i].SetName();
        }
    }

#endif

    #endregion

}