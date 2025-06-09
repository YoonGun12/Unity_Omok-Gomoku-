using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePlayerState
{
    public Enums.EPlayerType playerType;
    public Enums.EEasterEggMode easterEggMode;
    
    public delegate void OnForbbidenMark(bool onMark);
    public OnForbbidenMark onForbbidenMark;
    
    public delegate void OnMode(BasePlayerState player);
    public OnMode onMode;

    public abstract void OnEnter(GameLogic gameLogic);
    public abstract void OnExit(GameLogic gameLogic);
    public abstract void HandleMove(GameLogic gameLogic,int y, int x);

    protected void ProcessMove(GameLogic gameLogic, Enums.EPlayerType player, int Y, int X)
    {
        if (gameLogic.SetStone(player, Y, X))
        {

            if (easterEggMode == Enums.EEasterEggMode.FadeStone)
            {
                onMode += gameLogic.boardCellController.cells[Y,X].FadeMode;
            }
            
            bool gameResult = gameLogic.GameResult(player, Y, X);
            if (gameResult)
            {
                gameLogic.EndGame(player);
            }
            else
            {
                gameLogic.NextTurn(player);
            }
        }
    }
}
