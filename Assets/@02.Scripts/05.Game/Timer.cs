using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private Image fillImage;       // 채워지는 부분
    [SerializeField] private Image headCapImage;    // fillImage의 머리부분
    [SerializeField] private Image tailCapImage;    // fillImage의 꼬리부분
    [SerializeField] private TMP_Text timerText;    // 시간 표시
    [SerializeField] private float timeLimit;       // 시간 제한 설정(30초?)
    
    private float mCurrentTime;                     // 시간 측정을 위한 변수
    private bool mbIsPaused;                        // 시간 정지 여부

    public Action OnTimeOut;                       // 시간이 다 되면 호출할 콜백                 
    
    private void Awake()
    {
        mbIsPaused = true;
    }

    private void Update()
    {
        if (!mbIsPaused)
        {
            mCurrentTime += Time.deltaTime;
            if (mCurrentTime >= timeLimit)              // 시간이 다 돼면 이미지를 숨기고, 시간 정지 후 콜백 실행
            {
                headCapImage.gameObject.SetActive(false);
                tailCapImage.gameObject.SetActive(false);
                
                PauseTimer();
                OnTimeOut?.Invoke();
            }
            else                                        // 시간의 흐름에 따라 fillImage를 진행 상태에 맞게 채움
            {
                float rotationAngle = fillImage.fillAmount * - 360.0f;
                fillImage.fillAmount = (timeLimit - mCurrentTime) / timeLimit;
                headCapImage.transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationAngle);
                
                // 남은 시간 표시(소수점 없이)
                float timeTextTime = timeLimit - mCurrentTime;
                timerText.text = timeTextTime.ToString("F0");
            }
        }
    }

    public void StartTimer()
    {
        AudioManager.Instance.PlayGameBgm();
        mbIsPaused = false;
    }

    public void PauseTimer()
    {
        AudioManager.Instance.StopBgm();
        mbIsPaused = true;
    }

    public void InitTimer()
    {
        PauseTimer();
        mCurrentTime = 0;
        fillImage.fillAmount = 1;
        timerText.text = timeLimit.ToString("F0");
    }
}
