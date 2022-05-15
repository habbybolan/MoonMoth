using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Health
{

    public enum HEALTH_STATE
    {
        VULNERABLE,
        DAMAGED // invincibility frames
    }

    public enum DAMAGE_TYPE
    {
        TERRAIN,
        OBSTACLE
    }
}
