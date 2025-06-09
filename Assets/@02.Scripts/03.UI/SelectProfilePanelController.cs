using System;
using System.Collections;
using System.Collections.Generic;
using AudioEnums;
using UnityEngine;
using UnityEngine.UI;
using UserDataStructs;

public class SelectProfilePanelController : PopupPanelController
{
    [SerializeField] private List<Image> mProfileImages;
    public Action OnProfileSelected;
    public Action<int> OnProfileSelectedReturn;

    public override void Show()
    {
        for (int i = 0; i < mProfileImages.Count; i++)
        {
            mProfileImages[i].sprite = GameManager.Instance.GetProfileSprite(i);
        }
        base.Show();
    }

    public void OnClickCancelButton()
    {
        Hide();
    }

    public async void OnClickSelectProfileButtonFromProfilePanel(int profileIndex)
    {
        AudioManager.Instance.PlayAudioClip(ESfxType.Bird);
        await NetworkManager.Instance.ChangeProfileImage(new ProfileImageData(profileIndex), () => { }, () => { });
        OnProfileSelected?.Invoke();
        Hide(() => GameManager.Instance.OnMainPanelUpdate?.Invoke());
    }
    
    public void OnClickSelectProfileButtonFromSignupPanel(int profileIndex)
    {
        AudioManager.Instance.PlayAudioClip(ESfxType.Bird);
        OnProfileSelectedReturn?.Invoke(profileIndex);
        Hide();
    }
}
