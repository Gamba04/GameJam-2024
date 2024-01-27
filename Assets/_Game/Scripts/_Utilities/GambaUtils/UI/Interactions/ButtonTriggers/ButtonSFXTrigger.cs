using System.Collections.Generic;
using UnityEngine;

public class ButtonSFXTrigger : ButtonTrigger
{
    [Space]
    [SerializeField]
    private List<SFXTag> hover;
    [SerializeField]
    private List<SFXTag> click;

    protected override void OnHover() => PlaySFX(hover);

    protected override void OnClick() => PlaySFX(click);

    private void PlaySFX(List<SFXTag> sfx)
    {
        if (sfx == null || sfx.Count == 0) return;

        SFXPlayer.PlayRandomSFX(sfx);
    }
}