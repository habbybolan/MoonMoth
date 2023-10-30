using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [SerializeField] private PlayerController m_PlayerController;

    static GameState s_PropertyInstance;
    public static GameState PropertyInstance
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

    public PlayerController PlayerController
    {
        get { return m_PlayerController; }
    }

    protected GameStateEnum m_GameState = GameStateEnum.NOTHING;
    public GameStateEnum GameStateEnum
    {
        get { return m_GameState; }
    }

    public delegate void GameWonDelegate();
    public GameWonDelegate d_GameWonDelegate;

    public delegate void GameLostDelegate();
    public GameLostDelegate d_GameLostDelegate;

    public delegate void GameRunningDelegate();
    public GameRunningDelegate d_GameRunningDelegate;

    public delegate void GameTransitioningDelegate();
    public GameTransitioningDelegate d_GameTransitioningDelegate;

    public delegate void GameTutorialDelegate();
    public GameTutorialDelegate d_GameTutorialDelegate;

    // Don't manually call. Call GameManager.UpdateState for updating the state
    public void UpdateState(GameStateEnum newState)
    {
        if (newState == m_GameState) return;

        m_GameState = newState;
        OnStateSet();
    }

    private void OnStateSet()
    {
        switch(m_GameState)
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
        if (d_GameWonDelegate != null)
            d_GameWonDelegate();
    }

    private void OnGameLost()
    {
        if (d_GameLostDelegate != null)
            d_GameLostDelegate();
    }

    private void OnTutorialStarted()
    {
        if (d_GameTutorialDelegate != null)
            d_GameTutorialDelegate();
    }

    private void OnGameRunning()
    {
        if (d_GameRunningDelegate != null)
            d_GameRunningDelegate();
    }

    private void OnTransitioning()
    {
        if (d_GameTransitioningDelegate != null)
            d_GameTransitioningDelegate();
    }

    // **** End Game state events
}

public enum GameStateEnum
{
    NOTHING,
    WON,
    LOST,
    TRANSITIONING,
    RUNNING,
    TUTORIAL
}
