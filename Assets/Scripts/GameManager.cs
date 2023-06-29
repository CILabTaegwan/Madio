using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int world { get; private set; }
    public int stage { get; private set; }
    public int lives { get; private set; }
    public int coins { get; private set; }

    public AudioSource deathSound;

    private PlayerMovement player;

    private void Awake()
    {
        if (Instance != null) {
            // DestroyImmediate(gameObject);
        } else {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();

        NewGame();
    }

    public void NewGame()
    {
        lives = 3;
        coins = 0;
        player.Reset();
    }

    public void GameOver()
    {
        // TODO: show game over screen

        NewGame();
    }

    public void LoadLevel(int world, int stage)
    {
        this.world = world;
        this.stage = stage;

        SceneManager.LoadScene($"{world}-{stage}");
    }

    public void NextLevel()
    {
        LoadLevel(world, stage + 1);
    }

    public void ResetLevel(float delay)
    {

        Invoke(nameof(ResetLevel), delay);
    }

    public void ResetLevel()
    {
        lives--;

        player.gameObject.SetActive(true);

        if (lives > 0) {
            player.Resque();
        } else {
            GameOver();
        }

        Debug.Log("Resetlevel");
        if (deathSound != null)
        {
            deathSound.Play();
        }
    }

    public void AddCoin()
    {
        coins++;

        if (coins == 100)
        {
            coins = 0;
            AddLife();
        }
    }

    public void AddLife()
    {
        lives++;
    }

}
