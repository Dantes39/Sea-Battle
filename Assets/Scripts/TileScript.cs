using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    GameManager gameManager;
    public AudioSource musicFx;
    public AudioClip clipFail;
    public GameObject waterPrefab;
    Ray ray;
    RaycastHit hit;

    private bool missileHit = false;
    Color32[] hitColor = new Color32[2];

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        hitColor[0] = gameObject.GetComponent<MeshRenderer>().material.color;
        hitColor[1] = gameObject.GetComponent<MeshRenderer>().material.color;
    }

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) // в hit записывается куда попал луч
        {
            if (Input.GetMouseButtonDown(0) && hit.collider.gameObject.name == gameObject.name)
            {
                if (missileHit == false)
                {
                    gameManager.TileClicked(hit.collider.gameObject);
                }
            }
        }
    }

    /* void Update()
     {
         foreach (var touch in Input.touches)
         {
             if (touch.phase == TouchPhase.Began)
             {
                 ray = Camera.main.ScreenPointToRay(touch.position);
                 if (Physics.Raycast(ray, out hit))
                 {
                     if (touch.phase == TouchPhase.Began && hit.collider.gameObject.name == gameObject.name)
                     {
                         if (missileHit == false)
                         {
                             gameManager.TileClicked(hit.collider.gameObject);
                         }
                     }
                 }
             }
         }
     }*/

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Missile"))
        {
            missileHit = true;
        }
        else if (collision.gameObject.CompareTag("EnemyMissile"))
        {
            musicFx.PlayOneShot(clipFail);
            GameObject water = Instantiate(waterPrefab, collision.transform.position, waterPrefab.transform.rotation) as GameObject;
            hitColor[0] = new Color32(38, 57, 76, 255);
            GetComponent<Renderer>().material.color = hitColor[0];
            Destroy(water, 1f);
        }
    }

    public void SetTileColor(int index, Color32 color)
    {
        hitColor[index] = color;
    }

    public void SwitchColors(int colorIndex)
    {
        GetComponent<Renderer>().material.color = hitColor[colorIndex];
    }
}