using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.UI;


using TMPro;
using BestHTTP;
using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;

public class MailchimpResponse
{
    public string detail;
}

public class MailingListScreen : MonoBehaviour, IInitializable
{
    public static MailingListScreen Instance;

    [SerializeField] private TextMeshProUGUI _statusText;
    private Canvas _canvas;
    private Button _submitButton;
    private TMP_InputField _emailInput;

    private string _mailchimpAPIKey = "07e3abffc7c87a3c1e49d3f247efd51b-us6";
    private string _mailchimpListID = "88e631af10";

    // Start is called before the first frame update
    public void Init()
    {
        Instance = this;

        _canvas         = GetComponent<Canvas>();
        _emailInput     = GetComponentInChildren<TMP_InputField>();
        _submitButton   = GetComponentInChildren<Button>();
    }

    public void Skip()
    {
        var camera = ProCamera2D.Instance;
        var transitions = camera.GetComponent<ProCamera2DTransitionsFX>();
        transitions.OnTransitionExitEnded += delegate ()
        {
            //this.SetActive(false);

            // TODO: Load STart Screen 
            //StartCoroutine(SceneLoader.Instance.LoadScene("StartScreen"));
            // SceneLoader.Instance.BeginMapTransition("StartScreen", null, "");
            Debug.Log("Calling OnTransition Exit Ended");
            StartCoroutine(GoToStartScreen());
        };

        transitions.TransitionExit();
    }

    public IEnumerator GoToStartScreen()
    {
        yield return StartCoroutine(SceneLoader.Instance.LoadScene("StartScreen"));
        
        this.SetActive(false);

        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("IntroCinematics");
        
        yield return null;
    }

    // Event
    public void HandleSubmission()
    {
        var enteredEmail = _emailInput.text;

        if (IsValidEmail(enteredEmail))
        {
            _statusText.text = "";

            var addMemberUrl = $"https://us6.api.mailchimp.com/3.0/lists/{_mailchimpListID}/members";

            //Create my object
            var myData = new
            {
                email_address = enteredEmail,
                status = "subscribed",
            };

            //Tranform it to Json object
            string json = JsonConvert.SerializeObject(myData).ToString();

            var paramsAsBytes = Encoding.ASCII.GetBytes(json);

            HTTPRequest request = new HTTPRequest(new Uri(addMemberUrl), HTTPMethods.Post, OnRequestFinished);
            request.AddHeader("Authorization", $"Bearer {_mailchimpAPIKey}");
            request.RawData = paramsAsBytes;
            request.Send();
        }
        else
        {
            _statusText.text = "Error: Invalid e-mail";
        }
    }

    void OnRequestFinished(HTTPRequest request, HTTPResponse response)
    {


        if (response.IsSuccess)
        {
            _statusText.color = Color.green;
            _statusText.text = "Thank you!";

            // Todo: move on to start screen;
            Debug.Log("full transition exit ");
            StartCoroutine(GoToStartScreen());
        }
        else
        {
            var res = JsonConvert.DeserializeObject < MailchimpResponse>(response.DataAsText);

            _statusText.text = res.detail;
        }
    }

    private bool IsValidEmail(string email)
    {
        bool isEmail = Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        return isEmail;
    }
}
