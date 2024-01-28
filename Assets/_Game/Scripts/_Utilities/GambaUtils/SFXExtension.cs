using System.Collections.Generic;
using UnityEngine;

public class SFXExtension : MonoBehaviour
{
    [SerializeField]
    private List<SFXTag> sfx = new List<SFXTag>();

    public void PlaySFX(int index) => SFXPlayer.PlaySFX(sfx[index]);
}