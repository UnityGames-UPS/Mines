using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class ManageLineButtons : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler, IPointerUpHandler,IPointerDownHandler
{

	[SerializeField]
	private SlotBehaviour slotManager;
	[SerializeField]
	private TMP_Text num_text;
	private int buttonIndex;

    void Awake()
    {
        buttonIndex=GetButtonIndexByName(this.gameObject.name);
    }
    public void OnPointerEnter(PointerEventData eventData)
	{
		//if (Application.platform == RuntimePlatform.WebGLPlayer && !Application.isMobilePlatform)
		//{
			//Debug.Log("run on pointer enter");
			//slotManager.GenerateStaticLine(num_text);
			Debug.Log("@@ lines index 1  "+ buttonIndex);
			slotManager.PayoutLines[buttonIndex].SetActive(true);
		//}
	}
	public void OnPointerExit(PointerEventData eventData)
	{
		//if (Application.platform == RuntimePlatform.WebGLPlayer && !Application.isMobilePlatform)
		//{
			//Debug.Log("run on pointer exit");
			//slotManager.DestroyStaticLine();
			Debug.Log("@@ lines index 2  "+ buttonIndex);

			slotManager.PayoutLines[buttonIndex].SetActive(false);

		//}
	}
	public void OnPointerDown(PointerEventData eventData)
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform)
		{
			this.gameObject.GetComponent<Button>().Select();
			//Debug.Log("run on pointer down");
			//slotManager.GenerateStaticLine(num_text);
			Debug.Log("@@ lines index 3  "+ buttonIndex);

			slotManager.PayoutLines[buttonIndex].SetActive(true);

		}
	}
	public void OnPointerUp(PointerEventData eventData)
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform)
		{
			//Debug.Log("run on pointer up");
			//slotManager.DestroyStaticLine();
			slotManager.PayoutLines[buttonIndex].SetActive(false);
			DOVirtual.DelayedCall(0.1f, () =>
			{
				this.gameObject.GetComponent<Button>().spriteState = default;
				EventSystem.current.SetSelectedGameObject(null);
			 });
		}
	}

	int GetButtonIndexByName(string buttonName)
{
    switch (buttonName)
    {
        case "Button1": return 0;
        case "Button2": return 1;
        case "Button3": return 2;
        case "Button4": return 3;
        case "Button5": return 4;
        case "Button6": return 5;
        case "Button7": return 6;
        case "Button8": return 7;
        case "Button9": return 8;
        default:
            Debug.LogWarning("Unrecognized button name: " + buttonName);
            return -1;
    }
}
}
