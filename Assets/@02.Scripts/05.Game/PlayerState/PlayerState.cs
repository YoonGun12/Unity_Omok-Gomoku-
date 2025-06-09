using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerState : BasePlayerState
{
    private MultiplayManager mMultiplayManager;
    private string mRoomId;
    private bool mbIsMultiplay;
    private int size;
    
    public PlayerState(bool Black)
    {
        if (Black)
        {
            playerType = Enums.EPlayerType.Player_Black;
        }
        else
        {
            playerType = Enums.EPlayerType.Player_White;
        }
        
        mbIsMultiplay = false;
    }
    
    public PlayerState(bool Black,Enums.EEasterEggMode mode)
    {
        if (Black)
        {
            playerType = Enums.EPlayerType.Player_Black;
        }
        else
        {
            playerType = Enums.EPlayerType.Player_White;
        }
        
        mbIsMultiplay = false;

        switch (mode)
        {
            case Enums.EEasterEggMode.None:
                break;
            case Enums.EEasterEggMode.FadeStone:
                easterEggMode = Enums.EEasterEggMode.FadeStone;
                break;
        }
    }

    public PlayerState(bool Black, MultiplayManager multiplayManager, string roomId) : this(Black)
    {
        mMultiplayManager = multiplayManager;
        mRoomId = roomId;
        mbIsMultiplay = true;
    }
    

    /// <summary>
    /// 자신의 턴이 되었을 때 착수가능하게 버튼을 활성화
    /// </summary>
    /// <param name="gameLogic"></param>
    public override void OnEnter(GameLogic gameLogic)
    {
        size = gameLogic.boardCellController.size;
        
        //금수 위치의 셀들의 이미지 활성화
        onForbbidenMark?.Invoke(true);
        
        //셀이 눌렸을 때 : 셀 선택
        gameLogic.boardCellController.onCellClicked = (cellIndex) =>
        {
            int X = cellIndex % (size + 1);
            int Y = cellIndex / (size + 1);
            BoardCell cell = gameLogic.boardCellController.cells[Y, X];
            if (cell.IsForbidden != true && cell.playerType == Enums.EPlayerType.None)
            {
                SelectCell(cellIndex, gameLogic);
                gameLogic.currentSelectedCell = cellIndex;
            }
        };
        
        //착수 버튼을 눌렀을 때 : 선택된 셀에 착수
        gameLogic.gamePanelController.onBeginButtonClicked = () =>
        {
            int cellIndex = gameLogic.currentSelectedCell;
            if (cellIndex == Int32.MaxValue) return;
            
            int X = cellIndex % (size + 1);
            int Y = cellIndex / (size + 1);
            
            HandleMove(gameLogic, Y, X);
            
            cellIndex = Int32.MaxValue;
        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        //델리게이트 초기화
        gameLogic.gamePanelController.onBeginButtonClicked = null;
        gameLogic.boardCellController.onCellClicked  = null;
        onForbbidenMark?.Invoke(false);

        if (easterEggMode != Enums.EEasterEggMode.None)
        {
            onMode?.Invoke(this);
        }
    }

    public override void HandleMove(GameLogic gameLogic, int Y, int X)
    {
        ProcessMove(gameLogic, playerType, Y, X);

        if (mbIsMultiplay)
        {
            mMultiplayManager.SendPlayerMove(mRoomId, Y *  15 + X);
        }
    }

    public void SelectCell(int newCell,GameLogic gameLogic)
    {
        int prevCell = gameLogic.currentSelectedCell;
        if (prevCell != Int32.MaxValue)
        {
            int prevX = prevCell % (size + 1);
            int prevY = prevCell / (size + 1);

            gameLogic.boardCellController.cells[prevY, prevX].SelectMark(false);
        }

        int newX = newCell % (size + 1);
        int newY = newCell / (size + 1);
        gameLogic.boardCellController.cells[newY,newX].SelectMark(true);
    }
}
