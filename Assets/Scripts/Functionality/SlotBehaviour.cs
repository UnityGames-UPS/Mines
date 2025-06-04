using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class SlotBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField]
    private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField]
    private List<SlotImage> images;     //class to store total images
    [SerializeField]
    private List<SlotImage> Tempimages;     //class to store the result matrix

    [Header("Slots Elements")]
    [SerializeField]
    private LayoutElement[] Slot_Elements;

    [Header("Slots Transforms")]
    [SerializeField]
    private Transform[] Slot_Transform;

    [Header("Line Button Objects")]
    [SerializeField]
    private List<GameObject> StaticLine_Objects;

    [Header("Line Button Texts")]
    [SerializeField]
    private List<TMP_Text> StaticLine_Texts;


    [Header("Buttons")]
    [SerializeField]
    private Button SlotStart_Button;
    [SerializeField]
    private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;

    [SerializeField]
    private Button TBetPlus_Button;
    [SerializeField]
    private Button TBetMinus_Button;
    [SerializeField] private Button Turbo_Button;
    [SerializeField] private Button StopSpin_Button;

    [Header("Animated Sprites")]
    [SerializeField]
    private Sprite[] BlueKing_Sprite;

    [SerializeField]
    private Sprite[] ChineseMan_Sprite;
    [SerializeField]
    private Sprite[] Coin_Sprite;
    [SerializeField]
    private Sprite[] Drum_Sprite;
    [SerializeField]
    private Sprite[] FirecCrackeres_Sprite;
    [SerializeField]
    private Sprite[] BoyAsset_Sprite;
    [SerializeField]
    private Sprite[] GirlAsset_Sprite;
    [SerializeField]
    private Sprite[] Looket_Sprite;
    [SerializeField]
    private Sprite[] Scatter_Sprite;
    [SerializeField]
    private Sprite[] Wild_Sprite;
    [SerializeField]
    private Sprite[] SuperWild_Sprite;


    [Header("Miscellaneous UI")]
    [SerializeField]
    private TMP_Text Balance_text;
    [SerializeField]
    private TMP_Text TotalBet_text;
    [SerializeField]
    private TMP_Text LineBet_text;
    [SerializeField]
    private TMP_Text TotalWin_text;
    [SerializeField]
    private TMP_Text TotalLines_text;

    [SerializeField]
    private GameObject TotalWinGameObject;

    [Header("Audio Management")]
    [SerializeField]
    private AudioController audioController;

    [SerializeField]
    private UIManager uiManager;

    [Header("Free Spins Board")]
    [SerializeField]
    private GameObject FSBoard_Object;
    [SerializeField]
    private TMP_Text FSnum_text;

    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField]
    private GameObject Image_Prefab;    //icons prefab
    [SerializeField] Sprite[] TurboToggleSprites;

    private List<Tweener> alltweens = new List<Tweener>();

    private Tweener WinTween = null;

    [SerializeField]
    private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField]
    private SocketIOManager SocketManager;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine FreeSpinRoutine = null;
    private Coroutine tweenroutine;
    private Tween BalanceTween;
    internal bool IsAutoSpin = false;
    internal bool IsFreeSpin = false;
    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    internal bool CheckPopups = false;
    internal int BetCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    protected int Lines = 9;
    [SerializeField]
    private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing
    private int numberOfSlots = 15;          //number of columns
    private bool StopSpinToggle;
    private float SpinDelay = 0.2f;
    private bool IsTurboOn = false;
    internal bool WasAutoSpinOn;
    public List<SlotImage> ReelsFrameGameObject1;
    public List<SlotImage> ReelsHideGameObject1;

    public List<GameObject> PayoutLines;

    [SerializeField] private List<GameObject> GoldCoinSpawningParticals;
    [SerializeField] private GameObject AllOfKindAnimParent, AllOfKIndAnim;

    private bool GoldWildCompleted;
    private bool IsAllofKindAnimCompleted;
    [SerializeField] private List<GameObject> GoldWildEffect;
    [SerializeField] private GameObject FreeGameBottomPanel;



    private void Start()
    {
        IsAutoSpin = false;

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate { StartSlots(); });

        if (TBetPlus_Button) TBetPlus_Button.onClick.RemoveAllListeners();
        if (TBetPlus_Button) TBetPlus_Button.onClick.AddListener(delegate { ChangeBet(true); });

        if (TBetMinus_Button) TBetMinus_Button.onClick.RemoveAllListeners();
        if (TBetMinus_Button) TBetMinus_Button.onClick.AddListener(delegate { ChangeBet(false); });

        if (StopSpin_Button) StopSpin_Button.onClick.RemoveAllListeners();
        if (StopSpin_Button) StopSpin_Button.onClick.AddListener(() => { audioController.PlayButtonAudio(); StopSpinToggle = true; StopSpin_Button.gameObject.SetActive(false); });

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(AutoSpin);

        if (Turbo_Button) Turbo_Button.onClick.RemoveAllListeners();
        if (Turbo_Button) Turbo_Button.onClick.AddListener(TurboToggle);

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(StopAutoSpin);

        if (FSBoard_Object) FSBoard_Object.SetActive(false);
        if(FreeGameBottomPanel) FreeGameBottomPanel.SetActive(false);

        tweenHeight = (15 * IconSizeFactor) - 280;
    }

    void TurboToggle()
    {
        Debug.Log("IS TOGGLE 1");
        audioController.PlayButtonAudio();
        if (IsTurboOn)
        {
            Debug.Log("IS TOGGLE 2");

            IsTurboOn = false;
            Turbo_Button.GetComponent<ImageAnimation>().StopAnimation();
            Turbo_Button.image.sprite = TurboToggleSprites[0];
            // Turbo_Button.image.color = new UnityEngine.Color(0.86f, 0.86f, 0.86f, 1);
        }
        else
        {
            Debug.Log("IS TOGGLE 3");

            IsTurboOn = true;
            Turbo_Button.GetComponent<ImageAnimation>().StartAnimation();
            // Turbo_Button.image.color = new UnityEngine.Color(1, 1, 1, 1);
        }
    }

    #region Autospin
    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {

            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());

        }
    }

    private void StopAutoSpin()
    {
        audioController.PlayButtonAudio();
        if (IsAutoSpin)
        {
            IsAutoSpin = false;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(SpinDelay);
        }
        WasAutoSpinOn = false;
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }
    #endregion

    #region FreeSpin
    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {
            if (FSBoard_Object) FSBoard_Object.SetActive(true);
            if(FreeGameBottomPanel) FreeGameBottomPanel.SetActive(true);

            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));
        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        while (i < spinchances)
        {
            if (FSnum_text) FSnum_text.text = uiManager.FreeSpins.ToString();
            uiManager.FreeSpins--;
            StartSlots();
            yield return tweenroutine;
            yield return new WaitForSeconds(SpinDelay);
            i++;
        }
        if (FSBoard_Object) FSBoard_Object.SetActive(false);
        if(FreeGameBottomPanel) FreeGameBottomPanel.SetActive(false);
        if (WasAutoSpinOn)
        {
            AutoSpin();
        }
        else
        {
            ToggleButtonGrp(true);
        }
        IsFreeSpin = false;
    }
    #endregion

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
        }
    }


    private void MaxBet()
    {
        if (audioController) audioController.PlayButtonAudio();
        BetCounter = SocketManager.initialData.Bets.Count - 1;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;

    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            BetCounter++;
            if (BetCounter >= SocketManager.initialData.Bets.Count)
            {
                BetCounter = 0; // Loop back to the first bet
            }
        }
        else
        {
            BetCounter--;
            if (BetCounter < 0)
            {
                BetCounter = SocketManager.initialData.Bets.Count - 1; // Loop to the last bet
            }
        }
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;

    }

    #region InitialFunctions
    internal void shuffleInitialMatrix()
    {
        for (int i = 0; i < Tempimages.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, 11);
                Tempimages[i].slotImages[j].sprite = myImages[randomIndex];
            }
        }
    }

    internal void SetInitialUI()
    {
        BetCounter = 0;
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        if (TotalWin_text) TotalWin_text.text = "0.000";
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("F3");
        if (TotalLines_text) TotalLines_text.text = "9";
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        //_bonusManager.PopulateWheel(SocketManager.bonusdata);
        CompareBalance();

        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
    }
    #endregion

    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {
            case 0:
                for (int i = 0; i < Coin_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Coin_Sprite[i]);
                }
                animScript.AnimationSpeed = 11f;
                break;

            case 1:
                for (int i = 0; i < FirecCrackeres_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(FirecCrackeres_Sprite[i]);
                }
                animScript.AnimationSpeed = 15f;
                break;

            case 2:
                for (int i = 0; i < Looket_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Looket_Sprite[i]);
                }
                animScript.AnimationSpeed = 11f;
                break;

            case 3:
                for (int i = 0; i < Drum_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Drum_Sprite[i]);
                }
                animScript.AnimationSpeed = 15f;
                break;

            case 4:
                for (int i = 0; i < GirlAsset_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(GirlAsset_Sprite[i]);
                }
                animScript.AnimationSpeed = 9f;
                break;

            case 5:
                for (int i = 0; i < BoyAsset_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(BoyAsset_Sprite[i]);
                }
                animScript.AnimationSpeed = 15f;
                break;

            case 6:
                for (int i = 0; i < BlueKing_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(BlueKing_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;

            case 7:
                for (int i = 0; i < ChineseMan_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(ChineseMan_Sprite[i]);
                }
                animScript.AnimationSpeed = 11f;
                break;

            case 8:
                for (int i = 0; i < SuperWild_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(SuperWild_Sprite[i]);
                }
                animScript.AnimationSpeed = 15f;
                break;

            case 9:
                for (int i = 0; i < Wild_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Wild_Sprite[i]);
                }
                animScript.AnimationSpeed = 7f;
                break;

            case 10:
                for (int i = 0; i < Scatter_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Scatter_Sprite[i]);
                }
                animScript.AnimationSpeed = 15f;
                break;
        }
    }

    #region SlotSpin
    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (audioController) audioController.PlaySpinButtonAudio();

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }
        WinningsAnim(false);
        if (SlotStart_Button) SlotStart_Button.interactable = false;
        if (TempList.Count > 0)
        {
            StopGameAnimation();
        }
        DisableFrameHideLayout();
        ResetPayoutLines();
        if(TotalWinGameObject) TotalWinGameObject.GetComponent<ImageAnimation>().StopAnimation();

        tweenroutine = StartCoroutine(TweenRoutine());
    }
    private void ResetPayoutLines()
    {
        foreach (GameObject line in PayoutLines)
        {
            line.SetActive(false);
        }
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        if (currentBalance < currentTotalBet && !IsFreeSpin)
        {
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            ToggleButtonGrp(true);
            yield break;
        }
        if (audioController) audioController.PlayWLAudio("spin");
        CheckSpinAudio = true;

        IsSpinning = true;

        ToggleButtonGrp(false);
        if (!IsTurboOn && !IsFreeSpin && !IsAutoSpin)
        {
            StopSpin_Button.gameObject.SetActive(true);
        }
        if (IsTurboOn)
        {
            for (int i = 0; i < numberOfSlots; i++)
            {
                InitializeTweening(Slot_Transform[i]);
            }
        }
        else
        {
            for (int i = 0; i < numberOfSlots; i++)
            {
                InitializeTweening(Slot_Transform[i]);
               // yield return new WaitForSeconds(0.1f);
            }
        }



        if (!IsFreeSpin)
        {
            BalanceDeduction();
        }

        SocketManager.AccumulateResult(BetCounter);
        yield return new WaitUntil(() => SocketManager.isResultdone);

        for (int j = 0; j < SocketManager.resultData.ResultReel.Count; j++)
        {
            List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
            for (int i = 0; i < 5; i++)
            {
                if (images[i].slotImages[j]) images[i].slotImages[j].sprite = myImages[resultnum[i]];
                if (Tempimages[i].slotImages[j]) Tempimages[i].slotImages[j].sprite = myImages[resultnum[i]];
                PopulateAnimationSprites(Tempimages[i].slotImages[j].gameObject.GetComponent<ImageAnimation>(), resultnum[i]);
            }
        }

        if (IsTurboOn || IsFreeSpin)
        {
            Debug.Log("IS TURBO ON " + IsTurboOn);
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            for (int i = 0; i < 15; i++)
            {
                yield return new WaitForSeconds(0.1f);
                if (StopSpinToggle)
                {
                    break;
                }
            }
            StopSpin_Button.gameObject.SetActive(false);
        }

        for (int i = 0; i < numberOfSlots; i++)
        {
            yield return StopTweening(5, Slot_Transform[i], i, StopSpinToggle);
        }
        StopSpinToggle = false;


        yield return alltweens[^1].WaitForCompletion();
        KillAllTweens();

        if (SocketManager.playerdata.currentWining > 0)
        {
            SpinDelay = 1.2f;
        }
        else
        {
            SpinDelay = 0.2f;
        }
        if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("F3");
        if(SocketManager.playerdata.currentWining>0) 
        {
           if(TotalWinGameObject) TotalWinGameObject.GetComponent<ImageAnimation>().StartAnimation();
        }
        BalanceTween?.Kill();
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("F3");
        if (SocketManager.resultData.goldWildCol.Count > 0)
        {
            GoldWildCompleted = false;
            StartCoroutine(CheckForGoldWildColumn());
            yield return new WaitUntil(() => GoldWildCompleted);
        }

        if (SocketManager.resultData.featureAll)
        {
            IsAllofKindAnimCompleted = false;
            StartCoroutine(CheckForAllOfKind());
            yield return new WaitUntil(() => IsAllofKindAnimCompleted);
        }
        else
        {
            CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.resultData.jackpot);
        }
        CheckPopups = true;
        // if (TotalWin_text) TotalWin_text.text = SocketManager.playerdata.currentWining.ToString("F3");
        // BalanceTween?.Kill();
        // if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("F3");

        currentBalance = SocketManager.playerdata.Balance;
        CheckWinPopups();

        yield return new WaitUntil(() => !CheckPopups);
        if (!IsAutoSpin && !IsFreeSpin)
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            // yield return new WaitForSeconds(2f);
            IsSpinning = false;
        }
        if (SocketManager.resultData.isFreeSpin)
        {
            if (IsFreeSpin)
            {
                IsFreeSpin = false;
                if (FreeSpinRoutine != null)
                {
                    StopCoroutine(FreeSpinRoutine);
                    FreeSpinRoutine = null;
                }
            }
            StartCoroutine(uiManager.FreeSpinProcess((int)SocketManager.resultData.count));
            if (IsAutoSpin)
            {
                WasAutoSpinOn = true;
                StopAutoSpin();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private void BalanceDeduction()
    {
        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;

        balance = balance - bet;

        BalanceTween = DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("F3");
        });
    }

    internal void CheckWinPopups()
    {
        if (SocketManager.playerdata.currentWining >= currentTotalBet * 10 && SocketManager.playerdata.currentWining < currentTotalBet * 15)
        {
            uiManager.PopulateWin(1, SocketManager.playerdata.currentWining);
        }
        else if (SocketManager.playerdata.currentWining >= currentTotalBet * 15 && SocketManager.playerdata.currentWining < currentTotalBet * 20)
        {
            uiManager.PopulateWin(2, SocketManager.playerdata.currentWining);
        }
        else if (SocketManager.playerdata.currentWining >= currentTotalBet * 20)
        {
            uiManager.PopulateWin(3, SocketManager.playerdata.currentWining);
        }
        else
        {
            CheckPopups = false;
        }
    }

    public IEnumerator CheckForGoldWildColumn()
    {
        int GoldColumnCount = SocketManager.resultData.goldWildCol.Count;
        if (GoldColumnCount > 0)
        {
            CheckPopups = true;
            for (int i = 0; i < GoldColumnCount; i++)
            {
                int index = Convert.ToInt32(SocketManager.resultData.goldWildCol[i]); // if goldWildCol is List<object>
                GoldWildEffect[index].SetActive(true);
            }
            yield return new WaitForSeconds(2.5f);
            for (int i = 0; i < GoldColumnCount; i++)
            {
                int index = Convert.ToInt32(SocketManager.resultData.goldWildCol[i]); // if goldWildCol is List<object>
                GoldCoinSpawningParticals[index].SetActive(true);
            }

            // Wait for 3 seconds after the loop
            yield return new WaitForSeconds(4f);
            foreach (GameObject go in GoldCoinSpawningParticals)
            {
                go.SetActive(false);
            }
            foreach (GameObject go in GoldWildEffect)
            {
                go.SetActive(false);
            }
            GoldWildCompleted = true;

        }
    }
    private IEnumerator CheckForAllOfKind()
    {
        AllOfKindAnimParent.SetActive(true);
        AllOfKIndAnim.SetActive(true);

        yield return new WaitForSeconds(4.5f);
        AllOfKindAnimParent.SetActive(false);
        AllOfKIndAnim.SetActive(false);
        IsAllofKindAnimCompleted = true;
    }

    //generate the payout lines generated 
    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, double jackpot = 0)
    {
        List<int> y_points = null;
        List<int> points_anim = null;
        if (LineId.Count > 0 || points_AnimString.Count > 0)
        {
            for (int i = 0; i < LineId.Count; i++)
            {
                Debug.Log("line come " + LineId[i]);
                PayoutLines[LineId[i]].SetActive(true);
            }

            for (int i = 0; i < points_AnimString.Count; i++)
            {
                points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                for (int k = 0; k < points_anim.Count; k++)
                {
                    if (points_anim[k] >= 10)
                    {
                        StartGameAnimation(Tempimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject);
                    }
                    else
                    {
                        StartGameAnimation(Tempimages[0].slotImages[points_anim[k]].gameObject);
                    }
                }
            }
            foreach (SlotImage slotImage in ReelsHideGameObject1)
            {
                foreach (Image img in slotImage.slotImages)
                {
                    img.gameObject.SetActive(true);
                }
            }

            for (int i = 0; i < points_AnimString.Count; i++)
            {
                points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                for (int k = 0; k < points_anim.Count; k++)
                {
                    if (points_anim[k] >= 10)
                    {
                        ReelsHideGameObject1[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject.SetActive(false);
                        ReelsFrameGameObject1[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject.SetActive(true);
                    }
                    else
                    {
                        ReelsHideGameObject1[0].slotImages[points_anim[k]].gameObject.SetActive(false);
                        ReelsFrameGameObject1[0].slotImages[points_anim[k]].gameObject.SetActive(true);
                    }
                }
            }
            WinningsAnim(true);
        }
        else
        {

            //if (audioController) audioController.PlayWLAudio("lose");
            if (audioController) audioController.StopWLAaudio();
        }
        CheckSpinAudio = false;
    }

    private void DisableFrameHideLayout()
    {
        foreach (SlotImage slotImage in ReelsHideGameObject1)
        {
            foreach (Image img in slotImage.slotImages)
            {
                img.gameObject.SetActive(false);
            }
        }
        foreach (SlotImage slotImage in ReelsFrameGameObject1)
        {
            foreach (Image img in slotImage.slotImages)
            {
                img.gameObject.SetActive(false);
            }
        }
    }
    private void WinningsAnim(bool IsStart)
    {
        if (IsStart)
        {
            WinTween = TotalWin_text.gameObject.GetComponent<RectTransform>().DOScale(new Vector2(1.5f, 1.5f), 1f).SetLoops(-1, LoopType.Yoyo).SetDelay(0);
        }
        else
        {
            WinTween.Kill();
            TotalWin_text.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }

    #endregion

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }


    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button) AutoSpin_Button.interactable = toggle;
        if (TBetMinus_Button) TBetMinus_Button.interactable = toggle;
        if (TBetPlus_Button) TBetPlus_Button.interactable = toggle;

        //  if (Turbo_Button) Turbo_Button.interactable = toggle;
    }

    //start the icons animation
    private void StartGameAnimation(GameObject animObjects)
    {
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        temp.StartAnimation();
        TempList.Add(temp);
    }

    //stop the icons animation
    private void StopGameAnimation()
    {
        for (int i = 0; i < TempList.Count; i++)
        {
            TempList[i].StopAnimation();
        }
        TempList.Clear();
        TempList.TrimExcess();
    }


    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        Tweener tweener = slotTransform.DOLocalMoveY(-tweenHeight, 0.07f).SetLoops(-1, LoopType.Restart).SetDelay(0);
        tweener.Play();
        alltweens.Add(tweener);
    }



    private IEnumerator StopTweening(int reqpos, Transform slotTransform, int index, bool isStop)
    {
        alltweens[index].Kill();
        // int tweenpos = (reqpos * IconSizeFactor) - IconSizeFactor;
        slotTransform.localPosition = new Vector2(slotTransform.localPosition.x, 0);
        alltweens[index] = slotTransform.DOLocalMoveY(-855 + 100, 0.5f).SetEase(Ease.OutElastic);
        if (!isStop)
        {
            Debug.Log("playing stop sound");
            audioController.PlayWLAudio("spinStop");
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            if (index == alltweens.Count - 1)
            {
                audioController.PlayWLAudio("spinStop");
            }
            yield return null;
        }
    }


    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}

