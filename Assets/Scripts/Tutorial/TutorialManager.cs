using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialInputs
{
    UP,
    DOWN,
    LEFT,
    RIGHT,
    SHOOT,
    DODGE,
    DASH,
    AIM_MODE
}

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private List<Tutorial> AllTutorialPrefabs;
    [Tooltip("Bounding box that follows the player to limit XY movement")]
    [SerializeField] private GameObject m_TutorialBoundingBoxPrefab;

    private List<Tutorial> AllTutorials = new List<Tutorial>();
    private GameObject m_BoundingBox;

    protected bool m_isTutorialsRunning = false;
    protected int m_CurrTutorialIndex = 0;

    public delegate void NewTutorialEntered(Tutorial tutorial);
    public NewTutorialEntered NewTutorialEnteredDelegate;


    public Tutorial CurrTutorial
    {
        get { return m_isTutorialsRunning ? AllTutorials[m_CurrTutorialIndex] : null; }
    }

    private void Awake()
    {
        GameState.PropertyInstance.d_GameTutorialDelegate += StartTutorials;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (!m_isTutorialsRunning) return;
        Vector3 newPos = new Vector3(0, 0, PlayerManager.PropertyInstance.PlayerController.PlayerParent.transform.position.z);
        m_BoundingBox.transform.position = newPos;
    }

    public void StartTutorials()
    {
        InitializeTutorials();
        m_isTutorialsRunning = true;
        if (AllTutorials.Count == 0)
        {
            TutorialPhaseEnded();
            return;
        }
        m_BoundingBox = Instantiate(m_TutorialBoundingBoxPrefab, PlayerManager.PropertyInstance.PlayerController.PlayerMovement.transform.position, Quaternion.identity);
        PlayerManager.PropertyInstance.PlayerController.InitializeTutorialUI();
        StartNextTutorial();
    }

    private void InitializeTutorials()
    {
        // create all tutorials and bind delegates
        foreach (Tutorial tutorialPrefab in AllTutorialPrefabs)
        {
            Tutorial NewTutorial = Instantiate(tutorialPrefab);
            NewTutorial.d_TutorialFinishedDelegate += GotoNextTutorial;
            AllTutorials.Add(NewTutorial);
        }
    }

    public void GotoNextTutorial()
    {
        m_CurrTutorialIndex++;
        // if all tutorials finished
        if (m_CurrTutorialIndex >= AllTutorials.Count)
        {
            TutorialPhaseEnded();
        }
        else
        {
            StartNextTutorial();
        }
    }

    // Called to manually end the tutorial section, such as player skipping the tutorial
    public void SetTutorialsFinished()
    {
        TutorialPhaseEnded();
    }

    private void TutorialPhaseEnded()
    {
        PlayerManager.PropertyInstance.PlayerController.TutorialEnded();
        m_isTutorialsRunning = false;
        foreach (Tutorial tutorial in AllTutorials)
        {
            Destroy(tutorial.gameObject);
        }
        Destroy(m_BoundingBox);
        AllTutorials.Clear();
        GameManager.PropertyInstance.UpdateState(GameStateEnum.TRANSITIONING);
    }

    private void StartNextTutorial()
    {
        AllTutorials[m_CurrTutorialIndex].SetupTutorial();
        NewTutorialEnteredDelegate.Invoke(AllTutorials[m_CurrTutorialIndex]);
    }

    List<TutorialInfo> GetCurrentTutorialInfo()
    {
        return AllTutorials[m_CurrTutorialIndex].TutorialInfoList;
    }

    public void ReceiveTutorialInput(TutorialInputs Input)
    {
        if (AllTutorials[m_CurrTutorialIndex] != null)
        {
            AllTutorials[m_CurrTutorialIndex].ReceiveTutorialInput(Input);
        }
    }
}
