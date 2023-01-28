using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Manages the main game state
//  Includes transitions between levels, player death
public class GameManager : MonoBehaviour
{
    [SerializeField] private List<int> m_LostMothCountWinConditions = new List<int> { 3, 5, 7 };

    private int m_CurrLevel = 0;
    private int m_LostMothCount = 0;
    public int LostMothCount
    {
        get { return m_LostMothCount; }
    }

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

    private void Start()
    {
        UpdateState(GameStateEnum.TUTORIAL);
        PlayerManager.PropertyInstance.PlayerController.Health.d_DeathDelegate += OnPlayerDeath;
        PlayerManager.PropertyInstance.PlayerController.lostMothCollectedDelegate += OnLostMothCollected;
    }

    public void UpdateState(GameStateEnum newState)
    {
        GameState.PropertyInstance.UpdateState(newState);
        OnStateSet();
    }

    private void OnStateSet()
    {
        switch (GameState.PropertyInstance.GameStateEnum)
        {
            case GameStateEnum.TUTORIAL:
                OnTutorialStarted();
                break;
            case GameStateEnum.RUNNING:
                OnGameRunning();
                break;
            case GameStateEnum.TRANSITIONING:
                OnTransitioning();
                break;
            case GameStateEnum.LOST:
                OnGameLost();
                break;
            case GameStateEnum.WON:
                OnGameWon();
                break;
            default:
                break;
        }
    }

    // **** Game state events

    private void OnGameWon()
    {
        // TODO: Add delay for death animation
        SceneManager.LoadScene("WinLose");
        m_CurrLevel = 0;
    }

    private void OnGameLost()
    {
        SceneManager.LoadScene("WinLose");
        m_CurrLevel = 0;
    }

    private void OnTutorialStarted()
    {
        // TODO:
    }

    private void OnGameRunning()
    {
        // TODO:
    }

    private void OnTransitioning()
    {
        m_LostMothCount = 0;
    }

    private void OnPlayerDeath()
    {
        UpdateState(GameStateEnum.LOST);
    }

    // **** End Game state events

    private void OnLostMothCollected()
    {
        m_LostMothCount++;
        // if not currently transitioning and met the lost moth win threshold for the tile set
        if (m_LostMothCount >= CurrLostMothWinCondition())
        {
            OnAllLostMothsCollected();
        }
    }

    // Enough lost moths collected to either goto next level or game finished
    public void OnAllLostMothsCollected()
    {
        // if last tile set finished, then game won
        if (GameState.PropertyInstance.GameStateEnum != GameStateEnum.WON && GameState.PropertyInstance.GameStateEnum != GameStateEnum.LOST)
        {
            m_CurrLevel++;
            if (m_CurrLevel == 3)
            {
                UpdateState(GameStateEnum.WON);
            } else
            {
                UpdateState(GameStateEnum.TRANSITIONING);
            }
        }
    }

    // Gets the moth with condition count based on the curr set level
    public int CurrLostMothWinCondition()
    {
        return m_LostMothCountWinConditions[m_CurrLevel];
    }

    public int CurrLevel { get { return m_CurrLevel; }}
}
