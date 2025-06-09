using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class ConfirmPanelController : PopupPanelController
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject cancelButton;
    
    public Action OnConfirmButtonClick;
    public Action OnCancelButtonClick;

    /// <summary>
    /// 부모 클래스 Show() 메서드의 애니메이션 효과 + 메시지를 표시하고 콜백을 실행하는 기능의 메서드
    /// CancelButton의 경우 사용하지 않는 경우도 있어서 false로 비활성화 하여 확인 버튼만 보이게 할 수 있음
    /// </summary>
    /// <param name="message">표시해야 될 메시지</param>
    /// <param name="OnConfirmButtonClick">ConfirmPanel이 닫히고 나면 실행되어야 할 콜백</param>
    public void Show(string message, Action onConfirmButtonClick, bool activeCancelButton, Action onCancelButtonClick)
    {
        base.Show();
        
        this.messageText.text = message;
        this.OnConfirmButtonClick = onConfirmButtonClick;
        this.OnCancelButtonClick = onCancelButtonClick;
        this.cancelButton.SetActive(activeCancelButton);
    }
    
    /// <summary>
    /// Confirm Button 클릭시 호출되는 메서드
    /// OnConfirmButtonClick에 구독된 콜백이 실행됨
    /// </summary>
    public void OnClickConfirmButton()
    {
        Hide(() => OnConfirmButtonClick?.Invoke());
    }

    /// <summary>
    /// Cancel Button 클릭시 호출되는 메서드
    /// 콜백이 실행되지 않음, Popup Panel만 닫음
    /// </summary>
    public void OnClickCancelButton()
    {
        Hide(() => OnCancelButtonClick?.Invoke());
    }
}