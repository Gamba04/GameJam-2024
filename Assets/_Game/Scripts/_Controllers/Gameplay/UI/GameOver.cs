using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Text text;
    [SerializeField]
    private Animator anim;

    public void Play(int playerID)
    {
        text.text = $"PLAYER {playerID} WINS!!!!";
        anim.SetTrigger("Play");
    }
}