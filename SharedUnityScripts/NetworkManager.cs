﻿using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary>
/// This class handles all the network requests and serialization/deserialization of data
/// </summary>
public class NetworkManager : MonoBehaviour {

    [Tooltip("reference to ChatUI class")] 
    public ChatUIManager chatUI;

    [Tooltip("the url at which bot's custom connector is hosted ")]
    public string rasa_url;

    [Tooltip("the user name in sender field to rasa")]
    public string UserName; 

    [Tooltip("reference to the Speech class for voice synthesis")]
    public SpeechManager speechManager;

    [Tooltip("reference to the coach command event manager")]
    public CoachCommandExecutor commandExecutor;

    private void Start()
    {
        int uniqueID = UnityEngine.Random.Range(0,100000);
        UserName += uniqueID.ToString();
    }

    /// <summary>
    /// This method is called when user has finished an utterance to the assitant.
    /// It calls the <see cref="NetworkManager.PostRequest(string, string)"> coroutine to send
    /// the user message to the bot and also updates the UI with user message.
    /// </summary>
    public void SendVoiceMessageToRasa(string message)
    {
        // get user messasge from input, create a json object 

        PostMessage postMessage = new PostMessage
        {
            sender = UserName,
            message = message
        };
        string jsonBody = JsonUtility.ToJson(postMessage);

        // update UI object with user message, dont update if it's an inform
        if (!message.Contains("\\inform"))
        {
            chatUI.UpdateDisplay("user", message, "text");
        }

        // Create a post request with the data to send to Rasa server
        StartCoroutine(PostRequest(rasa_url, jsonBody));
    }

    /// <summary>
    /// This is a coroutine to asynchronously send a POST request to the Rasa server with 
    /// the user message. The response is deserialized and rendered on the UI object.
    /// </summary>
    /// <param name="url">the url where Rasa server is hosted</param>
    /// <param name="jsonBody">user message serialized into a json object</param>
    /// <returns></returns>
    private IEnumerator PostRequest (string url, string jsonBody) {
        // Create a request to hit the rasa custom connector
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] rawBody = new System.Text.UTF8Encoding().GetBytes(jsonBody);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(rawBody);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // receive the response
        yield return request.SendWebRequest();
        print(request.downloadHandler.text);

        // Render the response on UI object
        RecieveMessage(request.downloadHandler.text);
    }


    /// <summary>
    /// This method updates the UI object with bot response
    /// </summary>
    /// <param name="response">response json recieved from the bot</param>
    public void RecieveMessage (string response) {
        // Deserialize response recieved from the bot
        print(response);
        RootMessages recieveMessages =
            JsonUtility.FromJson<RootMessages>("{\"messages\":" + response + "}");

        // show message based on message type on UI
        foreach (RecieveData message in recieveMessages.messages) {
            FieldInfo[] fields = typeof(RecieveData).GetFields();
            foreach (FieldInfo field in fields) {
                string data = null;

                // extract data from response in try-catch for handling null exceptions
                try {
                    data = field.GetValue(message).ToString();
                } catch (NullReferenceException) { }

                
                // print data
                if (data != null && field.Name != "recipient_id") {
                    //Handle coach command
                    if (data.Contains("\\COMMAND"))
                    {
                        Debug.Log("RecieveMEssage coachcommand:" + data);
                        //Execute coach command
                        commandExecutor.ExecuteCommand(data);
                    }
                    else {
                        chatUI.UpdateDisplay("bot", data, field.Name);
                        speechManager.SpeechPlayback(data);
                    }
                    
                }
            }
        }
    }


    /// <summary>
    /// This method gets url resource from link and applies it to the passed texture.
    /// </summary>
    /// <param name="url">url where the image resource is located</param>
    /// <param name="image">RawImage object on which the texture will be applied</param>
    /// <returns></returns>
    public IEnumerator SetImageTextureFromUrl (string url, Image image) {
        // Send request to get the image resource
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
            // image could not be retrieved
            Debug.Log(request.error);

        else {
            // Create Texture2D from Texture object
            Texture texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Texture2D texture2D = texture.ToTexture2D();

            // set max size for image width and height based on chat size limits
            float imageWidth = 0, imageHeight = 0, texWidth = texture2D.width, texHeight = texture2D.height;
            if ((texture2D.width > texture2D.height) && texHeight > 0) {
                // Landscape image
                imageWidth = texWidth;
                // Landscape image
                imageWidth = texWidth;
                if (imageWidth > 200) imageWidth = 200;
                float ratio = texWidth / imageWidth;
                imageHeight = texHeight / ratio;
            }
            if ((texture2D.width < texture2D.height) && texWidth > 0) {
                // Portrait image
                imageHeight = texHeight;
                if (imageHeight > 200) imageHeight = 200;
                float ratio = texHeight / imageHeight;
                imageWidth = texWidth / ratio;
            }

            // Resize texture to chat size limits and attach to message
            // Image object as sprite
            TextureScale.Bilinear(texture2D, (int)imageWidth, (int)imageHeight);
            image.sprite = Sprite.Create(
                texture2D,
                new Rect(0.0f, 0.0f, texture2D.width, texture2D.height),
                new Vector2(0.5f, 0.5f), 100.0f);

            // Resize and reposition all chat bubbles
            StartCoroutine(chatUI.RefreshChatBubblePosition());
        }
    }
}