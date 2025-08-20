using System.Collections;
using System.Collections.Generic;
using Best.SocketIO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [Header("Script Refrence")]
    public SocketIOManager socketManager;
    public UIManager uiManager;

    public AudioController audioController;

    [Header("Button Refrence")]
    [SerializeField] internal List<OptionBtn> optionsBtn;



    [Header("Variable")]
    internal bool isAuto = false;
    internal bool isbetInProgress = false;
    internal bool autoLooping = false;
    internal int CurrentBombNo = 1;
    internal List<int> selectedOptions = new List<int>();
    internal double TotalPLInAuto;

    void Start()
    {
        SetOptionintractable(false);
        SetInitData();

    }

    void SetInitData()
    {
        for (int i = 0; i < optionsBtn.Count; i++)
        {
            optionsBtn[i].indexK = i + 1;
        }
    }

    internal void SetOptionintractable(bool isFalse)
    {
        foreach (var btn in optionsBtn)
        {
            if (!btn.isopend) btn.btn.interactable = isFalse;
        }

    }
    internal void Startbet()
    {
        if (socketManager.PlayerData.balance - socketManager.InitialData.bets[uiManager.BetAmountDropDown.value] < 0)
        {
            uiManager.ShowLowbalancePopup();
            return;
        }
        audioController.PlayWLAudio("start");
        if (!isAuto)
        {
            if (!isbetInProgress)
            {
                uiManager.ManualInputBlocker.SetActive(true);
                Debug.Log("DingDing");
                isbetInProgress = true;
                SetOptionintractable(true);
                uiManager.RaycastBlocker.SetActive(false);
                uiManager.SetStartButtonText("CASH OUT  0.00");
                uiManager.StartBet.interactable = false;
            }
            else
            {
                uiManager.ManualInputBlocker.SetActive(false);
                socketManager.AccumulateResult("CASH");
                uiManager.RaycastBlocker.SetActive(true);
                // isbetInProgress = false;
                uiManager.StartBet.interactable = false;
            }
        }
        else
        {
            if (!isbetInProgress)
            {

                selectedOptions.Clear();
                foreach (var obj in optionsBtn)
                {
                    if (obj.isSelectedForAuto)
                    {
                        selectedOptions.Add(obj.indexK);
                    }
                }
                if (selectedOptions.Count != 0)
                {
                    uiManager.AutoInputBlocker.SetActive(true);

                    isbetInProgress = true;
                    uiManager.RaycastBlocker.SetActive(true);

                    autoLooping = true;

                    uiManager.SetStartButtonText("STOP  0.00");

                    StartCoroutine(Autobet());
                }
            }
            else
            {
                uiManager.AutoInputBlocker.SetActive(false);
                uiManager.StartBet.interactable = false;

                autoLooping = false;

                uiManager.RaycastBlocker.SetActive(false);


            }

        }

    }

    IEnumerator Autobet()
    {
        if (socketManager.PlayerData.balance - socketManager.InitialData.bets[uiManager.BetAmountDropDown.value] < 0)
        {
            uiManager.ShowLowbalancePopup();
            yield break;
        }
        socketManager.isResultdone = false;
        socketManager.AccumulateResult("AT");

        yield return new WaitUntil(() => socketManager.isResultdone);



        for (int i = 0; i < optionsBtn.Count; i++)
        {
            int x = i + 1;

            if (socketManager.ResultData.payload.mines.Contains(x))
            {
                optionsBtn[i].SetResult("bomb");
            }
            else
            {
                optionsBtn[i].SetResult("diamond");
            }
        }

        if (socketManager.ResultData.payload.isMine) audioController.PlayWLAudio("bomb");
        else audioController.PlayWLAudio("diamond");


        StartCoroutine(ResetMineField(1f));
        uiManager.RaycastBlocker.SetActive(true);
        uiManager.SetStartButtonText("STOP  " + socketManager.ResultData.payload.currentWinning.ToString());
        checkForStopLoss();
        yield return new WaitForSeconds(3f);
        uiManager.playerBalanceText.text = socketManager.PlayerData.balance.ToString();
        TotalPLInAuto += socketManager.ResultData.payload.currentWinning - socketManager.InitialData.bets[uiManager.BetAmountDropDown.value];


        //if (socketManager.ResultData.payload.isCashOut) yield break;
        if (autoLooping)
        {
            StartCoroutine(Autobet());
        }
        else
        {
            //  socke
            // uiManager.AutoInputBlocker.SetActive(false);tManager.AccumulateResult("CASH");
            isbetInProgress = false;

            uiManager.AutoInputBlocker.SetActive(false);
            TotalPLInAuto = 0;


            uiManager.SetStartButtonText("START AUTOBET");
            uiManager.StartBet.interactable = true;
            foreach (var obj in optionsBtn)
            {

                obj.btn.interactable = true;

            }
        }
    }
    void checkForStopLoss()
    {

        if (double.TryParse(uiManager.StopOnProfitText.text, out double value))
        {
            if (value != 0 && value <= TotalPLInAuto)
            {

                autoLooping = false;
                uiManager.RaycastBlocker.SetActive(false);
            }
        }
        if (double.TryParse(uiManager.StopOnLossText.text, out double val))
        {
            if (val != 0 && val <= TotalPLInAuto * -1)
            {

                autoLooping = false;
                uiManager.RaycastBlocker.SetActive(false);
            }
        }

    }
    private bool isBetvalid(double currentprojectedBet, bool showPopup = true)
    {
        if (socketManager.PlayerData.balance >= currentprojectedBet)
        {
            return true;
        }
        else
        {
            return false;
            if (showPopup) uiManager.ShowLowbalancePopup();
        }
    }

    internal void OnoptionClicked(int index)
    {
        if (!isAuto)
        {

            selectedOptions.Clear();
            selectedOptions.Add(index);
            socketManager.AccumulateResult("MN");
            SetOptionintractable(false);
        }

    }
    internal void PlayMineSelectAudio()
    {
        audioController.PlayWLAudio("b2");
    }
    internal void OnResultSucess()
    {
        if (isAuto)
        {

            return;
        }
        if (!socketManager.ResultData.payload.isCashOut)
        {

            uiManager.SetStartButtonText("CASH OUT  " + socketManager.ResultData.payload.currentWinning.ToString());
            uiManager.StartBet.interactable = true;

            if (!socketManager.ResultData.payload.isMine)
            {
                audioController.PlayWLAudio("diamond");
                SetOptionintractable(true);
                foreach (int c in selectedOptions)
                {
                    optionsBtn[c - 1].SetResult("diamond");
                }
            }
            else
            {
                uiManager.ManualInputBlocker.SetActive(false);
                audioController.PlayWLAudio("bomb");
                for (int i = 0; i < optionsBtn.Count; i++)
                {
                    int x = i + 1;
                    Debug.Log(x + ".    ." + socketManager.ResultData.payload.mines.Contains(x) + ".    .");
                    if (socketManager.ResultData.payload.mines.Contains(x))
                    {
                        optionsBtn[i].SetResult("bomb");
                    }
                    else
                    {
                        optionsBtn[i].SetResult("diamond");
                    }
                }
                StartCoroutine(ResetMineField());
                uiManager.RaycastBlocker.SetActive(true);
                //  isbetInProgress = false;
                uiManager.StartBet.interactable = false;
            }
        }
        else
        {

            for (int i = 0; i < optionsBtn.Count; i++)
            {
                int x = i + 1;
                //                Debug.Log(x + ".    ." + socketManager.ResultData.payload.mines.Contains(x) + ".    .");
                if (socketManager.ResultData.payload.mines.Contains(x))
                {
                    optionsBtn[i].SetResult("bomb");
                }
                else
                {
                    optionsBtn[i].SetResult("diamond");
                }
            }
            StartCoroutine(ResetMineField());
            uiManager.RaycastBlocker.SetActive(true);

        }
        uiManager.playerBalanceText.text = socketManager.PlayerData.balance.ToString();
    }

    IEnumerator ResetMineField(float t = 3f)
    {
        yield return new WaitForSeconds(t);
        SetOptionintractable(false);
        if (!isAuto) uiManager.StartBet.interactable = true;
        for (int i = 0; i < optionsBtn.Count; i++)
        {
            optionsBtn[i].ResetAll();
        }
        if (!isAuto) uiManager.SetStartButtonText("START BET");
        if (!isAuto) isbetInProgress = false;
    }

    internal void HardReset()
    {
        foreach (var obj in optionsBtn)
        {
            obj.ResetAll();
            obj.isSelectedForAuto = false;
            obj.FillBorder.gameObject.SetActive(false);
            obj.border.gameObject.SetActive(false);
            obj.Cover.gameObject.SetActive(true);
        }
    }

}
