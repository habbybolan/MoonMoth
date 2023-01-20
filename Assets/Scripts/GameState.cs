using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState 
{
    public static GameStateEnum m_GameState = GameStateEnum.RUNNING; 
}

public enum GameStateEnum
{
    WON,
    LOST,
    TRANSITIONING,
    RUNNING
}
