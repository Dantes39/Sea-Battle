using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyScripts : MonoBehaviour
{
    public char[] guessGrid;
    List<int> potentialHits;
    List<int> currentHits;
    private int guess;
    public GameObject enemyMissilePrefab;
    public GameManager gameManager;

    private void Awake()
    {
        potentialHits = new List<int>();
        currentHits = new List<int>();
        guessGrid = Enumerable.Repeat('o', 100).ToArray();
    }

    public List<int[]> PlaceEnemyShips()
    {
        List<int[]> enemyShips = new List<int[]>
        {
            new int[] { -1, -1, -1, -1},
            new int[] { -1, -1, -1},
            new int[] { -1, -1, -1},
            new int[] { -1, -1},
            new int[] { -1, -1},
            new int[] { -1, -1},
            new int[] { -1},
            new int[] { -1},
            new int[] { -1},
            new int[] { -1},
        };

        int[] gridNumbers = Enumerable.Range(1, 100).ToArray();

        foreach (int[] tileNumArray in enemyShips)
        {
            bool taken = true;
            while (taken)
            {
                taken = false;
                int shipNose = Random.Range(0, 101);
                int rotateBool = Random.Range(0, 2);

                int minusAmount = rotateBool == 0 ? 10 : 1;

                for (int i = 0; i < tileNumArray.Length; i++)
                {
                    int index = shipNose - i * minusAmount;
                    if (index < 0 || index >= 100 || gridNumbers[index] < 0 ||
                        (rotateBool == 1 && shipNose / 10 != (index - 1) / 10))
                    {
                        taken = true;
                        break;
                    }
                }

                if (!taken)
                {
                    for (int j = 0; j < tileNumArray.Length; j++)
                    {
                        int index = shipNose - j * minusAmount;
                        tileNumArray[j] = index;
                        gridNumbers[index] = -1;
                        MarkSurroundingTilesAsOccupied(gridNumbers, index, rotateBool);
                    }
                }
            }
        }

        PrintGridAndShips(gridNumbers, enemyShips);
        return enemyShips;
    }

    private void MarkSurroundingTilesAsOccupied(int[] gridNumbers, int index, int rotateBool)
    {
        int[] neighbors;
        if (rotateBool == 0)
        {
            neighbors = new int[] { index - 10, index + 10, index - 1, index + 1,
                                    index - 9, index - 11, index + 9, index + 11 };
        }
        else
        {
            neighbors = new int[] { index - 10, index + 10, index - 1, index + 1,
                                    index - 9, index - 11, index + 9, index + 11 };
        }

        foreach (int neighbor in neighbors)
        {
            if (neighbor >= 0 && neighbor < 100 && gridNumbers[neighbor] != -1)
            {
                gridNumbers[neighbor] = -2;
            }
        }
    }

    private bool IsTileOccupied(int tileIndex)
    {
        if (tileIndex < 0 || tileIndex >= 100)
        {
            return false;
        }

        int[] gridNumbers = Enumerable.Range(1, 100).ToArray();
        return gridNumbers[tileIndex] < 0;
    }

    private void HighlightTile(int tileIndex)
    {
        string tileName = "sea (" + (tileIndex) + ")";
        GameObject tile = GameObject.Find(tileName);

        if (tile != null)
        {
            tile.GetComponent<Renderer>().material.color = Color.green; // Изменить цвет клетки
        }
    }

    public void NPCTurn()
    {
        List<int> hitIndex = new List<int>();

        for (int i = 0; i < guessGrid.Length; i++)
        {
            if (guessGrid[i] == 'h')
                hitIndex.Add(i);
        }

        if (hitIndex.Count > 1)
        {
            int diff = hitIndex[1] - hitIndex[0];
            int posNeg = Random.Range(0, 2) * 2 - 1;
            int nextIndex = hitIndex[0] + diff;

            while (nextIndex > 99 || nextIndex < 0 || guessGrid[nextIndex] != 'o')
            {
                if (nextIndex > 99 || nextIndex < 0 || guessGrid[nextIndex] == 'm')
                {
                    diff *= -1;
                }
                nextIndex += diff;
            }
            guess = nextIndex;
        }
        else if (hitIndex.Count == 1)
        {
            List<int> closeTiles = new List<int> { 1, -1, 10, -10 };

            int index = Random.Range(0, closeTiles.Count);
            int possibleGuess = hitIndex[0] + closeTiles[index];

            bool onGrid = possibleGuess >= 0 && possibleGuess < 100;

            while ((!onGrid || guessGrid[possibleGuess] != 'o') && closeTiles.Count > 0)
            {
                closeTiles.RemoveAt(index);
                if (closeTiles.Count == 0) break;
                index = Random.Range(0, closeTiles.Count);
                possibleGuess = hitIndex[0] + closeTiles[index];
                onGrid = possibleGuess >= 0 && possibleGuess < 100;
            }
            guess = possibleGuess;
        }
        else
        {
            int nextIndex = Random.Range(0, 100);
            while (guessGrid[nextIndex] != 'o')
                nextIndex = Random.Range(0, 100);
            nextIndex = GuessAgainCheck(nextIndex);
            guess = nextIndex;
        }

        Debug.Log("Next guess: " + (guess + 1));

        GameObject tile = GameObject.Find("sea (" + (guess + 1) + ")");
        guessGrid[guess] = 'm';

        string grid = "";
        grid += "guessGrid" + "\n";

        for (int i = 9; i >= 0; i--)
        {
            string row = "ряд " + (i + 1) + ": ";
            for (int j = 0; j < 10; j++)
            {
                row += guessGrid[i * 10 + j] + " ";
            }
            grid += row + "\n";
        }

        Debug.Log(grid);
        Debug.Log("Hit indices: " + string.Join(", ", hitIndex));

        Vector3 vec = tile.transform.position;
        vec.y += 15;
        GameObject missile = Instantiate(enemyMissilePrefab, vec, enemyMissilePrefab.transform.rotation);
        missile.GetComponent<EnemyMissileScript>().SetTarget(guess);
        missile.GetComponent<EnemyMissileScript>().targetTileLocation = tile.transform.position;
    }


    private int GuessAgainCheck(int nextIndex)
    {
        int newGuess = nextIndex;
        bool edgeCase = nextIndex < 10 || nextIndex > 89 || nextIndex % 10 == 0 || nextIndex % 10 == 9;
        bool nearGuess = false;
        if (nextIndex + 1 < 100) nearGuess = guessGrid[nextIndex + 1] != 'o';
        if (!nearGuess && nextIndex - 1 > 0) nearGuess = guessGrid[nextIndex - 1] != 'o';
        if (!nearGuess && nextIndex + 10 < 100) nearGuess = guessGrid[nextIndex + 10] != 'o';
        if (!nearGuess && nextIndex - 10 > 0) nearGuess = guessGrid[nextIndex - 10] != 'o';
        if (edgeCase || nearGuess) newGuess = Random.Range(0, 100);
        while (guessGrid[newGuess] != 'o') newGuess = Random.Range(0, 100);
        return newGuess;
    }

    public void MissileHit(int guess)
    {
        guessGrid[guess] = 'h';
        Invoke("EndTurn", 1.0f);
    }

    public void SunkPlayer()
    {
        for (int i = 0; i < guessGrid.Length; i++)
        {
            if (guessGrid[i] == 'h')
            {
                guessGrid[i] = 'x';

            }
        }
        for (int i = 0; i < guessGrid.Length; i++)
        {
            if (guessGrid[i] == 'x')
            {
                List<int> list = new List<int>() {-11, -10, -9, -1, 0, 1, 9, 10, 11};
                foreach (int j in list)
                {
                    if ((i + j) < 0 || 99 < (i + j)) continue;
                    else if (guessGrid[i + j] != 'x') guessGrid[i + j] = 'm';
                    string tileName = "sea (" + (i + 1 + j) + ")";
                    GameObject tile = GameObject.Find(tileName);
                    tile.GetComponent<TileScript>().SetTileColor(0, new Color32(38, 57, 76, 255));
                }
            }
        }
    }

    private void EndTurn()
    {
        gameManager.GetComponent<GameManager>().EndEnemyTurn();
    }

    public void PauseAndEnd(int miss)
    {
        if (currentHits.Count > 0 && currentHits[0] > miss)
        {
            foreach (int potential in potentialHits)
            {
                if (currentHits[0] > miss)
                {
                    if (potential < miss) potentialHits.Remove(potential);
                }
                else
                {
                    if (potential > miss) potentialHits.Remove(potential);
                }
            }
        }
        Invoke("EndTurn", 1.0f);
    }

    private void PrintGridAndShips(int[] gridNumbers, List<int[]> enemyShips)
    {
        string gridOutput = "";

        for (int i = 9; i >= 0; i--)
        {
            string row = $"{i + 1} ряд: ";
            for (int j = 0; j < 10; j++)
            {
                int index = i * 10 + j;
                row += gridNumbers[index] == -1 ? "X " : "O ";
            }
            gridOutput += row + "\n";
        }

        gridOutput += "Enemy Ships:\n";
        foreach (int[] ship in enemyShips)
        {
            string shipInfo = "";
            foreach (int tileNum in ship)
            {
                shipInfo += tileNum + ", ";
            }
            gridOutput += shipInfo.TrimEnd(',', ' ') + "\n";
        }

        Debug.Log(gridOutput);
    }

}
