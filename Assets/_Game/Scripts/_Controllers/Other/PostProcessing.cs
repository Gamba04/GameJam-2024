using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PostProcessing : MonoBehaviour
{
    [SerializeField]
    private List<Material> shaders = new List<Material>();

    private bool ShadersCheck
    {
        get
        {
            if (shaders.Count > 0)
            {
                for (int i = 0; i < shaders.Count; i++)
                {
                    if (shaders[i] == null) return false;
                }

                return true;
            }

            return false;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (ShadersCheck)
        {
            RenderTexture temp = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);

            // First
            Graphics.Blit(source, temp, shaders[0]);

            // Intermediate
            for (int i = 1; i < shaders.Count - 1; i++)
            {
                Graphics.Blit(temp, temp, shaders[i]);
            }

            // Last
            Graphics.Blit(temp, destination, shaders[shaders.Count - 1]);

            RenderTexture.ReleaseTemporary(temp);
            return;
        }

        Graphics.Blit(source, destination);
    }

}