using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using Random = UnityEngine.Random;

public static class MinimaxAIController
{
    private const int BOARD_SIZE = 14;      //Board의 size를 받아서 사용 (현재 임시로 상수 설정 후 사용 중)
    private static int mSearchRadius;       //착수된 돌들에서 몇칸 이내를 고려할 것인지
    private static int mMinimaxDepth;       //깊이 설정(몇 수 앞을 볼건지)
    private static float mMistakeChance;    //실수할 수 있는 확률
    private static int mConsiderationLimit; //고려할 수 있는 위치의 개수를 제한

    private static Dictionary<string, float> mTranspositionTable = new Dictionary<string, float>(); //보드 상태와 평가 점수를 저장하는 딕셔너리, 같은 패턴의 경우 저장된 값을 사용하여 속도 증가

    private static float timeLimit = 1.0f;  //시간 제한 1초

    /// <summary>
    /// AI 플레이 전 난이도를 설정하는 메서드
    /// </summary>
    /// <param name="level">난이도</param>
    public static void SetLevel(Enums.EDifficultyLevel level)
    {
        switch (level)
        {
            case Enums.EDifficultyLevel.Easy:
                mSearchRadius = 1;
                mMinimaxDepth = 2;
                mMistakeChance = 0.15f;
                mConsiderationLimit = 5;
                break;
            case Enums.EDifficultyLevel.Medium:
                mSearchRadius = 2;
                mMinimaxDepth = 3;
                mMistakeChance = 0.05f;
                mConsiderationLimit = 10;
                break;
            case Enums.EDifficultyLevel.Hard:
                mSearchRadius = 2;
                mMinimaxDepth = 4;
                mMistakeChance = 0.01f;
                mConsiderationLimit = 15;
                break;
        }
        
        mTranspositionTable.Clear(); //새로운 난이도 선택시(새 게임 시) 기존 저장된 값을 초기화
    }

    /// <summary>
    /// 현재 보드의 상태에서 가장 최적의 수를 찾는 메서드
    /// 실수 확률을 먼저 계산하고 아니라면 Minimax알고리즘을 통해 제한시간안에 최적의 수를 선택 후 반환
    /// </summary>
    /// <param name="board">현재 보드의 상태</param>
    /// <returns></returns>
    public static (int row, int col)? GetBestMove(Enums.EPlayerType[,] board)
    {
        (int row, int col) bestMove = (-1, -1);
        float startTime = Time.realtimeSinceStartup;
        
        //실수 확률 적용
        if (Random.value < mMistakeChance)
        {
            List<(int row, int col)> possibleMoves = GetAllValidMoves(board);
            if (possibleMoves.Count == 0) return null;
            return possibleMoves[Random.Range(0, possibleMoves.Count)];
        }
        
        //실수가 일어나지 않았다면
        for (int depth = 1; depth < mMinimaxDepth; depth++)
        {
            (int newRow, int newCol) = FindBestMoveAtDepth(board, depth);

            if (Time.realtimeSinceStartup - startTime < timeLimit) //시간이 남았으면 결과 업데이트
            {
                bestMove = (newRow, newCol);
            }
            else //시간 초과 경우
            {
                break;
            }
        }

        return bestMove != (-1, -1) ? bestMove : null; //초기값(-1, -1)인지 확인하고 아니라면 반환
    }

    /// <summary>
    /// 해당 depth에서의 최적인 수를 찾는 메서드
    /// 난이도에 따라 GetBestMove에서의 최대 depth가 달라짐
    /// DoMinimax로 해당 depth에서의 최고 점수를 가지는 최고의 수를 반환
    /// </summary>
    /// <param name="board">현재 보드의 상태</param>
    /// <param name="depth">Minimax에서 어느 깊이까지 볼건지</param>
    /// <returns></returns>
    private static (int row, int col) FindBestMoveAtDepth(Enums.EPlayerType[,] board, int depth)
    {
        float bestScore = float.MinValue;
        (int row, int col) bestMove = (-1, -1);

        List<(int row, int col)> possibleMoves = GetBestMoveCandidates(board);

        if (possibleMoves.Count == 0) return (-1, -1);

        foreach (var (row,col) in possibleMoves)
        {
            if(IsBlackForbiddenMove(board, row, col, Enums.EPlayerType.Player_Black)) continue; //만약 해당 위치가 흑돌이 금지되는 위치면 무시

            board[row, col] = Enums.EPlayerType.Player_White;
            float score = DoMinimax(board, 0, depth, float.MinValue, float.MaxValue, false);
            board[row, col] = Enums.EPlayerType.None;

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = (row, col);
            }
        }

        return bestMove;
    }

    /// <summary>
    /// 기존 Minimax 알고리즘에서 알파베타 가지치기를 추가하여 더이상 계산이 될 필요가 없는 항목들을 무시
    /// 게임이 종료되는 조건을 먼저 확인(점수 부여)한 뒤, 재귀함수로 깊이 탐색(설정된 최대 깊이까지 > 도달하면 평가)
    /// </summary>
    /// <param name="board">현재 오목보드 상태</param>
    /// <param name="currentDepth">현재 탐색중인 깊이(몇 수 앞까지 탐색했는지)</param>
    /// <param name="maxDepth">최대 탐색의 깊이 (난이도에따라 조정)</param>
    /// <param name="alpha">알파베타 가지치기용</param>
    /// <param name="beta">알파베타 가지치기용</param>
    /// <param name="isMaximizing">True > AI차례, False > 사용자 차례</param>
    /// <returns></returns>
    private static float DoMinimax(Enums.EPlayerType[,] board, int currentDepth, int maxDepth, float alpha, float beta,
        bool isMaximizing)
    {
        //게임 종료 조건을 확인 TODO: AI 흑,백 경우 분기
        if (CheckGameWin(Enums.EPlayerType.Player_White, board)) //백돌이 이기면
            return 1000 - currentDepth;
        if (CheckGameWin(Enums.EPlayerType.Player_Black, board))
            return -1000 + currentDepth;
        if (IsAllBlocksPlaced(board) || currentDepth >= maxDepth)
            return EvaluateBoard(board);

        //이미 확인한 보드 상태이면 여러번 평가 안하도록 캐싱
        string boardHash = GetBoardHash(board, isMaximizing);
        if (mTranspositionTable.ContainsKey(boardHash))
            return mTranspositionTable[boardHash];

        float bestScore = isMaximizing ? float.MinValue : float.MaxValue;
        List<(int row, int col)> possibleMoves = GetBestMoveCandidates(board);

        foreach (var (row, col) in possibleMoves)
        {
            Enums.EPlayerType currentPlayer = isMaximizing ? Enums.EPlayerType.Player_White : Enums.EPlayerType.Player_Black;
            if(currentPlayer == Enums.EPlayerType.Player_Black && IsBlackForbiddenMove(board, row, col, currentPlayer)) continue; //흑돌이 금지된 위치면 무시

            board[row, col] = currentPlayer;
            float score = DoMinimax(board, currentDepth + 1, maxDepth, alpha, beta, !isMaximizing);
            board[row, col] = Enums.EPlayerType.None;

            if (isMaximizing) //최댓값, 최솟값 구하는것에 따라 score업데이트 및 알파/베타 값 업데이트
            {
                bestScore = Mathf.Max(bestScore, score);
                alpha = Mathf.Max(alpha, bestScore);
            }
            else
            {
                bestScore = Mathf.Min(bestScore, score);
                beta = Mathf.Min(beta, bestScore);
            }

            if (beta <= alpha) //알파베타 가지치기
                break;
        }

        mTranspositionTable[boardHash] = bestScore;
        return bestScore;

    }

    /// <summary>
    /// 현재 보드 상태를 문자열 형태의 해시로 변환하여 중복 탐색 방지
    /// </summary>
    /// <param name="board">현재 오목 보드 상태</param>
    /// <param name="isMaximizing">true > AI, false > 플레이어</param>
    /// <returns></returns>
    //0 0 1 0 0
    //0 2 1 0 0
    //1 0 2 1 0
    //0 0 0 2 0
    //0 0 0 0 1   0 : 빈칸, 1 : 흑돌, 2: 백돌  일때 >> "001000211002100200001" 문자열로 저장하고 현재 누구 턴인지도 추가
    private static string GetBoardHash(Enums.EPlayerType[,] board, bool isMaximizing)
    {
        StringBuilder hash = new StringBuilder(); //보드 상태를 하나의 문자열로 변환하는 역할
        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                hash.Append((int)board[row, col]);
            }
        }
        hash.Append(isMaximizing ? "1" : "0");
        return hash.ToString();
    }

    /// <summary>
    /// AI가 둘 수 있는 후보 위치를 선정하고 우선순위에 따라 정렬하여 반환
    /// 착수 가능한 빈칸만 탐색 > 중요도를 점수로 계산 > 점수가 높은순서대로 정렬
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <returns></returns>
    private static List<(int row, int col)> GetBestMoveCandidates(Enums.EPlayerType[,] board)
    {
        List<(int row, int col, float score)> scoredMoves = new List<(int row, int col, float score)>();

        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                if (board[row, col] != Enums.EPlayerType.None) //보드에 놓여져 있는 모든 돌에서 mSearchRadius의 +-만큼 다시 순회하고 이때 15x15 보드에 넘어가지 않는 것들 중에서
                                                               //이미 처리했던 곳이 아니고, 렌주룰에 해당되지 않으면 점수부여
                {
                    for (int rowOffset = -mSearchRadius; rowOffset <= mSearchRadius; rowOffset++)
                    {
                        for (int colOffset = -mSearchRadius; colOffset <= mSearchRadius; colOffset++)
                        {
                            int newRow = row + rowOffset;
                            int newCol = col + colOffset;

                            if (newRow >= 0 && newRow < BOARD_SIZE && newCol >= 0 && newCol < BOARD_SIZE &&
                                board[newRow, newCol] == Enums.EPlayerType.None)
                            {
                                bool alreadyAdded = scoredMoves.Any(m => m.row == newRow && m.col == newCol);
                                if (!alreadyAdded)
                                {
                                    if (IsBlackForbiddenMove(board, newRow, newCol, Enums.EPlayerType.Player_Black))
                                        continue;

                                    float score = SimpleEvaluate(board, newRow, newCol); //모든 경우의 수를 고려하지 않고 중요한 경우만 빠르게 선택하게끔 휴리스틱함수 사용
                                    scoredMoves.Add((newRow, newCol, score));
                                }
                            }
                        }
                    }
                }
            }
            
        }

        //저장한 리스트에서 점수를 높은 순으로 정렬 후, 고려하는 제한까지 자르고 반환
        int moveToConsider = Mathf.Min(mConsiderationLimit, scoredMoves.Count);
        return scoredMoves.OrderByDescending(m => m.score).Take(moveToConsider).Select(m => (m.row, m.col)).ToList();
    }

    /// <summary>
    /// AI가 실수를 했을 때 착수가 가능한 위치를 반환하는 메서드
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <returns></returns>
    private static List<(int row, int col)> GetAllValidMoves(Enums.EPlayerType[,] board)
    {
        List<(int row, int col)> moves = new List<(int row, int col)>();

        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                if (board[row, col] != Enums.EPlayerType.None)
                {
                    for (int rowOffset = -mSearchRadius; rowOffset <= mSearchRadius; rowOffset++)
                    {
                        for (int colOffset = -mSearchRadius; colOffset <= mSearchRadius; colOffset++)
                        {
                            int newRow = row + rowOffset;
                            int newCol = col + colOffset;

                            if (newRow >= 0 && newRow < BOARD_SIZE && newCol >= 0 && newCol < BOARD_SIZE &&
                                board[newRow, newCol] == Enums.EPlayerType.None && !moves.Contains((newRow, newCol)))
                            {
                                if(IsBlackForbiddenMove(board, newRow, newCol, Enums.EPlayerType.Player_Black))
                                    continue;
                                moves.Add((newRow, newCol));
                            }
                        }
                    }
                }
            }
        }

        return moves;
    }

    /// <summary>
    /// AI가 특정 좌표에 돌을 놓았을 때 그 위치의 가치를 평가하는 간단한 휴리스틱 함수
    /// </summary>
    /// <param name="board">현재 오목 보드 상태</param>
    /// <param name="row">평가할 좌표</param>
    /// <param name="col">평가할 좌표</param>
    /// <returns></returns>
    private static float SimpleEvaluate(Enums.EPlayerType[,] board, int row, int col)
    {
        float score = 0;
        
        //공격
        board[row, col] = Enums.EPlayerType.Player_White;
        score += GetPatternScore(board, row, col, Enums.EPlayerType.Player_White) * 1.1f;
        
        //방어
        board[row, col] = Enums.EPlayerType.Player_Black;
        score += GetPatternScore(board, row, col, Enums.EPlayerType.Player_Black);

        //되돌리기
        board[row, col] = Enums.EPlayerType.None;

        return score;
    }

    /// <summary>
    /// 현재 보드 상태에서 AI가 얼마나 유리한지 점수로 반환
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <returns></returns>
    private static float EvaluateBoard(Enums.EPlayerType[,] board)
    {
        float score = 0;
        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                if (board[row, col] == Enums.EPlayerType.Player_White)
                    score += GetPatternScore(board, row, col, Enums.EPlayerType.Player_White);
                else if (board[row, col] == Enums.EPlayerType.Player_Black)
                    score -= GetPatternScore(board, row, col, Enums.EPlayerType.Player_Black);
            }
        }
        return score;
    }

    /// <summary>
    /// 특정 위치에서 주어진 player의 돌이 놓였을 때, 해당 방향으로의 패턴을 평가하고 점수 부여 및 반환
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <param name="row">평가할 좌표</param>
    /// <param name="col">평가할 좌표</param>
    /// <param name="player">평가할 플레이어 A or B</param>
    /// <returns></returns>
    private static float GetPatternScore(Enums.EPlayerType[,] board, int row, int col, Enums.EPlayerType player)
    {
        float score = 0;
        int[] directionX = { 1, 0, 1, -1 };
        int[] directionY = { 0, 1, 1, 1 };

        for (int direction = 0; direction < 4; direction++)
        {
            StringBuilder pattern = new StringBuilder();
            List<Enums.EPlayerType> linePattern = new List<Enums.EPlayerType>();

            for (int i = -4; i <= 4; i++)
            {
                int newRow = row + i * directionX[direction];
                int newCol = col + i * directionY[direction];

                if (newRow >= 0 && newRow < BOARD_SIZE && newCol >= 0 && newCol < BOARD_SIZE)
                {
                    linePattern.Add(board[newRow, newCol]);
                    if (board[newRow, newCol] == player)
                        pattern.Append('X');
                    else if (board[newRow, newCol] == Enums.EPlayerType.None)
                        pattern.Append('.');
                    else
                        pattern.Append('O');
                }
            }

            string patternStr = pattern.ToString();
           

            if (patternStr.Contains("XXXXX")) score += 1000;
            else if (patternStr.Contains(".XXXX.")) score += 200; // 양쪽 열린 4
            else if (patternStr.Contains("XXXX.") || patternStr.Contains(".XXXX")) score += 100; // 한쪽 열린 4
            else if (patternStr.Contains(".XXX.")) score += 50; // 양쪽 열린 3
            else if (patternStr.Contains("XXX..") || patternStr.Contains("..XXX")) score += 10; // 한쪽 열린 3
            else if (patternStr.Contains("XX...") || patternStr.Contains("...XX")) score += 5; // 두 개 연속
            else if (patternStr.Contains(".XX..") || patternStr.Contains("..XX.")) score += 8; // 양쪽 열린 2
        }

        return score;
    }

    /// <summary>
    /// 보드 판에 빈칸이 있는지 확인하는 메서드 true 꽉참, false 둘 곳이 있음
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    private static bool IsAllBlocksPlaced(Enums.EPlayerType[,] board)
    {
        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col <BOARD_SIZE; col++)
            {
                if (board[row, col] == Enums.EPlayerType.None)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 보드에서 특정 플레이어가 승리조건을 만족하는지 확인하는 함수(흑돌의 경우 장목이 승리가 되지 않도록)
    /// 승리시 true, 패배시 false 반환
    /// </summary>
    /// <param name="playerType">검사할 플레이어</param>
    /// <param name="board">현재 보드 상태</param>
    /// <returns></returns>
    public static bool CheckGameWin(Enums.EPlayerType playerType, Enums.EPlayerType[,] board)
    {
        bool isBlack = playerType == Enums.EPlayerType.Player_Black;

        int[] directionX = { 1, 0, 1, -1 };
        int[] directionY = { 0, 1, 1, 1 };

        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                if(board[row, col] != playerType) continue;

                for (int direction = 0; direction < 4; direction++)
                {
                    int count = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        int newRow = row + i * directionX[direction];
                        int newCol = col + i * directionY[direction];

                        if (newRow >= 0 && newRow < BOARD_SIZE && newCol >= 0 && newCol < BOARD_SIZE &&
                            board[newRow, newCol] == playerType)
                        {
                            count++;
                        }
                    }

                    if (isBlack)
                    {
                        if (count == 5)
                        {
                            //앞 방향 체크
                            int frontRow = row + 5 * directionX[direction];
                            int frontCol = col + 5 * directionY[direction];
                            bool hasFrontStone = frontRow >= 0 && frontRow < BOARD_SIZE && frontCol >= 0 &&
                                                 frontCol < BOARD_SIZE && board[frontRow, frontCol] == playerType;
                            
                            // 뒤 방향 체크
                            int backRow = row - directionX[direction];
                            int backCol = col - directionY[direction];
                            bool hasBackStone = (backRow >= 0 && backRow < BOARD_SIZE && backCol >= 0 && backCol < BOARD_SIZE &&
                                                 board[backRow, backCol] == playerType);
                                                
                            // 장목(6목 이상)이 아닌 경우만 승리
                            if (!hasFrontStone && !hasBackStone)
                                return true;
                        }
                    }
                    else if (count >= 5)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    #region RenjuRule

    private static bool IsBlackForbiddenMove(Enums.EPlayerType[,] board, int row, int col, 
        Enums.EPlayerType playerType)
    {
        if (playerType != Enums.EPlayerType.Player_Black) return false;

        board[row, col] = playerType;

        bool isThreeThree = CheckThreeThree(board, row, col, playerType);
        bool isFourFour = CheckFourFour(board, row, col, playerType);
        bool isOverline = CheckOverline(board, row, col, playerType);
        board[row, col] = Enums.EPlayerType.None;

        return isThreeThree || isFourFour || isOverline;
    }

    // 3-3 금수 체크
    private static bool CheckThreeThree(Enums.EPlayerType[,] board, int row, int col, Enums.EPlayerType playerType)
    {
        int[] dx = { 1, 0, 1, -1 };
        int[] dy = { 0, 1, 1, 1 };
        
        int openThreeCount = 0;
        
        for (int d = 0; d < 4; d++)
        {
            // 양방향 체크 (정방향, 역방향)
            for (int dir = -1; dir <= 1; dir += 2)
            {
                // 돌이 3개 연속으로 있는지 체크
                int count = 1; // 현재 위치 포함
                for (int i = 1; i <= 3; i++)
                {
                    int newRow = row + i * dir * dx[d];
                    int newCol = col + i * dir * dy[d];
                    
                    if (newRow >= 0 && newRow < BOARD_SIZE && newCol >= 0 && newCol < BOARD_SIZE &&
                        board[newRow, newCol] == playerType)
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                
                // 3개 돌이 연속으로 있고 양 끝이 비어있는지 체크
                if (count == 3)
                {
                    // 양쪽 끝 체크
                    int endRow1 = row + 3 * dir * dx[d];
                    int endCol1 = col + 3 * dir * dy[d];
                    int endRow2 = row - dir * dx[d];
                    int endCol2 = col - dir * dy[d];
                    
                    bool end1Empty = (endRow1 >= 0 && endRow1 < BOARD_SIZE && endCol1 >= 0 && endCol1 < BOARD_SIZE &&
                                     board[endRow1, endCol1] == Enums.EPlayerType.None);
                    bool end2Empty = (endRow2 >= 0 && endRow2 < BOARD_SIZE && endCol2 >= 0 && endCol2 < BOARD_SIZE &&
                                     board[endRow2, endCol2] == Enums.EPlayerType.None);
                    
                    if (end1Empty && end2Empty)
                    {
                        openThreeCount++;
                    }
                }
            }
        }
        
        // 3-3 금수: 양쪽이 열린 3이 2개 이상
        return openThreeCount >= 2;
    }
    
    // 4-4 금수 체크
    private static bool CheckFourFour(Enums.EPlayerType[,] board, int row, int col, Enums.EPlayerType playerType)
    {
        int[] dx = { 1, 0, 1, -1 };
        int[] dy = { 0, 1, 1, 1 };
        
        int openFourCount = 0;
        
        for (int d = 0; d < 4; d++)
        {
            // 양방향 체크 (정방향, 역방향)
            for (int dir = -1; dir <= 1; dir += 2)
            {
                // 돌이 4개 연속으로 있는지 체크
                int count = 1; // 현재 위치 포함
                for (int i = 1; i <= 4; i++)
                {
                    int newRow = row + i * dir * dx[d];
                    int newCol = col + i * dir * dy[d];
                    
                    if (newRow >= 0 && newRow < BOARD_SIZE && newCol >= 0 && newCol < BOARD_SIZE &&
                        board[newRow, newCol] == playerType)
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                
                // 4개 돌이 연속으로 있고 한쪽 끝이 비어있는지 체크
                if (count == 4)
                {
                    // 열린 부분 체크
                    int endRow = row + 4 * dir * dx[d];
                    int endCol = col + 4 * dir * dy[d];
                    
                    bool endEmpty = (endRow >= 0 && endRow < BOARD_SIZE && endCol >= 0 && endCol < BOARD_SIZE &&
                                    board[endRow, endCol] == Enums.EPlayerType.None);
                    
                    if (endEmpty)
                    {
                        openFourCount++;
                    }
                }
            }
        }
        
        // 4-4 금수: 열린 4가 2개 이상
        return openFourCount >= 2;
    }
    
    // 장목(6목 이상) 체크
    private static bool CheckOverline(Enums.EPlayerType[,] board, int row, int col, Enums.EPlayerType playerType)
    {
        int[] dx = { 1, 0, 1, -1 };
        int[] dy = { 0, 1, 1, 1 };
        
        for (int d = 0; d < 4; d++)
        {
            int count = 1; // 현재 위치 포함
            
            // 양방향으로 연속된 돌 체크
            for (int dir = -1; dir <= 1; dir += 2)
            {
                for (int i = 1; i < 6; i++)
                {
                    int newRow = row + i * dir * dx[d];
                    int newCol = col + i * dir * dy[d];
                    
                    if (newRow >= 0 && newRow < BOARD_SIZE && newCol >= 0 && newCol < BOARD_SIZE &&
                        board[newRow, newCol] == playerType)
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            
            // 6목 이상이면 장목
            if (count >= 6)
            {
                return true;
            }
        }
        
        return false;
    }
    

    #endregion
}
