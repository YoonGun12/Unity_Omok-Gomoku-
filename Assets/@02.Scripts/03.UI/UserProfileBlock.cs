using AudioEnums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UserDataStructs;

public class UserProfileBlock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mUserRankText;
    [SerializeField] private TextMeshProUGUI mNicknameRankText;
    [SerializeField] private TextMeshProUGUI mWinLoseText;
    [SerializeField] private Image mProfileImage;

    public void SetProfileBlock(in UserRankProfileResult userInfo, int userRank)
    {
        mUserRankText.text = userRank.ToString();
        mNicknameRankText.text = userInfo.rank.ToString() + "급 " + userInfo.nickname;
        mWinLoseText.text = userInfo.wincount.ToString() + "승 " + userInfo.losecount.ToString() + "패";
        mProfileImage.sprite = GameManager.Instance.GetProfileSprite(userInfo.profileimageindex);
    }

    public void SetColor(Color color)
    {
        GetComponent<Image>().color = color;
    }
    
    public void SetColor(string hexColor)
    {
        Color parsedColor;
        if (ColorUtility.TryParseHtmlString(hexColor, out parsedColor))
        {
            GetComponent<Image>().color = parsedColor;
        }
        else
        {
            Debug.LogWarning("Invalid hex color string: " + hexColor);
        }
    }

    public void OnClickProfileImage()
    {
        AudioManager.Instance.PlayAudioClip(ESfxType.Bird);
    }
}