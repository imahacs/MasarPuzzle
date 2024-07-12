using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinGame : MonoBehaviour
{
    public void WinGameButton()
    {
        Debug.Log("You Win!");
        // Load the next scene
        SceneManager.LoadScene(0);
    }
}
