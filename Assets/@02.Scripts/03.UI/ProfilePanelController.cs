using System;
using System.Collections;
using System.Collections.Generic;
using AudioEnums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserDataStructs;

public class ProfilePanelController : PopupPanelController
{
    [SerializeField] private TextMeshProUGUI mNicknameText;
    [SerializeField] private TextMeshProUGUI mEmailText;
    [SerializeField] private TextMeshProUGUI mCoinText;
    [SerializeField] private TextMeshProUGUI mWinText;
    [SerializeField] private TextMeshProUGUI mDrawText;
    [SerializeField] private TextMeshProUGUI mLoseText;
    [SerializeField] private TextMeshProUGUI mWinRateText;
    [SerializeField] private TextMeshProUGUI mRankupPoinText;
    [SerializeField] private TextMeshProUGUI mRankText;
    [SerializeField] private TextMeshProUGUI mWinLoseStreakText;
    [SerializeField] private TextMeshProUGUI mADBlockText;
    [SerializeField] private Image mWinLoseStreakImageObj;
    [SerializeField] private Sprite mWinStreakImage;
    [SerializeField] private Sprite mLoseStreakImage;
    [SerializeField] private Image mProfileImage;
    
    private List<PopupPanelController> mChildPanels = new List<PopupPanelController>();

    public override async void Show()
    {
        mChildPanels.Clear();
        
        UserInfoResult userInfo = await NetworkManager.Instance.GetUserInfo(() => { }, () => { });
        setProfileInfo(userInfo);
        base.Show();
    }

    public override void Hide(Action hideDelegate = null)
    {
        FindObjectOfType<MainButtonAnimation>().ShowAllStone();
        
        foreach (var panel in mChildPanels)
        {
            if (panel != null)
            {
                panel.Hide();
            }
        }
        mChildPanels.Clear();
        
        base.Hide(hideDelegate);
    }

    public void OnClickProfileButton()
    {
        AudioManager.Instance.PlayAudioClip(ESfxType.Bird);
        var childPanel = GameManager.Instance.OpenSelectProfilePanelFromProfilePanel();
        SelectProfilePanelController selectPanel = childPanel as SelectProfilePanelController;
        if (selectPanel != null)
        {
            selectPanel.OnProfileSelected = UpdateProfileInfo;
        }
        mChildPanels.Add(childPanel);
    }

    public void OnClickBackButton()
    {
        GameManager.Instance.OnMainPanelUpdate();
        Hide();
    }

    public async void UpdateProfileInfo()
    {
        UserInfoResult userInfo = await NetworkManager.Instance.GetUserInfo(() => { }, () => { });
        setProfileInfo(userInfo);
    }

    private void setProfileInfo(UserInfoResult userInfo)
    {
        mNicknameText.text = userInfo.nickname;
        mEmailText.text = userInfo.username;
        mCoinText.text = userInfo.coin.ToString();
        mWinText.text = userInfo.wincount.ToString();
        mDrawText.text = userInfo.drawcount.ToString();
        mLoseText.text = userInfo.losecount.ToString();

        if (userInfo.wincount > 0)
        {
            float winRateValue = (float)userInfo.wincount / (userInfo.wincount + userInfo.losecount) * 100f;
            mWinRateText.text = "Win Rate: "+winRateValue.ToString("F2") + "%";
        }
        else
        {
            mWinRateText.text = "Win Rate: "+"0%";
        }
        mRankText.text = userInfo.rank.ToString();
        mRankupPoinText.text = userInfo.rankuppoints.ToString() + " / "+ getRankChangeThreshold(userInfo.rank).ToString();

        if (userInfo.winlosestreak < 0)
        {
            mWinLoseStreakImageObj.sprite = mLoseStreakImage;
        }
        else
        {
            mWinLoseStreakImageObj.sprite = mWinStreakImage;
        }

        mWinLoseStreakText.text = Mathf.Abs(userInfo.winlosestreak).ToString();
        mADBlockText.text = userInfo.hasadremoval ? "O" : "X";
        mProfileImage.sprite = GameManager.Instance.GetProfileSprite(userInfo.profileimageindex);
    }

    private int getRankChangeThreshold(int rank)
    {
        if (rank >= 10) {
            return 3; // 10급 ~ 18급: 3점
        } 
        else if (rank >= 5) {
            return 5; // 5급 ~ 9급: 5점
        } 
        else 
        {
            return 10; // 1급 ~ 4급: 10점
        }
    }
}