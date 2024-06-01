using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void _menu(int index)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(index);
    }

    public void _exit()
    {
        Application.Quit();
    }
}
