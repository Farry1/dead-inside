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
    public GameObject WinPanel;
    public GameObject LosePanel;

    [Header("UI Buttons")]
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
        stageManager = StageManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        //Todo: Make this better
        EnemyUnitsController.Instance.CheckForWin();

        if (CheckForLose())
        {
            LosePanel.SetActive(true);
            StageManager.Instance.stageState = StageManager.StageState.Lost;
        }

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

    bool CheckForLose()
    {

        if (PlayerUnitsController.Instance.units.Count == 0)
        {
            return true;
        }
        else
            return false;

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
            if (unitPanelPrefab != null && playerInfoContainer != null)
            {
                GameObject unitPanel = Instantiate(unitPanelPrefab, playerInfoContainer.transform) as GameObject;
                unitPanel.gameObject.name = "UnitPanel";
                unit.relatedUIPanel = unitPanel;
                unitPanel.transform.Find("ProfileImage").GetComponentInChildren<Image>().sprite = unit.characterPortrait;
            }
        }
    }

    public void UpdateUnitPanel()
    {
        GameObject[] unitPanels = GameObject.FindGameObjectsWithTag("UnitPanel");
        foreach (GameObject unitPanel in unitPanels)
        {
            Destroy(unitPanel);
        }

        CreateUnitPanel();
    }

    public void ClearPlayerActionsPanel()
    {
        if (playerActionsContainer != null)
        {
            Button[] buttons = playerActionsContainer.GetComponentsInChildren<Button>();
            foreach (Button b in buttons)
            {
                Destroy(b.gameObject);
            }

            playerActionsContainer.SetActive(false);
        }
    }

    public void CreatePlayerActionMenu(List<Action> availableActions)
    {
        Debug.Log("Creating Actions Menu");
        foreach (Action action in availableActions)
        {
            GameObject actionButton = GameObject.Instantiate(actionButtonPrefab, playerActionsContainer.transform);
            actionButton.GetComponentInChildren<Text>().text = action.actionName;
            actionButton.GetComponentInChildren<Text>().gameObject.SetActive(false);

            actionButton.transform.Find("Image").GetComponentInChildren<Image>().sprite = action.actionUISprite;
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
