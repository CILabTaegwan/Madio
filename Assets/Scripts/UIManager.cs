using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Player player;
    public GameManager gameManager;

    public Text livesText;
    public Text coinsText;
    public Slider decibel;
    public RawImage left;
    public RawImage right;
    public RawImage scream;


    // Start is called before the first frame update
    void Start()
    {
        left = GetComponent<RawImage>();
        right = GetComponent<RawImage>();
        scream = GetComponent<RawImage>();
        decibel = GetComponent<Slider>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        player = GameObject.Find("Mario").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        livesText.text = gameManager.lives.ToString();
        //Debug.Log(gameManager.lives);
        coinsText.text = gameManager.coins.ToString();
    }
}
