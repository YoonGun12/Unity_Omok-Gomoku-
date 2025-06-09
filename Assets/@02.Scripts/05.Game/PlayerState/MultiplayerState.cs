using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerState : BasePlayerState
{
    private MultiplayManager mMultiplayManager;
    
    public MultiplayerState(bool Black, MultiplayManager multiplayManager)
    {
        if (Black)
        {
            playerType = Enums.EPlayerType.Player_Black;
        }
        else
        {
            playerType = Enums.EPlayerType.Player_White;
        }
        
        mMultiplayManager = multiplayManager;
    }
    
    public override void OnEnter(GameLogic gameLogic)
    {
        mMultiplayManager.OnOpponentMove = moveData =>
        {
            int Y = moveData.position / 15;
            int X = moveData.position % 15;
            UnityThread.executeInUpdate(() =>
            {
                HandleMove(gameLogic, Y, X);
            });
        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        mMultiplayManager.OnOpponentMove = null;
    }

    public override void HandleMove(GameLogic gameLogic, int y, int x)
    {
        if (gameLogic.isGameOver) return;
        ProcessMove(gameLogic, playerType, y, x);
    }
}
