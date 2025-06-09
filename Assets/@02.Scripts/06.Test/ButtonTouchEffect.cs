using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTouchEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform mRectTransform;
    private Vector3 mOriginalScale;
    [SerializeField] private float mPressedScale = 0.9f;
    [SerializeField] private float mDuration = 0.1f;

    private void Awake()
    {
        mRectTransform = GetComponent<RectTransform>();
        mOriginalScale = mRectTransform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        mRectTransform.DOScale(mOriginalScale * mPressedScale, mDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        mRectTransform.DOScale(mOriginalScale, mDuration).SetEase(Ease.OutBack);
    }
}
