using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임에서 공용으로 사용되는 열거형(Enum) 정의 클래스.
/// </summary>
public class Enums
{
    public enum EMultiplayManagerState
    {
        CreateRoom,     // 방 생성
        JoinRoom,       // 생성된 방에 참여
        StartGame,      // 생성된 방에 다른 유저가 참여해서 게임 시작
        ExitRoom,       // 자신이 방을 빠져 나왔을 때
        EndGame,         // 상대방이 접속을 끊거나 방을 나갔을 때
        RestartRoom,
    };
    
    public enum EPlayerType 
    { 
        None,           // 텅 빈 상태
        Player_Black,        // 흑돌
        Player_White         // 백돌
    }

    public enum EGameType
    {
        PassAndPlay,    // 1개 폰 2인 플레이
        SinglePlay,     // 싱글 플레이
        MultiPlay,       // 멀티 플레이
        PassAndPlayFade,
    }

    public enum EGameResult
    {
        None,           // 게임 진행 중
        Win,            // 플레이어 승
        Lose,           // 플레이어 패
        Draw,           // 무승부
    }
    
    public enum EItemType
    {
        NoAds,          //광고 제거
        NoAds_Coin_2000, //광고제거 + 코인 2000개
        Coin_1000,       //코인 1000개
        Coin_2000,       //코인 2000개
        Coin_4500,      //코인 4500개
        Coin_10000,      //코인 10000개
    }
    
    public enum EDifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    public enum EGameUIState
    {
        Turn_Black,
        Turn_White,
    }

    public enum EGameImage
    {
        BlackStone,
        WhiteStone,
        Selector,
        XMarker,
        LastPosMark,
        None,
    }

    public enum EEasterEggMode
    {
        None,
        FadeStone,
    }
}

namespace AudioEnums
{
    public enum ESfxType
    {
        //Bgm
        IntroBGM,
        GameBGM,
        
        //SFX
        Tic,
        Bird,
        Click,
        Coin,
        Coins,
        BranchRustle
    }
}