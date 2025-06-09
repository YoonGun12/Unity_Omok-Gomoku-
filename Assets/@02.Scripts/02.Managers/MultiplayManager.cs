using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SocketIOClient;
using UnityEngine;

public class RoomData
{
    [JsonProperty("roomId")]
    public string roomId { get; set; }
}

public class MoveData
{
    [JsonProperty("position")]
    public int position { get; set; }
}

public class UsersInfoData
{
    [JsonProperty("userId")]
    public string userId { get; set; }
    
    [JsonProperty("roomId")]
    public string roomId { get; set; }
    
    [JsonProperty("nickname")]
    public string nickname { get; set; }
    
    [JsonProperty("profileimageindex")]
    public int profileimageindex { get; set; }
    
    [JsonProperty("rank")]
    public int rank { get; set; }
    
    [JsonProperty("playerType")]
    public Enums.EPlayerType playerType { get; set; }
}

public class MultiplayManager : IDisposable
{
    private SocketIOUnity mSocket;
    
    private event Action<Enums.EMultiplayManagerState, string> mOnMultiplayStateChange;
    public Action<MoveData> OnOpponentMove;
    public Action<UsersInfoData> OnOpponentProfileUpdate;
    public Action OnRematchRequestReceived;
    
    public MultiplayManager(Action<Enums.EMultiplayManagerState, string> onMultiplayStateChange)
    {
        mOnMultiplayStateChange = onMultiplayStateChange;
        
        var uri = new Uri(Constants.GameSeverURL);
        mSocket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        mSocket.OnConnected += (sender, e) => {
            Debug.Log("[MultiplayManager] 소켓 연결 성공!");
        };
        mSocket.On("createRoom", CreateRoom);
        mSocket.On("joinRoom", JoinRoom);
        mSocket.On("startGame", StartGame);
        mSocket.On("exitRoom", ExitRoom);
        mSocket.On("endGame", EndGame);
        mSocket.On("restartRoom", RestartRoom);
        
        mSocket.On("doOpponent", DoOpponent);
        mSocket.On("opponentProfile", OpponentProfileReceived);
        
        // 재대국 관련 이벤트 핸들러 추가
        mSocket.On("rematchRequestReceived", RematchRequestReceived);
        mSocket.On("rematchFailed", RematchFailed);
        mSocket.On("rematchAcceptedReceived", AcceptRematchReceived);
        mSocket.On("rematchRejectedReceived", RejectedRematchReceived);
        
        mSocket.On("forfeitWinReceived", ForfeitWinReceived);
        mSocket.On("forfeitLoseReceived", ForfeitLoseReceived);
        
        mSocket.Connect();
    }
    
    // 자신이 방(세션)을 생성
    private void CreateRoom(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        mOnMultiplayStateChange?.Invoke(Enums.EMultiplayManagerState.CreateRoom, data.roomId);
    }
    
    // 상대방이 생성한 방(세션)에 참가
    private void JoinRoom(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        mOnMultiplayStateChange?.Invoke(Enums.EMultiplayManagerState.JoinRoom, data.roomId);
    }
    
    // 생성된 방에 상대방이 참가 했을 때 게임 시작
    private void StartGame(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        mOnMultiplayStateChange?.Invoke(Enums.EMultiplayManagerState.StartGame, data.roomId);
    }

    // 자신이 방에서 나갔을 때
    private void ExitRoom(SocketIOResponse response)
    {
        mOnMultiplayStateChange?.Invoke(Enums.EMultiplayManagerState.ExitRoom, null);
    }
    
    // 상대방이 방에서 나갔을 때
    private void EndGame(SocketIOResponse response)
    {
        mOnMultiplayStateChange?.Invoke(Enums.EMultiplayManagerState.EndGame, null);
    }

    private void RestartRoom(SocketIOResponse response)
    {
        mOnMultiplayStateChange?.Invoke(Enums.EMultiplayManagerState.RestartRoom, null);
    }
    
    public void LeaveRoom(string roomId)
    {
        mSocket.Emit("leaveRoom", new { roomId });
    }
    
    #region ProfileData
    
    // 나의 급수를 서버로 전달
    public void SendMyRank(int myRank)
    {
        Debug.Log($"[MultiplayManager] SendMyRank 호출, rank={myRank}");
        mSocket.Emit("setRank", new { myRank });
    }
    
    // 상대방의 프로필 정보를 서버로부터 수신
    private void OpponentProfileReceived(SocketIOResponse response)
    {
        try
        {
            var data = response.GetValue<UsersInfoData>();
            OnOpponentProfileUpdate?.Invoke(data); // 프로필 정보를 UI로 전달
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in OnOpponentProfileReceived: {ex.Message}");
        }
    }

    // 자신의 프로필 정보를 서버로 송신
    public void SendOpponentProfile(string roomId, UsersInfoData profileData)
    {
        var data = new 
        {
            roomId,
            userId = profileData.userId,
            nickname = profileData.nickname,
            profileimageindex = profileData.profileimageindex,
            rank = profileData.rank,
            playerType = profileData.playerType
        };
        
        mSocket.Emit("opponentProfile", data);
    }
    #endregion
    
    #region MoveData
    
    // 서버로부터 상대방의 마커 정보를 받기 위한 메서드
    private void DoOpponent(SocketIOResponse response)
    {
        try
        {
            var data = response.GetValue<MoveData>();
            OnOpponentMove?.Invoke(data);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in DoOpponent: {ex.Message}");
        }
    }
    
    // 플레이어의 마커 위치를 서버로 전달하기 위한 메서드
    public void SendPlayerMove(string roomId, int position)
    {
        mSocket.Emit("doPlayer", new { roomId , position });
    }
    
    #endregion

    #region RematchData

    // 재대국 요청을 서버에 보냄
    public void SendRematchRequest(string roomId)
    {
        Debug.Log("재대국 요청 보냄");
        mSocket.Emit("sendRematchRequest", new { roomId });
    }

    // 서버로부터 재대국 요청을 받았을 때 처리
    private void RematchRequestReceived(SocketIOResponse response)
    {
        Debug.Log("재대국 요청 받음");
        OnRematchRequestReceived?.Invoke();
    }

    private void RematchFailed(SocketIOResponse response)
    {
        UnityThread.executeInUpdate(() =>
        {
            GameManager.Instance.OpenConfirmPanel("상대방이 퇴장하였습니다. \n코인을 돌려받고 \n메인 화면으로 돌아갑니다.", () =>
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
            });
        });
    }

    // 재대국 요청 승낙
    public void AcceptRematch(string roomId)
    {
        mSocket.Emit("rematchAccepted", new { roomId });
    }

    private void AcceptRematchReceived(SocketIOResponse response)
    {
        mSocket.Emit("startRematch");
    }

    // 재대국 요청 거절
    public void RejectRematch()
    {
        mSocket.Emit("rematchRejected");
        UnityThread.executeInUpdate(() =>
        {
            GameManager.Instance.OpenConfirmPanel("상대방의 요청을 거절했습니다. \n메인 화면으로 돌아갑니다.", () =>
            {
                GameManager.Instance.ChangeToMainScene();
            }, false);
        });
    }
    
    private void RejectedRematchReceived(SocketIOResponse response)
    {
        UnityThread.executeInUpdate(() =>
        {
            GameManager.Instance.OpenConfirmPanel("상대방이 거절했습니다. \n코인을 돌려받고 \n메인 화면으로 돌아갑니다.", () =>
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

    #endregion

    #region ForfeitData

    public void SendForfeitRequest(string roomId)
    {
        mSocket.Emit("sendForfeitRequest", new { roomId });
    }

    private void ForfeitWinReceived(SocketIOResponse response)
    {
        Debug.Log("기권승리 메시지 받음");
        GameManager.Instance.OnForfeitWin?.Invoke();
    }

    private void ForfeitLoseReceived(SocketIOResponse response)
    {
        Debug.Log("기권패배 메시지 받음");
        GameManager.Instance.OnForfeitLose?.Invoke();
    }

    #endregion
    
    public void Dispose()
    {
        if (mSocket != null)
        {
            mSocket.Disconnect();
            mSocket.Dispose();
            mSocket = null;
        }
    }
}