using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using AudioEnums;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UserDataStructs;

public class SignupPanelController : PopupPanelController
{
    [SerializeField] private TMP_InputField mUsernameInputField;
    [SerializeField] private TMP_InputField mNicknameInputField;
    [SerializeField] private TMP_InputField mPasswordInputField;
    [SerializeField] private TMP_InputField mConfirmPasswordInputField;
    [SerializeField] private Image mProfileImage;
    private List<PopupPanelController> mChildPanels = new List<PopupPanelController>();
    private int mProfileImageIndex = 0;

    public async void OnClickConfirmButton()
    {
        var username = mUsernameInputField.text;
        var nickname = mNicknameInputField.text;
        var password = mPasswordInputField.text;
        var confirmPassword = mConfirmPasswordInputField.text;
        var profileImageIndex = mProfileImageIndex;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(confirmPassword))
        {
            GameManager.Instance.OpenConfirmPanel("입력할 항목이 남아있습니다.", () => { }, false);
            return;
        }

        if (!isValidEmailID(username))
        {
            GameManager.Instance.OpenConfirmPanel("Email 형식의 ID가 아닙니다.", () => { }, false);
            return;
        }

        if (password.Equals(confirmPassword))
        {
            SignupData signupData = new SignupData();
            signupData.username = username;
            signupData.nickname = nickname;
            signupData.password = password;
            signupData.profileimageindex = profileImageIndex;

            // 서버로 SignupData 전달하면서 회원가입 진행
            await NetworkManager.Instance.Signup(signupData, () => { Hide(); }, () =>
            {
                mUsernameInputField.text = string.Empty;
                mNicknameInputField.text = string.Empty;
                mPasswordInputField.text = string.Empty;
                mConfirmPasswordInputField.text = string.Empty;
            });
        }
        else
        {
            GameManager.Instance.OpenConfirmPanel("비밀번호가 서로 다릅니다.", () =>
            {
                mPasswordInputField.text = string.Empty;
                mConfirmPasswordInputField.text = string.Empty;
            }, false);
        }
    }

    public override void Show()
    {
        mChildPanels.Clear();
        
        SetProfileImage(mProfileImageIndex);
        base.Show();
    }

    public override void Hide(Action hideDelegate = null)
    {
        foreach (var panel in mChildPanels)
        {
            if (panel != null)
            {
                panel.Hide();
            }
        }
        mChildPanels.Clear();
        
        base.Hide(hideDelegate);
    }
    public void OnClickCancelButton()
    {
        Hide();
    }
    
    public void OnClickProfileButton()
    {
        AudioManager.Instance.PlayAudioClip(ESfxType.Bird);
        var childPanel = GameManager.Instance.OpenSelectProfilePanelFromSignupPanel();
        SelectProfilePanelController selectPanel = childPanel as SelectProfilePanelController;
        if (selectPanel != null)
        {
            selectPanel.OnProfileSelectedReturn = SetProfileImage;
        }
        mChildPanels.Add(childPanel);
    }

    public void SetProfileImage(int profileImageIndex)
    {
        mProfileImageIndex = profileImageIndex;
        mProfileImage.sprite = GameManager.Instance.GetProfileSprite(profileImageIndex);
    }

    private bool isValidEmailID(string emailID)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(emailID, pattern, RegexOptions.IgnoreCase);
    }
}