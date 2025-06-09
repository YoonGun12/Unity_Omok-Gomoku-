using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;

/// <summary>
/// 테스트 사용방법 : BuyProduct에 EItemType 매개변수넣고 호출(테스트는 버튼에 int로 받아 EItemType으로 변환)
/// </summary>
public class IAPManager : Singleton<IAPManager>, IStoreListener
{
    //상품의 ID 부여 (앱 상품 페이지에서 직접 설정)
    public const string PRODUCT_ID_COIN_1000 = "coin_1000";
    public const string PRODUCT_ID_COIN_2000 = "coin_2000";
    public const string PRODUCT_ID_COIN_4500 = "coin_4500";
    public const string PRODUCT_ID_COIN_10000 = "coin_10000";
    public const string PRODUCT_ID_NOADS = "noads";
    public const string PRODUCT_ID_NOADS_COIN_2000 = "noads_coin_2000";
  
    private IStoreController mStoreController; //구매 과정을 제어하는 함수를 제공
    private IExtensionProvider mStoreExtensionProvider; //여러 플랫폼을 위한 확장 처리를 제공

    //광고 상품 결제 테스트(하드 코딩)
    public bool isTest = true;

    /*private void Start()
    {
        if (mStoreController == null)
        {
            InitPurchasing();
        }
    }*/
    
    //초기화 작업
    private void InitPurchasing()
    {
        if (IsInitialized())
        {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance()); //유니티가 기본적으로 제공하는 스토어 설정
        builder.AddProduct(PRODUCT_ID_COIN_1000, ProductType.Consumable);
        builder.AddProduct(PRODUCT_ID_COIN_2000, ProductType.Consumable);
        builder.AddProduct(PRODUCT_ID_COIN_4500, ProductType.Consumable);
        builder.AddProduct(PRODUCT_ID_COIN_10000, ProductType.Consumable);
        builder.AddProduct(PRODUCT_ID_NOADS, ProductType.NonConsumable);
        builder.AddProduct(PRODUCT_ID_NOADS_COIN_2000, ProductType.NonConsumable);
        UnityPurchasing.Initialize(this, builder);
    }
    
    private bool IsInitialized()
    {
        return mStoreController != null && mStoreExtensionProvider != null;
    }

    public void BuyProduct(Enums.EItemType itemType)
    {
        if (isTest)
        {
            int coinAmount = 0;
            string message = "";
            
            switch (itemType)
            {
                case Enums.EItemType.Coin_1000:
                    coinAmount = 1000;
                    message = "코인이 1,000개 지급되었습니다!";
                    break;
                case Enums.EItemType.Coin_2000:
                    coinAmount = 2000;
                    message = "코인이 2,000개 지급되었습니다!";
                    break;
                case Enums.EItemType.Coin_4500:
                    coinAmount = 4500;
                    message = "코인이 4,500개 지급되었습니다!";
                    break;
                case Enums.EItemType.Coin_10000:
                    coinAmount = 10000;
                    message = "코인이 10,000개 지급되었습니다!";
                    break;
                case Enums.EItemType.NoAds:
                    UniTask.Void(async () =>
                    {
                        await NetworkManager.Instance.RemoveAds(() =>
                        {
                            GameManager.Instance.OpenConfirmPanel("광고제거가 적용되었습니다!", null, false);
                            GameManager.Instance.OnAdsRemoved?.Invoke();
                        }, () =>
                        {
                            GameManager.Instance.OpenConfirmPanel("광고제거 실패", null, false);
                        });
                    });
                    return;

                case Enums.EItemType.NoAds_Coin_2000:

                    UniTask.Void(async () =>
                    {
                        await NetworkManager.Instance.RemoveAds(() =>
                        {
                            GameManager.Instance.OnAdsRemoved?.Invoke();
                        }, () =>
                        {
                            GameManager.Instance.OpenConfirmPanel("광고제거 실패", null, false);
                            return;
                        });

                        await NetworkManager.Instance.AddCoin(2000, i =>
                        {
                            GameManager.Instance.OpenConfirmPanel("광고제거와 코인이 2,000개 지급되었습니다!", null, false);
                            AudioManager.Instance.PlaySfxSound(6);
                            GameManager.Instance.OnCoinUpdated?.Invoke();
                        }, () =>
                        {
                            GameManager.Instance.OpenConfirmPanel("구매 오류", null, false);
                        });
                    });
                    return;
                default:
                    Debug.Log("테스트 모드: 알 수 없는 아이템");
                    return;
            }
            
            UniTask.Void(async () =>
            {
                await NetworkManager.Instance.AddCoin(coinAmount, i =>
                {
                    GameManager.Instance.OpenConfirmPanel(message, null, false);
                    AudioManager.Instance.PlaySfxSound(6);
                    GameManager.Instance.OnCoinUpdated?.Invoke();
                }, () =>
                {
                    GameManager.Instance.OpenConfirmPanel("코인 지급 실패 (테스트)", null, false);
                });
            });

            return;
            
        }
        
        switch (itemType)
        {
            case Enums.EItemType.Coin_1000:
                BuyProductID(PRODUCT_ID_COIN_1000);
                break;
            case Enums.EItemType.Coin_2000:
                BuyProductID(PRODUCT_ID_COIN_2000);
                break;
            case Enums.EItemType.Coin_4500:
                BuyProductID(PRODUCT_ID_COIN_4500);
                break;
            case Enums.EItemType.Coin_10000:
                BuyProductID(PRODUCT_ID_COIN_10000);
                break;
            case Enums.EItemType.NoAds:
                BuyProductID(PRODUCT_ID_NOADS);
                break;
            case Enums.EItemType.NoAds_Coin_2000:
                BuyProductID(PRODUCT_ID_NOADS_COIN_2000);
                break;
        }
    }
    
    private void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = mStoreController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                mStoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }
    
    
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        mStoreController = controller;
        mStoreExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializedFailed InitializationFailureReason:" + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log("OnInitializedFailed InitializationFailureReason:" + error + "message:" + message);
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(string.Format("OnPurchaseFailed : FAIL. Product: '{0}', PurchaseFailureResone: {1}",
            product.definition.storeSpecificId, failureReason));
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        if (string.Equals(purchaseEvent.purchasedProduct.definition.id, PRODUCT_ID_COIN_1000, StringComparison.Ordinal))
        {
            UniTask.Void(async () =>
            {
                await NetworkManager.Instance.AddCoin(1000, i =>
                {
                    GameManager.Instance.OpenConfirmPanel("코인이 1,000개 지급되었습니다!", null, false);
                    AudioManager.Instance.PlaySfxSound(6);
                    GameManager.Instance.OnCoinUpdated?.Invoke();
                }, () =>
                {
                    GameManager.Instance.OpenConfirmPanel("구매 오류", null, false);
                } );
            });
        }
        else if (string.Equals(purchaseEvent.purchasedProduct.definition.id, PRODUCT_ID_COIN_2000, StringComparison.Ordinal))
        {
            UniTask.Void(async () =>
            {
                await NetworkManager.Instance.AddCoin(2000, i =>
                {
                    GameManager.Instance.OpenConfirmPanel("코인이 2,000개 지급되었습니다!", null, false);
                    AudioManager.Instance.PlaySfxSound(6);
                    GameManager.Instance.OnCoinUpdated?.Invoke();
                }, () =>
                {
                    GameManager.Instance.OpenConfirmPanel("구매 오류", null, false);
                } );
            });
        }
        else if (string.Equals(purchaseEvent.purchasedProduct.definition.id, PRODUCT_ID_COIN_4500, StringComparison.Ordinal))
        {
            UniTask.Void(async () =>
            {
                await NetworkManager.Instance.AddCoin(4500, i =>
                {
                    GameManager.Instance.OpenConfirmPanel("코인이 4,500개 지급되었습니다!", null, false);
                    AudioManager.Instance.PlaySfxSound(6);
                    GameManager.Instance.OnCoinUpdated?.Invoke();
                }, () =>
                {
                    GameManager.Instance.OpenConfirmPanel("구매 오류", null, false);
                } );
            });
        }
        else if (string.Equals(purchaseEvent.purchasedProduct.definition.id, PRODUCT_ID_COIN_10000, StringComparison.Ordinal))
        {
            UniTask.Void(async () =>
            {
                await NetworkManager.Instance.AddCoin(10000, i =>
                {
                    GameManager.Instance.OpenConfirmPanel("코인이 10,000개 지급되었습니다!", null, false);
                    AudioManager.Instance.PlaySfxSound(6);
                    GameManager.Instance.OnCoinUpdated?.Invoke();
                }, () =>
                {
                    GameManager.Instance.OpenConfirmPanel("구매 오류", null, false);
                } );
            });
        }
        else if (string.Equals(purchaseEvent.purchasedProduct.definition.id, PRODUCT_ID_NOADS, StringComparison.Ordinal))
        {
            UniTask.Void(async () =>
            {
                await NetworkManager.Instance.RemoveAds(() =>
                {
                    GameManager.Instance.OpenConfirmPanel("광고제거가 적용되었습니다!", null, false);
                    GameManager.Instance.OnAdsRemoved?.Invoke();
                }, () =>
                {
                    GameManager.Instance.OpenConfirmPanel("광고제거 실패", null, false);
                });
            });
        }
        else if (string.Equals(purchaseEvent.purchasedProduct.definition.id, PRODUCT_ID_NOADS_COIN_2000, StringComparison.Ordinal))
        {
            UniTask.Void(async () =>
            {
                await NetworkManager.Instance.RemoveAds(() =>
                {
                }, () =>
                {
                    GameManager.Instance.OpenConfirmPanel("광고제거 실패", null, false);
                    return;
                });
            
                await NetworkManager.Instance.AddCoin(2000, i =>
                {
                    GameManager.Instance.OpenConfirmPanel("광고제거와 코인이 2,000개 지급되었습니다!", null, false);
                    AudioManager.Instance.PlaySfxSound(6);
                    GameManager.Instance.OnCoinUpdated?.Invoke();
                }, () =>
                {
                    GameManager.Instance.OpenConfirmPanel("구매 오류", null, false);
                } );
            });
        }
        else
        {
            Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", purchaseEvent.purchasedProduct.definition.id));
        }

        return PurchaseProcessingResult.Complete;
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
}
