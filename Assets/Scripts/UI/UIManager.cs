//using System.Diagnostics;
//using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UIManager : MonoBehaviour
{
    [Header("Script Refrence")]
    public SocketIOManager socketManager;
    public GameManager gameManager;

    public AudioController audioController;


    [Space]
    [Header("Manual Auto Panel")]
    [SerializeField] private Button ManualBtn;
    [SerializeField] private Button AutoBtn;
    [SerializeField] private GameObject toglePanel;


    [Header("Fields")]
    [SerializeField] private GameObject BetAmount;
    [SerializeField] private GameObject NoOfMines;
    [SerializeField] private GameObject StopOnProfit;
    [SerializeField] private GameObject StopOnLoss;



    [Header("Leftpanel")]
    [SerializeField] internal TMP_Dropdown BetAmountDropDown;


    [Header("DropDown Mines")]
    [SerializeField] private Slider MinesSlider;
    [SerializeField] private TMP_Text DiamondAmount;
    [SerializeField] private TMP_Text BombAmount;


    [Header("Player Balance")]
    [SerializeField] internal TMP_Text playerBalanceText;


    [Header("Stop Loss")]
    [SerializeField] internal TMP_InputField StopOnProfitText;
    [SerializeField] internal TMP_InputField StopOnLossText;

    [Header("Start Button")]
    [SerializeField] internal Button StartBet;
    [SerializeField] private TMP_Text StartBtnText;

    [SerializeField] internal GameObject RaycastBlocker;

    [Header("InputBlocker")]
    [SerializeField] internal GameObject ManualInputBlocker;
    [SerializeField] internal GameObject AutoInputBlocker;

    [Space]
    [Space]
    [Space]
    [Space]
    [Space]
    [Header("popup")]
    [SerializeField]
    private GameObject PopupPanel;
    [SerializeField] private Button InfoBtn;
    [SerializeField] private Button SettingBtn;


    [SerializeField] private GameObject Infopanel;
    [SerializeField] private GameObject ReconectionPopup;
    [SerializeField] private GameObject DisconectionPopup;
    [SerializeField] private GameObject LowBalancePopup;
    [Header("Settings")]
    [SerializeField] private GameObject SetingPopup;
    [SerializeField] private Button MusicON;
    [SerializeField] private Button MusicOF;
    [SerializeField] private Button SoundON;
    [SerializeField] private Button SoundOF;
    [Header("QuitPopup")]
    [SerializeField] private Button ExitButton;
    [SerializeField] private GameObject QuitPopup;
    [SerializeField] private Button Yes;
    [SerializeField] private Button No;



    void Start()
    {
        if (AutoBtn)
        {
            AutoBtn.onClick.RemoveAllListeners();
            AutoBtn.onClick.AddListener(() => ToggleAuto(true));
        }
        if (ManualBtn)
        {
            ManualBtn.onClick.RemoveAllListeners();
            ManualBtn.onClick.AddListener(() => ToggleAuto(false));
        }
        if (StartBet)
        {
            StartBet.onClick.RemoveAllListeners();
            StartBet.onClick.AddListener(() => gameManager.Startbet());
        }

        if (InfoBtn)
        {
            InfoBtn.onClick.RemoveAllListeners();
            InfoBtn.onClick.AddListener(() => { TogglePopup(Infopanel, true); audioController.PlayWLAudio("button"); });
        }
        if (SettingBtn)
        {
            SettingBtn.onClick.RemoveAllListeners();
            SettingBtn.onClick.AddListener(() =>
            {
                TogglePopup(SetingPopup, true); audioController.PlayWLAudio("button");
            });
        }
        if (SoundON)
        {
            SoundON.onClick.AddListener(() => SoundToggle(true));
        }
        if (SoundOF)
        {
            SoundOF.onClick.AddListener(() => SoundToggle(false));
        }
        if (MusicON)
        {
            MusicON.onClick.AddListener(() => MusicToggle(true));
        }
        if (MusicOF)
        {
            MusicOF.onClick.AddListener(() => MusicToggle(false));
        }
        if (ExitButton) ExitButton.onClick.AddListener(() => CallOnExitFunction());

        MinesSlider.onValueChanged.AddListener(OnSliderValueChanged);
        OnSliderValueChanged(MinesSlider.value);


    }

    private void SoundToggle(bool on)
    {
        audioController.PlayWLAudio("button");
        if (on)
        {
            audioController.muteAudio = true;
            SoundON.gameObject.SetActive(false);
            SoundOF.gameObject.SetActive(true);

        }
        else
        {
            audioController.muteAudio = false;
            SoundON.gameObject.SetActive(true);
            SoundOF.gameObject.SetActive(false);
        }
    }
    private void MusicToggle(bool on)
    {
        audioController.PlayWLAudio("button");
        if (on)
        {
            audioController.bg_adudio.Pause();
            MusicON.gameObject.SetActive(false);
            MusicOF.gameObject.SetActive(true);

        }
        else
        {
            audioController.bg_adudio.Play();
            MusicON.gameObject.SetActive(true);
            MusicOF.gameObject.SetActive(false);
        }
    }
    internal void ShowLowbalancePopup()
    {
        OnClickCloseButton();
        TogglePopup(LowBalancePopup, true);
    }
    #region panels
    public void OnClickCloseButton()
    {
        audioController.PlayWLAudio("button");

        if (Infopanel.activeInHierarchy) TogglePopup(Infopanel, false);
        if (ReconectionPopup.activeInHierarchy) TogglePopup(ReconectionPopup, false);
        if (DisconectionPopup.activeInHierarchy) TogglePopup(DisconectionPopup, false);
        if (LowBalancePopup.activeInHierarchy) TogglePopup(LowBalancePopup, false);
        if (SetingPopup.activeInHierarchy) TogglePopup(SetingPopup, false);

    }
    public void TogglePopup(GameObject panel, bool setActive)
    {
        // OnClickCloseButton();
        PopupPanel.SetActive(setActive);
        panel.SetActive(setActive);

    }


    internal void CheckAndClosePopups()
    {
        OnClickCloseButton();
    }

    internal void ReconnectionPopup()
    {
        TogglePopup(ReconectionPopup, true);
    }
    internal void DisconnectionPopup()
    {
        TogglePopup(DisconectionPopup, true);

    }


    #endregion



    public void CallOnExitFunction()
    {

        audioController.PlayButtonAudio();
        StartCoroutine(socketManager.CloseSocket());
        // Application.ExternalCall("window.parent.postMessage", "onExit", "*");
    }

    internal void SetInitData()
    {
        List<string> stringOptions = socketManager.InitialData.bets.Select(n => n.ToString()).ToList();

        // Clear old options
        BetAmountDropDown.ClearOptions();

        // Add to dropdown
        BetAmountDropDown.AddOptions(stringOptions);

        playerBalanceText.text = socketManager.PlayerData.balance.ToString();
    }

    void OnSliderValueChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        DiamondAmount.text = (25 - intValue).ToString();
        BombAmount.text = intValue.ToString();
        gameManager.CurrentBombNo = intValue;
    }


    void ToggleAuto(bool isAuto)
    {
        if (gameManager.isbetInProgress) return;

        gameManager.HardReset();

        audioController.PlayWLAudio("button");
        Debug.Log("isAuto" + isAuto);
        gameManager.isAuto = isAuto;
        if (isAuto)
        {
            toglePanel.transform.position = AutoBtn.transform.position;
            RaycastBlocker.SetActive(false);
            gameManager.SetOptionintractable(true);
            StartBtnText.text = "Start AutoBet";

        }
        else
        {
            toglePanel.transform.position = ManualBtn.transform.position;
            RaycastBlocker.SetActive(true);
            StartBtnText.text = "Start Bet";
        }

        gameManager.isbetInProgress = false;
        // NoOFBets.SetActive(isAuto);
        StopOnProfit.SetActive(isAuto);
        StopOnLoss.SetActive(isAuto);
    }

    internal void SetStartButtonText(string BtnText)
    {
        StartBtnText.text = BtnText;
    }

}

