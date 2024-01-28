using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private Text text;
    [SerializeField]
    private Image player;

    private bool restartAvailable;

    #region Update

    private void Update()
    {
        UpdateInputs();   
    }

    private void UpdateInputs()
    {
        if (restartAvailable && Input.anyKeyDown)
        {
            if (!UIManager.IsOnTransition)
            {
                UIManager.SetFade(true, GambaFunctions.ReloadScene);
            }
        }
    }

    #endregion

    // ----------------------------------------------------------------------------------------------------------------------------

    #region Public Methods

    public void Play(int playerID, CharacterInfo info)
    {
        anim.SetTrigger("Play");

        text.text = $"PLAYER {playerID} WINS!!!!";
        player.sprite = info.winSprite;
        SFXPlayer.PlaySFX(info.sfx);
    }

    public void EnableRestart()
    {
        restartAvailable = true;
    }

    #endregion

}