using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UserDataStructs;


public class SigninPanelController : PopupPanelController
{
    [SerializeField] private TMP_InputField mUsernameInputField;
    [SerializeField] private TMP_InputField mPasswordInputField;
    [SerializeField] private Image mAutoSigninButtonCheckImage;

    private Action OnSigninButtonClick;

    public void Show(Action onSigninButtonClick)
    {
        base.Show();
        
        OnSigninButtonClick = onSigninButtonClick;
        mAutoSigninButtonCheckImage.gameObject.SetActive(UserInformations.IsAutoSignin);
    }

    public void OnClickAutoSigninButton()
    {
        UserInformations.IsAutoSignin  = !UserInformations.IsAutoSignin;
        mAutoSigninButtonCheckImage.gameObject.SetActive(UserInformations.IsAutoSignin);
    }
    
    public async void OnClickSigninButton()
    {
        string username = mUsernameInputField.text;
        string password = mPasswordInputField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            GameManager.Instance.OpenConfirmPanel("빈칸을 모두 채워주세요.", () => { }, false);
            return;
        }

        SigninData signinData = new SigninData(username, password);

        await NetworkManager.Instance.SigninWithSigninData(signinData, (string nickname) =>
        {
            Debug.Log("어서오세요 "+nickname+"님");
            Hide(OnSigninButtonClick);
        }, (int result) =>
        {
            if (result == 0)        //INVALID_USERNAME
            {
                mUsernameInputField.text = string.Empty;
            }
            else if (result == 1)   //INVALID_PASSWORD
            {
                mPasswordInputField.text = string.Empty;
            }
        });
    }

    public void OnClickSignupButton()
    {
        GameManager.Instance.OpenSignupPanel();
    }
}