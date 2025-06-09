using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class PopupPanelController : MonoBehaviour
{
    [SerializeField] private RectTransform panelRectTransform;  // 팝업될 Panel 자신의 RectTransform
    
    private CanvasGroup mBackgroundCanvasGroup;                 // Panel 뒤를 가릴 (검은)배경
    
    private void Awake()
    {
        mBackgroundCanvasGroup = GetComponent<CanvasGroup>();
    }
    
    /// <summary>
    /// Panel을 나타나게하는 메서드
    /// </summary>
    public virtual void Show()
    {
        // 보여지기 전 초기화
        mBackgroundCanvasGroup.alpha = 0;
        panelRectTransform.localScale = Vector3.zero;
        
        GameManager.Instance.PushPopup(this);
        
        // 배경은 등속으로 등장, 패널은 튕기듯이 등장
        mBackgroundCanvasGroup.DOFade(1, 0.3f).SetEase(Ease.Linear);
        panelRectTransform.DOScale(1, 0.3f).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Panel을 사라지게하는 메서드
    /// </summary>
    /// <param name="OnPanelControllerHide"></param>
    public virtual void Hide(Action onPanelControllerHide = null)
    {
        GameManager.Instance.PopPopup(this);
        
        // 사라지기 전 초기화
        mBackgroundCanvasGroup.alpha = 1;
        panelRectTransform.localScale = Vector3.one;
        
        // 배경은 등속으로 퇴장, 패널은 빠르게 작아지다가 끝에서 살짝 당겨지듯 퇴장
        mBackgroundCanvasGroup.DOFade(0, 0.3f).SetEase(Ease.Linear);
        panelRectTransform.DOScale(0, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            // 콜백이 있다면 실행 후 오브젝트 파괴
            onPanelControllerHide?.Invoke();
            Destroy(gameObject);
        });
    }
}
