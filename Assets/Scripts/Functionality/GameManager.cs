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
        if (!isAuto)
        {
            if (!isbetInProgress)
            {
                Debug.Log("DingDing");
                isbetInProgress = true;
                SetOptionintractable(true);
                uiManager.RaycastBlocker.SetActive(false);
                uiManager.SetStartButtonText("CASH OUT  0.00");
                uiManager.StartBet.interactable = false;
            }
            else
            {
                socketManager.AccumulateResult("CASH");
                uiManager.RaycastBlocker.SetActive(true);
                isbetInProgress = false;
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

                    isbetInProgress = true;
                    uiManager.RaycastBlocker.SetActive(true);

                    autoLooping = true;

                    uiManager.SetStartButtonText("STOP  0.00");

                    StartCoroutine(Autobet());
                }
            }
            else
            {
                uiManager.SetStartButtonText("START AUTOBET");

                isbetInProgress = false;

                autoLooping = false;

                uiManager.RaycastBlocker.SetActive(false);



            }

        }

    }

    IEnumerator Autobet()
    {
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
        yield return new WaitForSeconds(3f);
        uiManager.playerBalanceText.text = socketManager.PlayerData.balance.ToString();

        //if (socketManager.ResultData.payload.isCashOut) yield break;
        if (autoLooping)
        {
            StartCoroutine(Autobet());
        }
        else
        {
            //  socketManager.AccumulateResult("CASH");
            isbetInProgress = false;
            foreach (var obj in optionsBtn)
            {

                obj.btn.interactable = true;

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
                SetOptionintractable(true);
                foreach (int c in selectedOptions)
                {
                    optionsBtn[c - 1].SetResult("diamond");
                }
            }
            else
            {
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
                isbetInProgress = false;
                uiManager.StartBet.interactable = false;
            }
        }
        else
        {
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
            isbetInProgress = false;
        }
        uiManager.playerBalanceText.text = socketManager.PlayerData.balance.ToString();
    }

    IEnumerator ResetMineField(float t = 3f)
    {
        yield return new WaitForSeconds(t);
        SetOptionintractable(false);
        uiManager.StartBet.interactable = true;
        for (int i = 0; i < optionsBtn.Count; i++)
        {
            optionsBtn[i].ResetAll();
        }
        if (!isAuto) uiManager.SetStartButtonText("START BET");
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
