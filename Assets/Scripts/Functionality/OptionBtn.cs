using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems; // For IPointerEnterHandler & IPointerExitHandler

public class OptionBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] internal Button btn;
    [SerializeField] internal Image border;
    [SerializeField] internal Image FillBorder;
    [SerializeField] internal Image bomb;
    [SerializeField] internal Image diamond;
    [SerializeField] internal Image Cover;

    internal int indexK;

    internal bool isopend;

    internal bool isSelectedForAuto = false;

    private Tween coverPulseTween;
    private Tween hoverTween;
    GameObject objAnimate;
    void Start()
    {
        if (btn)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClick);
        }
        ResetAll();
    }

    internal void SetIndex(int index)
    {
        indexK = index;
    }

    public void OnClick()
    {

        if (gameManager.isAuto)
        {
            gameManager.PlayMineSelectAudio();
            if (!isSelectedForAuto)
            {
                isSelectedForAuto = true;
                isopend = true;
                // btn.interactable = false;
                FillBorder.gameObject.SetActive(true);
                border.gameObject.SetActive(true);
                Cover.gameObject.SetActive(false);
            }
            else
            {
                isSelectedForAuto = false;
                isopend = true;
                // btn.interactable = false;
                Cover.gameObject.SetActive(true);
                FillBorder.gameObject.SetActive(false);
                border.gameObject.SetActive(false);
            }

        }
        else
        {
            Debug.Log("option Clicked");
            WaitTillResult();
            isopend = true;
            btn.interactable = false;
            gameManager.OnoptionClicked(indexK);
        }
    }


    internal void SetResult(string val)
    {
        coverPulseTween?.Kill();




        if (gameManager.isAuto)
        {
            if (isSelectedForAuto) objAnimate = FillBorder.gameObject;
            else objAnimate = Cover.gameObject;
        }
        else
        {
            objAnimate = Cover.gameObject;
        }



        objAnimate.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            bomb.gameObject.SetActive(false);
            diamond.gameObject.SetActive(false);
            btn.interactable = false;

            if (val == "bomb")
            {
                bomb.gameObject.SetActive(true);
                bomb.transform.localScale = Vector3.zero;
                bomb.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }
            else if (val == "diamond")
            {
                diamond.gameObject.SetActive(true);
                diamond.transform.localScale = Vector3.zero;
                diamond.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }
        });
    }

    internal void WaitTillResult()
    {
        objAnimate.transform.localScale = Vector3.one;

        coverPulseTween?.Kill();

        if (gameManager.isAuto)
        {
            objAnimate = FillBorder.gameObject;
        }
        else
        {
            objAnimate = Cover.gameObject;
        }

        coverPulseTween = objAnimate.transform
            .DOScale(1.1f, 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    internal void ResetAll()
    {
        coverPulseTween?.Kill();
        hoverTween?.Kill();


        if (gameManager.isAuto)
        {
            if (isSelectedForAuto) objAnimate = FillBorder.gameObject;
            else objAnimate = Cover.gameObject;
        }
        else
        {
            objAnimate = Cover.gameObject;
        }


        isopend = false;
        bomb.gameObject.SetActive(false);
        diamond.gameObject.SetActive(false);
        objAnimate.gameObject.SetActive(true);
        objAnimate.transform.localScale = Vector3.one;
        transform.localScale = Vector3.one; // Reset hover scale

    }

    internal void ResetParticular()
    {
        ResetAll();
    }

    // Hover effects
    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverTween?.Kill();
        hoverTween = transform.DOScale(1.07f, 0.2f).SetEase(Ease.OutQuad); // Slight zoom-in
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverTween?.Kill();
        hoverTween = transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad); // Back to normal
    }
}
