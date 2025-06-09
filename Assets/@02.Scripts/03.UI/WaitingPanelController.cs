using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingPanelController : PopupPanelController
{
    // 남은 시간을 표시할 스크롤바와 텍스트
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private float mProgressDuration = 30.0f;    // 매칭 제한시간
    
    private Coroutine mProgressCoroutine;
    
    public override void Show()
    {
        base.Show();
        
        StartProgressBar();
    }

    private void Hide()
    {
        GameManager.Instance.bIsStartGame =false;
        StopProgressBar();
        
        base.Hide();
    }
    
    public void OnClickCancelButton()
    {
        StopProgressBar();
        
        Hide(() =>
        {
            GameManager.Instance.OpenConfirmPanel("매칭을 취소하였습니다. \n소비한 코인을 돌려드립니다.", () =>
            {
                UniTask.Void(async () =>
                {
                    await NetworkManager.Instance.AddCoin(Constants.ConsumeCoin, i =>
                    {
                        GameManager.Instance.ChangeToMainScene();
                    }, () =>
                    {
                        GameManager.Instance.OpenConfirmPanel("돌려 받지 못함", null, false);
                    });
                });
            }, false);
        });
        
    }
    
    private void StartProgressBar()
    {
        if (mProgressCoroutine != null)
        {
            StopCoroutine(mProgressCoroutine);
        }

        mProgressCoroutine = StartCoroutine(FillProgressBar());
    }

    private void StopProgressBar()
    {
        if (mProgressCoroutine != null)
        {
            StopCoroutine(mProgressCoroutine);
            mProgressCoroutine = null;
        }
        
        progressBar.value = 0.0f;
        progressText.text = $"{mProgressDuration}초";
    }

    private IEnumerator FillProgressBar()
    {
        float time = 0.0f;
        float duration = mProgressDuration;

        while (time < duration)
        {
            time += Time.deltaTime;
            progressBar.value = time / duration;
            
            // 남은시간 표시
            float remainingTime = duration - time;
            progressText.text = string.Format("{0:0}초", remainingTime);

            if (GameManager.Instance.bIsStartGame)
            {
                this.Hide();
            }
            
            yield return null;
        }
        
        // 제한시간이 지나면
        OnMatchingTimeout();
    }
    
    private void OnMatchingTimeout()
    {
        Hide();

        if (GameManager.Instance.bIsTryRematch)
        {
            UnityThread.executeInUpdate(() =>
            {
                GameManager.Instance.OpenConfirmPanel("상대방이 응답하지 않았습니다. \n코인을 돌려받고 \n메인 화면으로 돌아갑니다.", () =>
                {
                    UniTask.Void(async () =>
                    {
                        await NetworkManager.Instance.AddCoin(Constants.ConsumeCoin, i =>
                        {
                            GameManager.Instance.ChangeToMainScene();
                        }, () =>
                        {
                            GameManager.Instance.OpenConfirmPanel("돌려 받지 못함", null, false);
                        });
                    });
                }, false);
            });
        }
        else
        {
            GameManager.Instance.OpenConfirmPanel("다른 유저와의 매칭이 실패하였습니다. \n급수에 맞는 AI와 매칭됩니다.", () =>
            {
                GameManager.Instance.ChangeToGameScene(Enums.EGameType.SinglePlay);
            }, true, () =>
            {
                GameManager.Instance.OpenConfirmPanel("매칭을 취소하였습니다. \n소비한 코인을 돌려드립니다.", () =>
                {
                    UniTask.Void(async () =>
                    {
                        await NetworkManager.Instance.AddCoin(Constants.ConsumeCoin, i =>
                        {
                            GameManager.Instance.ChangeToMainScene();
                        }, () =>
                        {
                            GameManager.Instance.OpenConfirmPanel("돌려 받지 못함", null, false);
                        });
                    });
                }, false);
            });
        }
    }
}
