using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UserDataStructs;

public class GamePanelController : MonoBehaviour
{
    [SerializeField] private GameObject timer;

    [SerializeField] private GameObject turnUI;
    [SerializeField] private GameObject forfietButton;

    [SerializeField] private CanvasGroup blackTurnPanel;
    [SerializeField] private CanvasGroup whiteTurnPanel;

    [SerializeField] private Image playerBlackProfileImage;
    [SerializeField] private TMP_Text playerBlackProfileText;
    [SerializeField] private Image playerWhiteProfileImage;
    [SerializeField] private TMP_Text playerWhiteProfileText;


    private const float mDisableAlpha = 0.3f;
    private const float mEnableAlpha = 1.0f;

    private MultiplayManager mMultiplayManager;

    //착수버튼이 눌렸을 때 플레이어에게 알림
    public delegate void OnBeginButtonClicked();

    public OnBeginButtonClicked onBeginButtonClicked;

    /// <summary>
    /// Play하고 있을 때는 턴을 표시하는 turnUI를 보여주고
    /// Record(기보)하고 있을 때는 recordUI를 보여줌
    /// </summary>
    /// <param name="gameUIState"></param>
    public void SetGameUI(Enums.EGameUIState gameUIState)
    {
        switch (gameUIState)
        {
            case Enums.EGameUIState.Turn_Black:
                timer.SetActive(true);
                turnUI.SetActive(true);
                forfietButton.SetActive(true);

                blackTurnPanel.alpha = mEnableAlpha;
                whiteTurnPanel.alpha = mDisableAlpha;
                break;
            case Enums.EGameUIState.Turn_White:
                timer.SetActive(true);
                turnUI.SetActive(true);
                forfietButton.SetActive(true);

                blackTurnPanel.alpha = mDisableAlpha;
                whiteTurnPanel.alpha = mEnableAlpha;
                break;
        }
    }

    public void SetMyProfile(Enums.EGameType gameType, Enums.EPlayerType playerType)
    {
        // 객체가 유효한지 확인
        if (playerBlackProfileImage == null || playerBlackProfileText == null ||
            playerWhiteProfileImage == null || playerWhiteProfileText == null)
        {
            Debug.LogWarning("프로필 UI 객체가 유효하지 않습니다.");
            return;
        }
        
        UserInfoResult userInfo = NetworkManager.Instance.GetUserInfoSync(() => { }, () => { });

        if (gameType == Enums.EGameType.PassAndPlay || gameType == Enums.EGameType.PassAndPlayFade)
        {
            playerBlackProfileImage.sprite = GameManager.Instance.GetProfileSprite(userInfo.profileimageindex);
            playerBlackProfileText.text = "Player 1";
        }
        else if (gameType == Enums.EGameType.SinglePlay)
        {
            playerBlackProfileImage.sprite = GameManager.Instance.GetProfileSprite(userInfo.profileimageindex);
            playerBlackProfileText.text = $"{userInfo.rank}급 {userInfo.nickname}";
        }
        else if (gameType == Enums.EGameType.MultiPlay)
        {
            if (playerType == Enums.EPlayerType.Player_Black)
            {
                playerBlackProfileImage.sprite = GameManager.Instance.GetProfileSprite(userInfo.profileimageindex);
                playerBlackProfileText.text = $"{userInfo.rank}급 {userInfo.nickname}";
            }
            else if (playerType == Enums.EPlayerType.Player_White)
            {
                playerWhiteProfileImage.sprite = GameManager.Instance.GetProfileSprite(userInfo.profileimageindex);
                playerWhiteProfileText.text = $"{userInfo.rank}급 {userInfo.nickname}";
            }
        }
    }

    public void SetOpponentProfile(UsersInfoData opponentInfo)
    {
        UnityThread.executeInUpdate(() =>
        {
            Debug.Log($"상대방 프로필 닉네임: {opponentInfo.nickname}");
            Debug.Log($"상대방 프로필 이미지 번호: {opponentInfo.profileimageindex}");
            Debug.Log($"상대방 프로필 급수: {opponentInfo.rank}");
            
            // 객체가 유효한지 확인
            if (playerBlackProfileImage == null || playerBlackProfileText == null ||
                playerWhiteProfileImage == null || playerWhiteProfileText == null)
            {
                Debug.LogWarning("프로필 UI 객체가 유효하지 않습니다.");
                return;
            }

            if (opponentInfo == null) return;

            if (opponentInfo.playerType == Enums.EPlayerType.Player_Black)
            {
                playerBlackProfileImage.sprite = GameManager.Instance.GetProfileSprite(opponentInfo.profileimageindex);
                playerBlackProfileText.text = $"{opponentInfo.rank}급 {opponentInfo.nickname}";
            }
            else if (opponentInfo.playerType == Enums.EPlayerType.Player_White)
            {
                playerWhiteProfileImage.sprite = GameManager.Instance.GetProfileSprite(opponentInfo.profileimageindex);
                playerWhiteProfileText.text = $"{opponentInfo.rank}급 {opponentInfo.nickname}";
            }
        });
    }

    public void SetOpponentProfile_NonMultiplay(Enums.EGameType gameType, Enums.EDifficultyLevel difficultyLevel)
    {
        UserInfoResult userInfo = NetworkManager.Instance.GetUserInfoSync(() => { }, () => { });
        
        if (gameType == Enums.EGameType.PassAndPlay || gameType == Enums.EGameType.PassAndPlayFade)
        {
            playerWhiteProfileImage.sprite = GameManager.Instance.GetProfileSprite(userInfo.profileimageindex);
            playerWhiteProfileText.text = "Player 2";
        }
        else if (gameType == Enums.EGameType.SinglePlay)
        {
            playerWhiteProfileImage.sprite = GameManager.Instance.GetProfileSprite(userInfo.profileimageindex);
            
            if (difficultyLevel == Enums.EDifficultyLevel.Easy)
            {
                playerWhiteProfileText.text = "하급 AI";
            }
            else if (difficultyLevel == Enums.EDifficultyLevel.Medium)
            {
                playerWhiteProfileText.text = "중급 AI";
            }
            else if (difficultyLevel == Enums.EDifficultyLevel.Hard)
            {
                playerWhiteProfileText.text = "상급 AI";
            }
        }
    }
    
    #region TimerController

    public void InitClock()
    {
        timer.GetComponent<Timer>().InitTimer();
    }

    public void StartClock()
    {
        timer.GetComponent<Timer>().StartTimer();
    }

    public void StopClock()
    {
        timer.GetComponent<Timer>().PauseTimer();
    }

    #endregion

    /// <summary>
    /// 기권버튼 클릭 시 패배 처리 호출하는 메서드
    /// 확인 팝업 후 확인 시 패배처리
    /// </summary>
    public void OnClickForfeitButton()
    {
        GameManager.Instance.OpenConfirmPanel("기권하시겠습니까?", () =>
        {
            StopClock();

            if (GameManager.Instance.bIsMultiplay) { }

            // GameManager에서 현재 GameLogic 가져옴
            var currentLogic = GameManager.Instance.GetGameLogic();

            if (GameManager.Instance.bIsMultiplay)
            {
                GameManager.Instance.OnSendForfeit?.Invoke();
            }
            else if (currentLogic != null)
            {
                // 기권 시 플레이어가 패배하므로 반대편이 승리합니다.
                Enums.EPlayerType winner = (currentLogic.localPlayerType == Enums.EPlayerType.Player_Black) ?
                    Enums.EPlayerType.Player_White : Enums.EPlayerType.Player_Black;
                currentLogic.EndGame(winner);
            }
            else
            {
                GameManager.Instance.ChangeToMainScene();
            }
        });
    }

    /// <summary>
    /// 착수 버튼 클릭 시 착수 처리하는 메서드
    /// </summary>
    public void OnClickBeginButton()
    {
        onBeginButtonClicked?.Invoke();
    }

    /// <summary>
    /// 패배함수를 액션에 등록하는 메서드
    /// </summary>
    public void SetTimeOutAction(Action action)
    {
        timer.GetComponent<Timer>().OnTimeOut -= action;
        timer.GetComponent<Timer>().OnTimeOut += action;
    }
    public void OnClickBirdImage()
    {
        AudioManager.Instance.PlaySfxSound(3);
    }
}
