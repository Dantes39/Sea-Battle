using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Continue : MonoBehaviour
{
    public GameObject panel;

    public void _continue()
    {
        panel.SetActive(false);        
        Time.timeScale = 1f;
    }
}
