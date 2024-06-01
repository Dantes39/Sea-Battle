using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Ships")]
    public GameObject[] ships;
    public EnemyScripts enemyScripts;
    private ShipScripts shipScript;
    private List<int[]> enemyShips;
    private int shipIndex = 0;
    public List<TileScript> allTileScript;

    [Header("UI")]
    public Button nextBtn;
    public Button rotateBtn;
    public Button replayBtn;
    public Button replayBtnGG;
    public Text TopText;
    public Text playerShipText;
    public Text enemyShipText;

    [Header("Objects")]
    public GameObject missilePrefab;
    public GameObject enemyMissilePrefab;
    public GameObject woodDock;
    public GameObject firePrefab;
    public GameObject bangPrefab;
    public GameObject waterPrefab;
    public GameObject GGImage;
    public GameObject winImage;
    public GameObject loseImage;
    public Text GGText;

    [Header("Music")]
    public AudioSource musicFx;
    public AudioClip clipBang, clipFail;

    private bool setupComplete = false;
    private bool playerTurn = true;

    private List<GameObject> playerFires = new List<GameObject>();
    private List<GameObject> enemyFires = new List<GameObject>();

    public static char[] guessGrid;

    private int enemyShipCount = 10;
    private int playerShipCount = 10;

    void Start()
    {
        shipScript = ships[shipIndex].GetComponent<ShipScripts>();
        nextBtn.onClick.AddListener(() => NextShipClicked());
        rotateBtn.onClick.AddListener(() => RotateClicked());
        replayBtn.onClick.AddListener(() => ReplayClicked());
        replayBtnGG.onClick.AddListener(() => ReplayClicked());
        enemyShips = enemyScripts.PlaceEnemyShips();
        guessGrid = enemyScripts.guessGrid;
    }

    private void NextShipClicked()
    {
        if (!shipScript.OnGameBoard() || !shipScript.CheckSurroundingTiles())
        {
            shipScript.FlashColor(Color.red);
        }
        else
        {
            if (shipIndex <= ships.Length - 2)
            {
                shipIndex++;
                shipScript = ships[shipIndex].GetComponent<ShipScripts>();
                shipScript.FlashColor(Color.green);
            }
            else
            {
                rotateBtn.gameObject.SetActive(false);
                nextBtn.gameObject.SetActive(false);
                woodDock.SetActive(false);
                TopText.text = "Guess an enemy tile";
                setupComplete = true;

                for (int i = 0; i < ships.Length; i++)
                    ships[i].SetActive(false);
            }
        }
    }

    void RotateClicked()
    {
        shipScript.RotateShip();
    }

    void Update()
    {
    }

    public void TileClicked(GameObject tile)
    {
        if (setupComplete && playerTurn)
        {
            Vector3 tilePos = tile.transform.position;
            tilePos.y += 10;
            playerTurn = false;
            Instantiate(missilePrefab, tilePos, missilePrefab.transform.rotation);
        }
        else if (!setupComplete)
        {
            PlaceShip(tile);
            shipScript.SetClickedTile(tile);
        }
    }

    private void PlaceShip(GameObject tile)
    {
        shipScript = ships[shipIndex].GetComponent<ShipScripts>();
        shipScript.ClearTileList();
        Vector3 newVec = shipScript.GetOffSetVec(tile.transform.position);
        ships[shipIndex].transform.localPosition = newVec;
    }

    public void CheckHit(GameObject tile)
    {
        int tileNum = int.Parse(Regex.Match(tile.name, @"\d+").Value);
        int hitCount = 0;

        foreach (int[] tileNumArray in enemyShips)
        {
            if (tileNumArray.Contains(tileNum))
            {
                for (int i = 0; i < tileNumArray.Length; i++)
                {
                    if (tileNumArray[i] == tileNum)
                    {
                        tileNumArray[i] = -4;
                        hitCount++;
                    }
                    else if (tileNumArray[i] == -4)
                    {
                        hitCount++;
                    }
                }
                if (hitCount == tileNumArray.Length)
                {
                    musicFx.PlayOneShot(clipBang);
                    enemyShipCount--;
                    TopText.text = "SUNK!!!";
                    GameObject bang = Instantiate(bangPrefab, tile.transform.position, bangPrefab.transform.rotation) as GameObject;
                    enemyFires.Add(Instantiate(firePrefab, tile.transform.position, firePrefab.transform.rotation));
                    tile.GetComponent<TileScript>().SetTileColor(1, new Color32(68, 0, 0, 255));
                    tile.GetComponent<TileScript>().SwitchColors(1);
                    Destroy(bang, 1f);
                }
                else
                {
                    musicFx.PlayOneShot(clipBang);
                    TopText.text = "HIT!!! Keep Going!";
                    GameObject bang = Instantiate(bangPrefab, tile.transform.position, bangPrefab.transform.rotation) as GameObject;
                    tile.GetComponent<TileScript>().SetTileColor(1, new Color32(255, 0, 0, 255));
                    tile.GetComponent<TileScript>().SwitchColors(1);
                    Destroy(bang, 1f);
                }
                playerTurn = true;
                return;
            }
        }
        if (hitCount == 0)
        {
            musicFx.PlayOneShot(clipFail);
            GameObject water = Instantiate(waterPrefab, tile.transform.position, bangPrefab.transform.rotation) as GameObject;
            tile.GetComponent<TileScript>().SetTileColor(1, new Color32(38, 57, 76, 255));
            tile.GetComponent<TileScript>().SwitchColors(1);
            TopText.text = "Missed...";
            Destroy(water, 1f);
            Invoke(nameof(EndPlayerTurn), 2.0f);
        }
        if (enemyShipCount < 1)
        {
            enemyShipText.text = enemyShipCount.ToString();
            GameOver(true);
        }
    }

    public void EnemyHitPlayer(Vector3 tile, int tileNum, GameObject hitObj)
    {
        tile.y += 0.2f;
        playerFires.Add(Instantiate(firePrefab, tile, firePrefab.transform.rotation));
        musicFx.PlayOneShot(clipBang);
        GameObject bang = Instantiate(bangPrefab, tile, bangPrefab.transform.rotation) as GameObject;
        guessGrid[tileNum] = 'h';

        if (hitObj.GetComponent<ShipScripts>().HitCheckSank())
        {
            playerShipCount--;
            playerShipText.text = playerShipCount.ToString();
            enemyScripts.SunkPlayer();
        }
        Destroy(bang, 1f);
        if (playerShipCount < 1)
        {
            playerShipText.text = playerShipCount.ToString();
            enemyScripts.SunkPlayer();
            GameOver(false);
        }
        else
        {
            TopText.text = "Enemy Hit!";
            enemyScripts.NPCTurn();
        }
    }

    void ShowWinImage()
    {
        winImage.SetActive(true);
    }

    void ShowLoseImage()
    {
        loseImage.SetActive(true);
    }

    void GameOver(bool playerWins)
    {
        playerTurn = false;
        if (playerWins)
        {
            ShowWinImage();
        }
        else
        {
            ShowLoseImage();
        }
    }

    public void EndPlayerTurn()
    {
        for (int i = 0; i < ships.Length; i++)
            ships[i].SetActive(true);

        foreach (GameObject fire in playerFires)
            fire.SetActive(true);

        foreach (GameObject fire in enemyFires)
            fire.SetActive(false);

        enemyShipText.text = enemyShipCount.ToString();
        TopText.text = "Enemy's turn";
        ColorAllTiles(0);
        enemyScripts.NPCTurn();
    }

    public void EndEnemyTurn()
    {
        for (int i = 0; i < ships.Length; i++)
            ships[i].SetActive(false);

        foreach (GameObject fire in playerFires)
            fire.SetActive(false);

        foreach (GameObject fire in enemyFires)
            fire.SetActive(true);

        playerShipText.text = playerShipCount.ToString();
        TopText.text = "Select a tile";
        playerTurn = true;
        ColorAllTiles(1);
    }

    private void ColorAllTiles(int colorIndex)
    {
        foreach (TileScript tileScript in allTileScript)
        {
            tileScript.SwitchColors(colorIndex);
        }
    }

    void ReplayClicked()
    {
        Time.timeScale = 1f;
        Invoke("Replay", 0.05f);
    }

    void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
