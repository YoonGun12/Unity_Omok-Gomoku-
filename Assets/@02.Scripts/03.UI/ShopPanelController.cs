using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelController : PopupPanelController
{
    public TMP_Text coinText;
    public Button[] NoAdsButtons;

    private async void OnEnable()
    {
        UpdateCoin();
        GameManager.Instance.OnCoinUpdated += UpdateCoin;
        GameManager.Instance.OnAdsRemoved += UpdateAdButtons;

        var userInfo = await NetworkManager.Instance.GetUserInfo(() => { }, () => { });
        if (userInfo.hasadremoval)
        {
            UpdateAdButtons();
        }
    }

    private void OnDisable()
    {
        GameManager.Instance.OnCoinUpdated -= UpdateCoin;
        GameManager.Instance.OnAdsRemoved -= UpdateAdButtons;
    }

    private void UpdateCoin()
    {
        NetworkManager.Instance.GetUserInfo(() =>
        {
            
        }, () =>
        {
            Debug.Log("코인 로드 실패");
            
        }).ContinueWith(userInfo =>
        {
            int coin = userInfo.coin;
            coinText.text = $"{coin:N0} 코인";

        });
    }

    private void UpdateAdButtons()
    {
        NoAdsButtons[0].interactable = false;
        NoAdsButtons[1].interactable = false;
    }

    public void OnClickCloseButton()
    {
        Hide(GameManager.Instance.OnMainPanelUpdate);
    }

    public void OnClickRewardAds()
    {
        AdmobAdsManager.Instance.ShowRewardedAd();
    }

    public void OnClickShopItem(int type)
    {
        Enums.EItemType selectedItem = (Enums.EItemType)type;
        IAPManager.Instance.BuyProduct(selectedItem);
    }
    
    public override void Hide(Action OnPanelControllerHide = null)
    {
        FindObjectOfType<MainButtonAnimation>().ShowAllStone();

        base.Hide(OnPanelControllerHide);
    }
}
