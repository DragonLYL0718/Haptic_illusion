using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;


public class XML2 : MonoBehaviour
{
    public void SaveItems(UserDatabase2 userDatabase, string userID)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(UserDatabase2));
        FileStream fileStream = new FileStream("./UserData/user_data_" + userID + ".xml", FileMode.OpenOrCreate);
        serializer.Serialize(fileStream, userDatabase);
        fileStream.Close();
    }
}

[System.Serializable]
public class UserDataEntry2
{
    public int number;
    public float sample;
    public int IllusionSample;
    public float timeWithIllusion;
    public float timeWithoutIllusion;
    public float answerTime;
    public int firstAnswer;
    public int secondAnswer;
    public int thirdAnswer;

    public UserDataEntry2()
    {
        this.number = 0;
        this.sample = 0.0f;
        this.IllusionSample = 1;
        this.timeWithIllusion = 0.0f;
        this.timeWithoutIllusion = 0.0f;
        this.answerTime = 0.0f;
        this.firstAnswer = 1;
        this.secondAnswer = 1;
        this.thirdAnswer = 1;
    }

}

[System.Serializable]
public class UserDatabase2
{
    public List<UserDataEntry2> dataList;

    public UserDatabase2()
    {
        dataList = new List<UserDataEntry2>();
    }
}
