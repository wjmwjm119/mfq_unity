using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;


public class UploadMoive : MonoBehaviour
{

    public string m_strSecId;
    public string m_strSecKey;
    public int m_iRandom;
    public long m_qwNowTime;
    public int m_iSignValidDuration;

    public static long GetIntTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1);
        return Convert.ToInt64(ts.TotalSeconds);
    }

    private byte[] hash_hmac_byte(string signatureString, string secretKey)
    {
        var enc = Encoding.UTF8; HMACSHA1 hmac = new HMACSHA1(enc.GetBytes(secretKey));
        hmac.Initialize();
        byte[] buffer = enc.GetBytes(signatureString);
        return hmac.ComputeHash(buffer);
    }

    public string GetUploadSignature()
    {
        string strContent = "";
        strContent += ("secretId=" + Uri.EscapeDataString((m_strSecId)));
        strContent += ("&currentTimeStamp=" + m_qwNowTime);
        strContent += ("&expireTime=" + (m_qwNowTime + m_iSignValidDuration));
        strContent += ("&random=" + m_iRandom);

        byte[] bytesSign = hash_hmac_byte(strContent, m_strSecKey);
        Debug.Log(bytesSign.Length);
        byte[] byteContent = System.Text.Encoding.Default.GetBytes(strContent);
        Debug.Log(byteContent.Length);
        byte[] nCon = new byte[bytesSign.Length + byteContent.Length];
        Debug.Log(nCon.Length);
        bytesSign.CopyTo(nCon, 0);
        byteContent.CopyTo(nCon, bytesSign.Length);
        Debug.Log("/////////////////////////////");
        for (int i = 0; i < bytesSign.Length; i++)
        {
            Debug.Log(bytesSign[i]);
        }
        

        return Convert.ToBase64String(nCon);
    }


    private void Start()
    {

//        m_strSecId = "个人 API 密钥中的Secret Id";
//        m_strSecKey = "个人 AP I密钥中的Secret Key";
        m_qwNowTime = GetIntTimeStamp();
        m_iRandom = new System.Random().Next(0, 1000000);
        m_iSignValidDuration = 3600 * 24 * 2;

        Debug.Log(GetUploadSignature());
    }

}
