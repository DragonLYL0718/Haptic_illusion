using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using TMPro;

public class SurveySystem2 : MonoBehaviour
{
    public string userID;

    //time count
    [HideInInspector]
    public float totalTime;

    //minimum time for exploring
    public float timeRemaining;
    [HideInInspector]
    public static float oneTrialTime;

    //check if time is still running
    [HideInInspector]
    public static bool timeIsRunning;
    //check if time is up an input is enabled
    private bool inputEnabled;

    //check if it is waiting time
    [HideInInspector]
    public static bool isWaiting = true;
    //Whether to record the position of the foot
    [HideInInspector]
    public static bool RecordFlag = false;

    //test time for illusion
    private float timeWithIllusion;
    //test time for real
    private float timeWithoutIllusion;

    [HideInInspector]
    public static int number = 0;
    [SerializeField]
    private int trialNumber;
    [HideInInspector]
    public static bool hasEnded = false;


    public GameObject trialBox;
    public TextMeshProUGUI trialNumberText;

    public GameObject waitingBox;
    public TextMeshProUGUI waitingBoxText;

    public GameObject instructionBox;
    public TextMeshProUGUI instructionBoxText;

    //questionnaire and answers
    public GameObject firstBox;
    public GameObject secondBox;
    public GameObject thirdBox;

    [HideInInspector]
    public int firstAnswer;
    [HideInInspector]
    public int secondAnswer;
    [HideInInspector]
    public int thirdAnswer;

    //database
    private XML2 xmlManager;
    private UserDatabase2 userDatabase;

    //AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        xmlManager = GetComponent<XML2>();
        userDatabase = new UserDatabase2();
        //audioSource = GetComponent<AudioSource>();

        totalTime = 0;
        oneTrialTime = timeRemaining;
        timeIsRunning = true;
        inputEnabled = false;

        firstBox.SetActive(false);
        secondBox.SetActive(false);
        thirdBox.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (isWaiting == false)
        {
            totalTime += Time.fixedDeltaTime;
            //Debug.Log("1");
        }

        if (!hasEnded)
        {
            if (timeIsRunning)
            {
                //Debug.Log("2");
                trialBox.SetActive(true);
                instructionBox.SetActive(true);

                //when timer is active, the dialogue boxes are inactive
                firstBox.SetActive(false);
                secondBox.SetActive(false);
                thirdBox.SetActive(false);

                if (timeRemaining > 10.0f)
                {
                    //Debug.Log("3");
                    waitingBoxText.text = "WAIT";
                    waitingBoxText.color = Color.red;
                    instructionBoxText.text = "Please, get into the starting position";
                    isWaiting = true;
                }
                else
                {
                    //Debug.Log("4");
                    RecordFlag = true;
                    instructionBoxText.text = "Please, explore the virtual object from right to left";
                    waitingBoxText.text = "GO";
                    waitingBoxText.color = Color.black;
                    isWaiting = false;
                }

                //if there is still time remainiing, continue subtracting
                if (timeRemaining > 0)
                {
                    //Debug.Log("5");
                    timeRemaining -= Time.fixedDeltaTime;
                }

                //when time is up, play the beep sound, set remaining time to 0, enable input
                else
                {
                    //Debug.Log("6");
                    // audioSource.Play();
                    timeIsRunning = false;
                    timeRemaining = 0;
                    inputEnabled = true;
                }
            }

        }
    }
    // Update is called once per frame
    void Update()
    {
        trialNumber = number + 1;

        //samples are doubled during one trial
        if (trialNumber <= Randomize2.samples.Length)
        {
            int sampleNumber = trialNumber % 2 == 0 ? (trialNumber / 2) : (trialNumber / 2 + 1);

            int total = Randomize2.samples.Length / 2;
            trialNumberText.text = (sampleNumber + "/" + total + ":" + (trialNumber%2));
        }
        else
        {
            trialNumberText.text = "-";
        }


        //if input is enabled, then press space bar to call first question's box and disable the input
        if (inputEnabled)
        {   
            //if (clickAction.GetStateDown(inputSource) || Input.GetKeyDown("space"))
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log(totalTime);

                RecordFlag = false;

                if (Randomize2.illusions[number] == true)
                {
                    timeWithIllusion = totalTime;
                    totalTime = 0;
                }
                else
                {
                    timeWithoutIllusion = totalTime;
                    totalTime = 0;
                }
                if (number % 2 == 1)
                {
                    instructionBox.SetActive(false);
                    trialBox.SetActive(false);
                    waitingBox.SetActive(false);
                    firstBox.SetActive(true);
                }
                else
                {
                    Increment();
                }

                inputEnabled = false;
            }

        }
    }
    //increment to move on to next sample value
    public void Increment()
    {
        if (number < Randomize2.samples.Length)
        {
            if(number % 2 == 1)
            {
                UserDataEntry2 dataEntry = GetAnswers();
                Debug.Log(trialNumber + " " + dataEntry.firstAnswer + " " + dataEntry.secondAnswer + " " + dataEntry.thirdAnswer);
                userDatabase.dataList.Add(dataEntry);
                xmlManager.SaveItems(userDatabase, userID);
                Debug.Log("saved");
            }
            number++;
            timeRemaining = oneTrialTime;
            timeIsRunning = true;
            totalTime = 0;
        }

        if (number == Randomize2.samples.Length)
        {
            //xmlManager.SaveItems(userDatabase, userID);
            //Debug.Log("saved");
            instructionBox.SetActive(false);
            trialBox.SetActive(false);
        }
    }

    public void AnswerFirstQuestion(bool answer)
    {
        if (answer)
        {
            firstAnswer = 1;
        }
        else
        {
            firstAnswer = 2;
        }
    }
    public void AnswerSecondQuestion(bool answer)
    {
        if (answer)
        {
            secondAnswer = 1;
        }
        else
        {
            secondAnswer = 2;
        }
    }
    public void AnswerThirdQuestion(bool answer)
    {
        if (answer)
        {
            thirdAnswer = 1;
        }
        else
        {
            thirdAnswer = 2;
        }
    }

    public UserDataEntry2 GetAnswers()
    {
        UserDataEntry2 dataEntry = new UserDataEntry2
        {
            number = (number + 1) / 2,
            sample = (Randomize2.samples[number] >= 1 && Randomize2.samples[number] <= 2) ? (2 / Randomize2.samples[number]) : Randomize2.samples[number],
            IllusionSample = (Randomize2.illusions[number]) ? 2 : 1,
            timeWithIllusion = timeWithIllusion,
            timeWithoutIllusion = timeWithoutIllusion,
            answerTime = totalTime,
            firstAnswer = firstAnswer,
            secondAnswer = secondAnswer,
            thirdAnswer = thirdAnswer
        };

        return dataEntry;
    }
}
