using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Manages the main game state
//  Includes transitions between levels, player death
public class GameManager : MonoBehaviour
{
    [SerializeField] private List<int> m_LostMothCountWinConditions = new List<int> { 3, 5, 7 };

    public delegate void NextLevelDelegate();
    public NextLevelDelegate d_NextLevelDelegate; 

    private int m_CurrLevel = 0;

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
        GameState.m_GameState = GameStateEnum.RUNNING;
    }

    // On player dying, lose
    public void OnGameOver()
    {
        GameState.m_GameState = GameStateEnum.LOST;
        SceneManager.LoadScene("WinLose");
        m_CurrLevel = 0;
    }

    // On player winning the game
    public void OnGameWon()
    {
        GameState.m_GameState = GameStateEnum.WON;
        SceneManager.LoadScene("WinLose");
        m_CurrLevel = 0; 
    }

    // Enough lost moths collected to either goto next level or game finished
    public void OnAllLostMothsCollected()
    {
        // if last tile set finished, then game won
        TileManager.PropertyInstance.TileSetFinished(); 
        if (GameState.m_GameState != GameStateEnum.WON && GameState.m_GameState != GameStateEnum.LOST)
        {
            m_CurrLevel++;
            GameState.m_GameState = GameStateEnum.TRANSITIONING;
            if (d_NextLevelDelegate != null)
                d_NextLevelDelegate();
        }
    }

    // Gets the moth with condition count based on the curr set level
    public int CurrLostMothWinCondition()
    {
        return m_LostMothCountWinConditions[m_CurrLevel];
    }

    public int CurrLevel { get { return m_CurrLevel; }}
}
