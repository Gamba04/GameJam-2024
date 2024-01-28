using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDController : UISection
{
    [Header("Components")]
    [SerializeField]
    private GameObject root;

    #region Public Methods

    public override void SetVisible(bool value, bool instant = false)
    {
        root.SetActive(value);
    }

    #endregion

}