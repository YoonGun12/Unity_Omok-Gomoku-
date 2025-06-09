using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UserDataStructs
{
    public struct SignupData
    {
        public string username;
        public string nickname;
        public string password;
        public int profileimageindex;
    }
    
    public struct SigninData
    {
        public string username;
        public string password;

        public SigninData(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }
    
    public struct SigninResult
    {
        public int result;
        public string nickname;
    }

    public struct UserInfoResult
    {
        public string userId;
        public string username;
        public string nickname;
        public int profileimageindex;
        public int coin;
        public int wincount;
        public int losecount;
        public int drawcount;
        public int rank;
        public int rankuppoints;
        public int winlosestreak;
        public bool hasadremoval;
    }

    public struct ProfileImageData
    {
        public int profileimageindex;

        public ProfileImageData(int profileimageindex)
        {
            this.profileimageindex = profileimageindex;
        }
    }

    [Serializable]
    public struct UserRankProfileResult
    {
        public string username;
        public string nickname;
        public int rank;
        public int wincount;
        public int losecount;
        public int profileimageindex;
    }
    
    [Serializable]
    public struct UsersRankInfo
    {
        public UserRankProfileResult[] userrankprofiles;
        public UserRankProfileResult playerrankprofile;
    }

    public struct CoinData
    {
        public int amount;
    }

    public struct CoinResult
    {
        public bool success;
        public int coin;
        public string message;
    }
}