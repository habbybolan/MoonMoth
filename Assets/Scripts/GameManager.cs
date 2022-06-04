using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Manages the main game state
//  Includes transitions between levels, player death
public class GameManager : MonoBehaviour
{ 
    static GameManager s_PropertyInstance;
    public static GameManager PropertyInstance
    {
        get { return s_PropertyInstance; }
    }

    private void Awake()
    {
        // Singleton
        if (s_PropertyInstance != null && s_PropertyInstance != this)
            Destroy(this);
        else
            s_PropertyInstance = this;
    }

    // On player dying, lose
    public void OnGameOver()
    {
        GameState.m_GameState = GameStateEnum.LOST;
        SceneManager.LoadScene("WinLose");
    }

    // On player winning the game
    public void OnGameWon()
    {
        GameState.m_GameState = GameStateEnum.WON;
        SceneManager.LoadScene("WinLose");
    }

    // Enough lost moths collected to either goto next level or game finished
    public void OnAllLostMothsCollected()
    {
        // if last tile set finished, then game won
        TileManager.PropertyInstance.TileSetFinished(); 
        if (GameState.m_GameState != GameStateEnum.WON && GameState.m_GameState != GameStateEnum.LOST)
        {
            GameState.m_GameState = GameStateEnum.TRANSITIONING;
            FireflyManager.PropertyInstance.TileSetFinished();
        }
    }
}
