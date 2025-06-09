using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;

public class BoardCell : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]private Image mStoneImage;
    [SerializeField]private Image mUtilImage;
    [SerializeField]private Image mUtilLastImage;
    [SerializeField]private List<Sprite> mImages;
    [SerializeField]private int mFadeCount = 5;
    private int fading; 
    
    public Enums.EPlayerType playerType = Enums.EPlayerType.None;
    public delegate void OnCellClicked(int index);
    public OnCellClicked onCellClicked;
    public int cellIndex;
    public bool IsForbidden = false;

    private void OnEnable()
    {
        fading = mFadeCount;
    }
    
    public void InitBlockCell(int blockindex, OnCellClicked onCellClicked)
    {
        cellIndex = blockindex;
        this.onCellClicked = onCellClicked;
    }

    public void ResetCell()
    {
        mStoneImage.DOFade(0,0);
        mStoneImage.sprite = GetImage(Enums.EGameImage.None);
        mUtilImage.DOFade(0,0);
        mUtilImage.sprite = GetImage(Enums.EGameImage.None);
        mUtilLastImage.DOFade(0,0);
        mUtilLastImage.sprite = GetImage(Enums.EGameImage.None);
        
        IsForbidden = false;
        playerType = Enums.EPlayerType.None;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        onCellClicked?.Invoke(cellIndex);
    }

    public void SetMark(Enums.EPlayerType playerType)
    {
        switch (playerType)
        {
            case Enums.EPlayerType.Player_Black:
                mStoneImage.DOFade(1,0);
                mStoneImage.sprite = GetImage(Enums.EGameImage.BlackStone);
                break;
            case Enums.EPlayerType.Player_White:
                mStoneImage.DOFade(1,0);
                mStoneImage.sprite = GetImage(Enums.EGameImage.WhiteStone);
                break;
        }
    }

    public void SelectMark(bool onMark)
    {
        if (onMark)
        {
            mUtilImage.DOFade(1,0);
            mUtilImage.sprite = GetImage(Enums.EGameImage.Selector);
        }
        else
        {
            mUtilImage.DOFade(0,0);
            mUtilImage.sprite = GetImage(Enums.EGameImage.None);
        }
    }
    public void PlacedMark(bool onMark)
    {
        if (onMark)
        {
            mUtilLastImage.DOFade(1,0);
            mUtilLastImage.sprite = GetImage(Enums.EGameImage.LastPosMark);
        }
        else
        {
            mUtilLastImage.DOFade(0,0);
            mUtilLastImage.sprite = GetImage(Enums.EGameImage.None);
        }
    }

    public void OnForbbiden(bool isForbidden,BasePlayerState playerBlack)
    {
        if (isForbidden)
        {
            IsForbidden = true;
            playerBlack.onForbbidenMark += ForbbidneMark;

        }
        else
        {
            IsForbidden = false;
            playerBlack.onForbbidenMark -= ForbbidneMark;
            
            mUtilImage.DOFade(0,0);
            mUtilImage.sprite = GetImage(Enums.EGameImage.None);
        }
    }

    public void ForbbidneMark(bool onMark)
    {
        if (onMark)
        {
            mUtilImage.DOFade(1,0);
            mUtilImage.sprite = GetImage(Enums.EGameImage.XMarker);
        }
        else
        {
            mUtilImage.DOFade(0,0);
            mUtilImage.sprite = GetImage(Enums.EGameImage.None);
        }
    }

    public Sprite GetImage(Enums.EGameImage GameImage)
    {
        if (GameImage == Enums.EGameImage.None)
        {
            return null;
        }
        return mImages[(int)GameImage];
    }

    public void FadeMode(BasePlayerState player)
    {

        float alpha = fading == mFadeCount ? 1f : (float)fading / mFadeCount; 
        fading--;

        mStoneImage.DOFade(alpha, 0); 

        if (fading < 0)
        {
            player.onMode -= FadeMode;
        }
    }
}
