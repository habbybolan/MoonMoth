using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderController : CharacterController<SpiderHealth>
{
    [SerializeField] private SpiderMovement m_SpiderMovement;
    [SerializeField] private SpiderWeapon m_SpiderWeapon;
    [SerializeField] private SpiderWeb m_SpiderWeb;
    [SerializeField] private float m_ShootDistance = 50f;
    [SerializeField] private float m_DistancePastCameraToStartShooting = 10f;

    private int m_TileID;           // ID of the tile the spider spawned on, used for deleting spiders when tile deleted
    private SpiderState m_State;    // Current state of the spider

    private Vector3 m_OriginalLocalPosition;
    private Quaternion m_OriginalRotation;

    private void Awake()
    {
        m_OriginalLocalPosition = transform.localPosition;
        m_OriginalRotation = transform.localRotation;
    }

    protected override void Start()
    {
        m_State = SpiderState.HANGING;
        base.Start();

        // delegate for when the web attached to this spider is broken
        m_SpiderWeb.d_WebDestroyedDelegate = SetFallingState;
    }

    private void SetFallingState()
    {
        m_State = SpiderState.FALLING;
    }

    private void Update()
    {
        // if spider is in falling state (web destroyed), don't shoot anymore
        if (m_State == SpiderState.FALLING)
        {
            return;
        }

        if (PlayerManager.PropertyInstance.PlayerController && PlayerManager.PropertyInstance.PlayerController.DistanceFromPlayer(transform.position) < m_ShootDistance)
        {
            m_SpiderMovement.LookTowardPlayer();
            // only shoot at player if passed a certain distance from camera
            if (PlayerManager.PropertyInstance.PlayerController.ZDistanceFromPlayerCamera(transform.position) > m_DistancePastCameraToStartShooting)
            {
                m_SpiderWeapon.ShootAtPlayer();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if spider collides with terrain while falling, then destroy it
        if (m_State == SpiderState.FALLING)
        {
            // TODO: Change collision check for terrain and obstacles (checking for health)
            if (collision.gameObject.tag == "Terrain")
            {
                Death();
            }
        }
    }

    public override void Death()
    {
        // let tile destroy object
        m_SpiderWeb.RemoveSpiderFromWeb();
        GetComponent<Rigidbody>().useGravity = false;
        transform.position = Vector3.zero;
    }

    private void OnEnable()
    {
        transform.localRotation = m_OriginalRotation;
        transform.localPosition = m_OriginalLocalPosition;
        m_State = SpiderState.HANGING;
        m_SpiderWeb.spawn = true;
    }

    private void OnDisable()
    {
        m_SpiderWeb.DestroyWeb();
    }

    protected override void ApplyEffect(DamageInfo.HIT_EFFECT effect)
    {
        // TODO:
    }

    public int TileID
    {
        get { return m_TileID; }
        set { m_TileID = value; }
    }

    public SpiderWeb spiderWeb { get { return m_SpiderWeb; } }

    enum SpiderState
    {
        HANGING,    // spider currently handing on web
        FALLING     // Spider falling from rope
    }
    
}
