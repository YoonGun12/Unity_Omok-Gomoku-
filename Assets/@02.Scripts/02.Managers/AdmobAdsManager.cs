using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using GoogleMobileAds.Api;
using UnityEngine.SceneManagement;
using UserDataStructs;


/// <summary>
/// 사용방법 : ShowRewardedAd 호출
/// </summary>
public class AdmobAdsManager : Singleton<AdmobAdsManager>
{
#if UNITY_ANDROID
    private string mRewardedAdUnitID = "ca-app-pub-3940256099942544/5224354917";// 보상형 광고 Test ID
    //private string mRewardedAdUnitID = "ca-app-pub-7882694754839983/3707741912" //보상형 광고 ID;
#endif

    private RewardedAd mRewardedAd;
    
    private void Start()
    {
        //구글 모바일 Ads SDK 초기화
        MobileAds.Initialize(initStatus =>
        {
            //보상형광고 표시
            LoadRewardedAd();
        });
    }

    #region Rewarded Ads

    public void LoadRewardedAd()
    {
        if (mRewardedAd != null)
        {
            mRewardedAd.Destroy();
            mRewardedAd = null;
        }
        var adRequest = new AdRequest();
        RewardedAd.Load(mRewardedAdUnitID, adRequest, (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("보상형광고 로드가 실패 :" + error);
                return;
            }

            mRewardedAd = ad;

            RegisterRewardedAdEventHandlers(mRewardedAd);
        });
    }

    public async void ShowRewardedAd() //코인 지급을 하나의 메서드로 빼도 될듯
    {
        UserInfoResult userinfo = await NetworkManager.Instance.GetUserInfo(() => { }, () => { });

        if (userinfo.hasadremoval)
        {
            await NetworkManager.Instance.AddCoin(500, i =>
            {
                GameManager.Instance.OpenConfirmPanel("코인이 500개 지급되었습니다!", null, false);
                AudioManager.Instance.PlaySfxSound(5);
                GameManager.Instance.OnCoinUpdated?.Invoke();
            }, () =>
            {
                GameManager.Instance.OpenConfirmPanel("오류", null, false);
            });
            return;
        }
        
        
        //광고 제거 아이템이 없을 떄
        if (mRewardedAd != null && mRewardedAd.CanShowAd())
        {
            mRewardedAd.Show(async (Reward reward) =>
            {
                await NetworkManager.Instance.AddCoin(500, i =>
                {
                    GameManager.Instance.OpenConfirmPanel("코인이 500개 지급되었습니다!", null, false);
                    GameManager.Instance.OnCoinUpdated?.Invoke();
                }, () =>
                {
                    GameManager.Instance.OpenConfirmPanel("오류", null, false);
                });
                
            });
        }
    }
    
    private void RegisterRewardedAdEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
            
            // Reload the ad so that we can show another as soon as possible.
            LoadRewardedAd();
        };
    }

    #endregion

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
}
