using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사용자 정보를 저장하고 관리하는 클래스.
/// PlayerPrefs를 활용하여 데이터를 저장하고 불러온다.
/// </summary>
public class UserInformations
{
    public static bool IsAutoSignin
    {
        get { return PlayerPrefs.GetInt("IsAutoSignin", 1) == 1; }
        set { PlayerPrefs.SetInt("IsAutoSignin", value ? 1 : 0); PlayerPrefs.Save(); }
    }
}