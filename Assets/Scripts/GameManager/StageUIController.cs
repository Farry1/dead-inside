using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StageUIController : MonoBehaviour
{

    [Header("UI Prefabs")]
    public GameObject actionButtonPrefab;
    public GameObject unitPanelPrefab;


    [Header("UI Element")]
    public GameObject playerActionsContainer;
    public GameObject playerInfoContainer;
    public Text stateIndicator;
    public GameObject GameOverScreen;

    [Header("UI Buttons")]
    public Button playerMoveButton;
    public Button nextTurnButton;

    [Header("Ingame Menu")]
    public GameObject InGameMenuContainer;


    private StageManager stageManager;

    private static StageUIController _instance;
    public static StageUIController Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        playerMoveButton.interactable = true;
        stageManager = StageManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        EnemyUnitsController.Instance.CheckForWin();
        InGameMenu();

        switch (stageManager.stageState)
        {
            case StageManager.StageState.PlayerTurn:
                stateIndicator.gameObject.SetActive(true);
                stateIndicator.text = "Player Turn";

                nextTurnButton.gameObject.SetActive(true);

                break;
            case StageManager.StageState.EnemyTurn:
                stateIndicator.gameObject.SetActive(true);
                stateIndicator.text = "Enemy Turn";
                break;
            default:
                stateIndicator.gameObject.SetActive(false);
                nextTurnButton.gameObject.SetActive(false);
                break;
        }
    }

    public void SetPlayerActionContainer(bool state)
    {
        playerActionsContainer.SetActive(state);
    }

    public void CreateUnitPanel()
    {
        Debug.Log("Creating Player Info Panel");
        foreach (PlayerUnit unit in PlayerUnitsController.Instance.units)
        {
            GameObject unitPanel = GameObject.Instantiate(unitPanelPrefab, playerInfoContainer.transform);
            unit.relatedUIPanel = unitPanel;

            //actionButton.GetComponentInChildren<Text>().text = action.actionName;
            //actionButton.GetComponentInChildren<Button>().onClick.AddListener(action.Execute);
        }
    }

    public void ClearPlayerActions()
    {
        Button[] buttons = playerActionsContainer.GetComponentsInChildren<Button>();
        foreach (Button b in buttons)
        {
            Destroy(b.gameObject);
        }
    }

    public void CreateActionMenu(List<Action> availableActions)
    {
        Debug.Log("Creating Actions Menu");
        foreach (Action action in availableActions)
        {
            GameObject actionButton = GameObject.Instantiate(actionButtonPrefab, playerActionsContainer.transform);
            actionButton.GetComponentInChildren<Text>().text = action.actionName;
            actionButton.GetComponentInChildren<Button>().onClick.AddListener(action.Execute);
        }
    }

    bool toggleIngameMenu = false;
    void InGameMenu()
    {
        InGameMenuContainer.SetActive(toggleIngameMenu);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            toggleIngameMenu = !toggleIngameMenu;
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

}
