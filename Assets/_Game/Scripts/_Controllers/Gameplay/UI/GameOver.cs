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

    public void Play(int playerID, Sprite sprite)
    {
        anim.SetTrigger("Play");

        text.text = $"PLAYER {playerID} WINS!!!!";
        player.sprite = sprite;
    }
}