using System.Collections.Generic;
using UnityEngine;

public class ShipScripts : MonoBehaviour
{
    public float xOffset = 0;
    public float zOffset = 0;
    private float nextYRotation = 90f;

    private GameObject clickedTile;
    int hitCount = 0;
    public int shipSize;

    private Material[] allMaterials;

    List<Color> allColors = new List<Color>();
    List<GameObject> touchTiles = new List<GameObject>();

    private void Start()
    {
        allMaterials = GetComponent<Renderer>().materials;
        for (int i = 0; i < allMaterials.Length; i++)
            allColors.Add(allMaterials[i].color);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Tile"))
        {
            touchTiles.Add(collision.gameObject);
        }
    }

    public void ClearTileList()
    {
        touchTiles.Clear();
    }

    public Vector3 GetOffSetVec(Vector3 tilePos)
    {
        return new Vector3(tilePos.x + xOffset, 2, tilePos.z + zOffset);
    }

    public void RotateShip()
    {
        if (clickedTile == null) return;
        touchTiles.Clear();
        transform.localEulerAngles += new Vector3(0, nextYRotation, 0);
        nextYRotation *= -1;

        float temp = xOffset;
        xOffset = zOffset;
        zOffset = temp;
        SetPosition(clickedTile.transform.position);
    }

    public void SetPosition(Vector3 newVec)
    {
        ClearTileList();
        Vector3 previousPosition = transform.localPosition;
        transform.localPosition = new Vector3(newVec.x + xOffset, 2, newVec.z + zOffset);

        if (!CheckSurroundingTiles())
        {
            // Если проверка не прошла, вернуть корабль на предыдущую позицию или вывести сообщение об ошибке
            transform.localPosition = previousPosition;
            Debug.Log("Корабль не может быть размещен слишком близко к другому кораблю.");
        }
    }

    public void SetClickedTile(GameObject tile)
    {
        clickedTile = tile;
    }

    public bool OnGameBoard()
    {
        return touchTiles.Count == shipSize;
    }

    public bool HitCheckSank()
    {
        hitCount++;
        return shipSize <= hitCount;
    }

    public void FlashColor(Color tempColor)
    {
        foreach (Material mat in allMaterials)
        {
            mat.color = tempColor;
        }
        Invoke("ResetColor", 0.4f);
    }

    private void ResetColor()
    {
        int i = 0;
        foreach (Material mat in allMaterials)
        {
            mat.color = allColors[i++];
        }
    }

    public bool CheckSurroundingTiles()
    {
        foreach (var tile in touchTiles)
        {
            Vector3 tilePos = tile.transform.position;

            // Проверка всех соседних клеток вокруг каждой клетки корабля
            if (!IsTileFree(tilePos + new Vector3(1, 0, 0)) || // Right
                !IsTileFree(tilePos + new Vector3(-1, 0, 0)) || // Left
                !IsTileFree(tilePos + new Vector3(0, 0, 1)) || // Up
                !IsTileFree(tilePos + new Vector3(0, 0, -1)) || // Down
                !IsTileFree(tilePos + new Vector3(1, 0, 1)) || // Up-Right
                !IsTileFree(tilePos + new Vector3(-1, 0, 1)) || // Up-Left
                !IsTileFree(tilePos + new Vector3(1, 0, -1)) || // Down-Right
                !IsTileFree(tilePos + new Vector3(-1, 0, -1))) // Down-Left
            {
                return false;
            }
        }
        return true;
    }

    public bool IsTileFree(Vector3 pos)
    {
        float tileRadius = 0.5f; // Пример радиуса плитки
        Collider[] colliders = Physics.OverlapSphere(pos, tileRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Ship") && collider.gameObject != gameObject)
            {
                return false;
            }
        }
        return true;
    }
}
