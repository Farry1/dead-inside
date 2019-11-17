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
    public GameObject projectedUnitUIsContainer;
    public GameObject playerInfoContainer;
    public Text stateIndicator;
    public GameObject WinPanel;
    public GameObject LosePanel;

    public GameObject selectedUnitInformationContainer;
    public Image selectedUnitImage;
    public Text selectedUnitName;
    public Text selectedUnitMoveRange;

    [Header("UI Buttons")]
    public Button nextTurnButton;

    [Header("Ingame Menu")]
    public GameObject InGameMenuContainer;

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

    }

    // Update is called once per frame
    void Update()
    {
        //Todo: Make this better
        EnemyUnitsController.Instance.CheckForWin();

        if (CheckForLose())
        {
            if (StageManager.Instance.stageState != StageManager.StageState.Lost)
            {
                LosePanel.SetActive(true);
                StageManager.Instance.SwitchStageState(StageManager.StageState.Lost);
            }
        }

        InGameMenu();

        if (StageManager.Instance.stageState != StageManager.StageState.Lost ||
            StageManager.Instance.stageState != StageManager.StageState.Won)
        {
            switch (StageManager.Instance.stageState)
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

    }

    bool CheckForLose()
    {

        if (UnitsManager.Instance.playerUnits.Count == 0)
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
        foreach (PlayerUnit unit in UnitsManager.Instance.playerUnits)
        {
            if (unitPanelPrefab != null && playerInfoContainer != null)
            {
                GameObject unitPanel = Instantiate(unitPanelPrefab, playerInfoContainer.transform) as GameObject;
                unitPanel.gameObject.name = "UnitPanel";
                unit.relatedUIPanel = unitPanel;
                unitPanel.transform.Find("ProfileImage").GetComponentInChildren<Image>().sprite = unit.characterPortrait;
                unitPanel.transform.Find("ProfileImage").GetComponentInChildren<Image>().preserveAspect = true;
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
