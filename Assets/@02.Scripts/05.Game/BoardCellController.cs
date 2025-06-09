using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Serialization;

public class BoardCellController : MonoBehaviour
{
    public BoardCell[,] cells;
    public delegate void OnCellClicked(int cellIndex);
    public OnCellClicked onCellClicked;
    public int size = 14;
    
    private RectTransform mGrid;
    [SerializeField] private GameObject mCellPrefab;

    private readonly string mGirdStr = "Grid";

    private void Awake()
    {
        mGrid = Util.GetChildComponent<RectTransform>(gameObject, mGirdStr);
    }
    
    /// <summary>
    /// 보드를 생성 및 초기화 메서드
    /// </summary>
    public void InitBoard()
    {
        if (cells != null)
        {
            for (int i = 0; i < size + 1; i++)
            {
                for (int j = 0; j < size + 1; j++)
                {
                    cells[i, j].ResetCell();
                }
            }
        }
        cells = new BoardCell[size + 1, size + 1];
        
        //그리드 총 길이
        float gridWidth = mGrid.rect.width;
        float gridHeight = mGrid.rect.height;
        
        //한칸 길이
        float cellWSize = gridWidth / size;
        float cellHSize = gridHeight / size;
        
        //왼쪽하단
        float originX = gridWidth * -0.5f;
        float originY = gridHeight * -0.5f;

        int blockCount = 0;
        
        for (int y = 0; y < size + 1; y++)
        {
            for (int x = 0; x < size + 1; x++)
            {
                GameObject cell = Instantiate(mCellPrefab, mGrid.transform);
                
                //위치 크기 정렬
                RectTransform cellRect = cell.GetComponent<RectTransform>();
                float posX = originX + (gridWidth / size) * x;
                float posY = originY + (gridHeight / size) * y;
        
                cellRect.localPosition = new Vector2(posX, posY);
                cellRect.sizeDelta = new Vector2(cellWSize, cellHSize);
                
                //셀 초기화
                BoardCell boardCell = cell.GetComponent<BoardCell>();
                cells[y, x] = boardCell;
                boardCell.InitBlockCell(blockCount++, (blockIndex) =>
                {
                    onCellClicked?.Invoke(blockIndex);
                });
            }
        }
    }
    
}
