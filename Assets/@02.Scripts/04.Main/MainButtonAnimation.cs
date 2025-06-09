using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MainButtonAnimation : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject mOmoknuniPrefab;        //날라오는 새 프리팹
    [SerializeField] private GameObject mBlackStonePrefab;      //오목판에 놓이는 흑돌
    [SerializeField] private GameObject mWhiteStonePrefab;      //오목판에 놓이는 백돌
    [SerializeField] private GameObject collisionEffectPrefab;  //메인 버튼과 충돌효과
    [SerializeField] private GameObject stackEffectPrefab;      //오목판 놓일때 효과

    [Header("Transform")]
    [SerializeField] private Transform[] mStartPos;         // 새가 날라오는 시작 위치들
    [SerializeField] private Transform[] mStoneTargets;     // 오목눈이의 버튼 타겟
    [SerializeField] private Transform[] mFalling;          // 오목판 떨어지는 위치들

    [Header("Animation")]
    [SerializeField] private float flyDuration = 0.5f;
    [SerializeField] private float fallDuration = 0.5f;

    [Header("UI")]
    [SerializeField] private Sprite blackStoneSprite;       //메인 버튼에 있는 흑돌
    [SerializeField] private Sprite whiteStoneSprite;       //메인 버튼에 있는 백돌

    private bool isBlackTurn = true;                        //흑, 백 번갈아
    private List<int> occupiedIndexes = new List<int>();
    private List<GameObject> placedStone = new List<GameObject>();
    private int[] miniBoard = new int[9];
    private bool isAnimationProgress = false;
    
    /// <summary>
    /// 메인메뉴 버튼을 클릭했을 때
    /// </summary>
    /// <param name="buttonIndex">0 : 대국시작, 1: 내 기보, 2: 랭킹, 3: 상점, 4: 설정, 5: 게임종료</param>
    /// <param name="onClickAction">버튼 애니메이션 후 진행할 액션</param>
    public void StartClickAnimation(int buttonIndex, Action onClickAction)
    {
        if (isAnimationProgress) return;

        isAnimationProgress = true;
        
        int ranStartIndex = Random.Range(0, mStartPos.Length);      //처음 새가 생길 위치 정하기 위한 랜덤값
        Vector3 startPos = mStartPos[ranStartIndex].position;       //새의 시작위치
        Vector3 targetPos = mStoneTargets[buttonIndex].position;    //새가 도착할 위치(index를 받아와서 이동)

        var omoknuni = Instantiate(mOmoknuniPrefab, startPos, Quaternion.identity);     //새 프리팹 생성
        
        // 새 날아가기 애니메이션  시작위치 >>> 메인버튼
        omoknuni.transform.DOMove(targetPos, flyDuration).SetEase(Ease.InQuad).OnComplete(() =>
        {
            PlayHitAnimation(omoknuni, buttonIndex); 
            PlayFallingStoneAnimation(buttonIndex);
            ExitAnimation(omoknuni, targetPos, buttonIndex);
            UpdateButtonStateDelayed(onClickAction);
        });
    }

    /// <summary>
    /// 새가 버튼과 부딪히는 애니메이션
    /// </summary>
    /// <param name="omoknuni">새 프리팹</param>
    /// <param name="buttonIndex"></param>
    private void PlayHitAnimation(GameObject omoknuni, int buttonIndex)
    {
        AudioManager.Instance.PlaySfxSound(3);                                          //새 소리 재생
        omoknuni.GetComponent<Animator>().SetBool("isHit", true);                       //애니메이션 Hit으로 전환
        Instantiate(collisionEffectPrefab, omoknuni.transform.position, Quaternion.identity);//충돌 효과 생성
        mStoneTargets[buttonIndex].gameObject.SetActive(false);                              //메인 버튼에 있는 돌 비활성화
    }

    private void PlayFallingStoneAnimation(int buttonIndex)
    {
        List<int> emptyIndexes = new();
        for (int i = 0; i < mFalling.Length; i++)
        {
            if(!occupiedIndexes.Contains(i)) emptyIndexes.Add(i);
        }

        if (emptyIndexes.Count == 0)
        {
            bool isWin = CheckMiniGameWin();
            
            if (!isWin)
            {
                ResetStoneState();
            }
            
            
            for (int i = 0; i < mFalling.Length; i++)
            {
                emptyIndexes.Add(i);
            }
        }

        int targetIndex = emptyIndexes[Random.Range(0, emptyIndexes.Count)];
        Vector3 spawnPos = mStoneTargets[buttonIndex].position;                                         //오목판에 놓일 돌 생성 위치
        occupiedIndexes.Add(targetIndex);
        
        GameObject stonePrefab = isBlackTurn ? mBlackStonePrefab : mWhiteStonePrefab;                   //순서에 따라 흑백 돌 결정
        var fallingStone = Instantiate(stonePrefab, spawnPos, quaternion.identity); //돌 프리팹 생성
        placedStone.Add(fallingStone);

        Vector3 fallTarget = mFalling[targetIndex].position;   //오목판 위치 결정

        fallingStone.transform.DOScale(1.1f, 0.1f).SetLoops(2, LoopType.Yoyo);              //돌 크기 요요
        fallingStone.transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360);    //돌 회전
        fallingStone.transform.DOMove(fallTarget, 0.5f).SetEase(Ease.InQuad).OnComplete(() =>       //돌 떨어지기
        {
            AudioManager.Instance.PlaySfxSound(2);                                              //착수 소리 재생
            Instantiate(stackEffectPrefab, fallingStone.transform.position, Quaternion.identity);    //착수 효과 생성
            miniBoard[targetIndex] = isBlackTurn ? 1 : 2;
            CheckMiniGameWin();
        });
    }

    private void ExitAnimation(GameObject omoknuni, Vector3 origin, int buttonIndex)
    {
        Vector3 randomOffset = new Vector3(Random.Range(-5f, 5f), Random.Range(-10f, 10f), Random.Range(-5f, 5f));  //새가 충돌 후 어느 방향으로 날아갈 것인지 랜덤값
        Vector3 endPos = origin + randomOffset; //충돌 후 날아갈 위치

        omoknuni.transform.DORotate(new Vector3(0, 0, 360), fallDuration, RotateMode.FastBeyond360);    //새 회전
        omoknuni.transform.DOMove(endPos, fallDuration).SetEase(Ease.OutQuad).OnComplete(() =>          //새 충돌 후 이동
        {
            Destroy(omoknuni);
            mStoneTargets[buttonIndex].gameObject.SetActive(true);
        });
    }

    private void UpdateButtonStateDelayed(Action onClickAction)
    {
        DOVirtual.DelayedCall(1f, () =>
        {
            onClickAction?.Invoke();    //버튼 각 기능들 수행
            ToggleTurnColor();          //흑백 순서 결정

            for (int i = 0; i < mStoneTargets.Length; i++)
            {
                mStoneTargets[i].GetComponent<Image>().sprite = isBlackTurn ? blackStoneSprite : whiteStoneSprite;
            }

            isAnimationProgress = false;
        });
    }

    private void ToggleTurnColor()
    {
        isBlackTurn = !isBlackTurn;
    }

    public void HideAllStone()
    {
        foreach (var stone in placedStone)
        {
            if(stone != null) stone.SetActive(false);
        }
    }

    public void ShowAllStone()
    {
        foreach (var stone in placedStone)
        {
            if(stone != null) stone.SetActive(true);
        }
    }

    public void ResetStoneState()
    {
        // 놓인 돌들 제거
        foreach (var stone in placedStone)
        {
            if (stone != null)
            {
                stone.transform.DOScale(0, 0.2f).SetEase(Ease.InBack).OnComplete(() => Destroy(stone));
            }
        }
        Array.Clear(miniBoard, 0, miniBoard.Length);
        HideAllStone();
        
        placedStone.Clear();
        occupiedIndexes.Clear();

        isBlackTurn = true;

        for (int i = 0; i < mStoneTargets.Length; i++)
        {
            mStoneTargets[i].GetComponent<Image>().sprite = blackStoneSprite;
        }
    }

    private bool CheckMiniGameWin()
    {
        int[][] winPatterns = new int[][]
        {
            new[] { 0, 1, 2 },
            new[] { 3, 4, 5 },
            new[] { 6, 7, 8 },
            new[] { 0, 3, 6 },
            new[] { 1, 4, 7 },
            new[] { 2, 5, 8 },
            new[] { 0, 4, 8 },
            new[] { 2, 4, 6 }
        };

        foreach (var pattern in winPatterns)
        {
            int a = pattern[0];
            int b = pattern[1];
            int c = pattern[2];

            if (miniBoard[a] != 0 && miniBoard[a] == miniBoard[b] && miniBoard[b] == miniBoard[c])
            {
                bool isBlackWin = miniBoard[a] == 1;
                
                ResetStoneState();
                
                UniTask.Void(async () =>
                {
                    await NetworkManager.Instance.AddCoin(50, i =>
                    {
                        GameManager.Instance.OpenConfirmPanel($"?\n{(isBlackWin? "흑":"백")}이 이겼네요?\n50코인이라도...", ()=>AudioManager.Instance.PlaySfxSound(5), false);
                        GameManager.Instance.OnMainPanelUpdate?.Invoke();
                    }, () =>
                    {
                        GameManager.Instance.OpenConfirmPanel("미니게임 보상 오류", null, false);
                    } );
                });
                return true;
            }
        }

        return false;
    }
}
