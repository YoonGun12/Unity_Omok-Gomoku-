using System;
using System.Collections;
using System.Collections.Generic;
using AudioEnums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserDataStructs;

/// <summary>
/// 메인 화면에서 UI 버튼 입력을 처리하는 컨트롤러.
/// GameManager를 통해 각종 패널을 열거나 씬을 전환하는 역할을 수행.
/// </summary>
public class MainPanelController : MonoBehaviour
{
    [SerializeField] private Image profileImage;
    [SerializeField] private TMP_Text userInfoText;
    [SerializeField] private TMP_Text coinText;
    private MainButtonAnimation mainButtonAnimation;

    private void Awake()
    {
        mainButtonAnimation = GetComponent<MainButtonAnimation>();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.TryCloseTopmostPopup())
                return;

            GameManager.Instance.OpenConfirmPanel("정말 게임을 종료하시겠습니까?", () =>
            {
                Application.Quit();
            }, true);
        }
    }
    
    public async void SetProfileInfo()
    {
        UserInfoResult userInfo = await NetworkManager.Instance.GetUserInfo(() => { }, () => { });

        profileImage.sprite = GameManager.Instance.GetProfileSprite(userInfo.profileimageindex);
        userInfoText.text = $"{userInfo.rank}급 {userInfo.nickname}";
        coinText.text = $"코인: {userInfo.coin}";
    }
    
    public void OnClickStartButton()
    {
        mainButtonAnimation.StartClickAnimation(0, ()=>
        {
            mainButtonAnimation.HideAllStone();
            GameManager.Instance.OpenGameTypeSelectPanel();
        });
    }
    
    public void OnClickRecordButton()
    {
        mainButtonAnimation.StartClickAnimation(1, ()=>
        {
            mainButtonAnimation.HideAllStone();
            GameManager.Instance.OpenRecordPanel();
        });
    }

    public void OnClickLeaderboardButton()
    {
        mainButtonAnimation.StartClickAnimation(2, ()=>
        {
            mainButtonAnimation.HideAllStone();
            GameManager.Instance.OpenLeaderboardPanel();
        });
    }
    public void OnClickShopButton()
    {
        mainButtonAnimation.StartClickAnimation(3, ()=>
        {
            mainButtonAnimation.HideAllStone();
            GameManager.Instance.OpenShopPanel();
        });
    }

    public void OnClickSettingsButton()
    {
        mainButtonAnimation.StartClickAnimation(4, ()=>
        {
            mainButtonAnimation.HideAllStone();
            GameManager.Instance.OpenSettingsPanel();
        });
    }

    public void OnClickProfileButton()
    {
        AudioManager.Instance.PlayAudioClip(ESfxType.Bird);
        mainButtonAnimation.HideAllStone();
        GameManager.Instance.OpenProfilePanel();
    }
    
    // 로그아웃 클릭 시 호출되는 메서드 구현
    public void OnClickExitButton()
    {
        GameManager.Instance.OpenConfirmPanel("정말 게임을 종료하시겠습니까?", ()=>Application.Quit(),true, () => { }) ;
        
    }
}
