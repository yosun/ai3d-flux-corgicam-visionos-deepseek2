using System.Collections;  
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class FalApiRequest : MonoBehaviour
{
    private string falKey = "FAL_API_KEY"; // Replace with your actual FAL_KEY
    private string apiUrl = "https://fal.run/fal-ai/flux-lora";

    public string testPrompt = "photo of CAM corgi teaching quantum physics at caltech";

    public delegate void VOIDTEX(Texture2D tex);
    public VOIDTEX TexReturned;

    void Start()
    {
        // testing 
        ActuallySendPrompt(testPrompt);
    }

    public void ActuallySendPrompt(string s)
    {
        StartCoroutine(SendPromptAndLoadImage(s));
    }

    IEnumerator SendPromptAndLoadImage(string prompt)
    {
        // Create the JSON payload
        string jsonPayload = @"{
            ""loras"": [
                {
                    ""path"": ""https://storage.googleapis.com/fal-flux-lora/97e6cffdacef4a2eb9848c2e29d6c143_lora.safetensors"",
                    ""scale"": 1
                }
            ],
            ""prompt"": """ + prompt + @""",
            ""embeddings"": [],
            ""model_name"": null,
            ""enable_safety_checker"": true
        }";

        // Create the UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Key " + falKey);
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            // Parse the JSON response
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Response: " + jsonResponse);

            // Deserialize the JSON response
            ResponseData responseData = JsonUtility.FromJson<ResponseData>(jsonResponse);

            // Check if there are images in the response
            if (responseData.images != null && responseData.images.Length > 0)
            {
                string imageUrl = responseData.images[0].url;

                // Download the image
                UnityWebRequest textureRequest = UnityWebRequestTexture.GetTexture(imageUrl);
                yield return textureRequest.SendWebRequest();

                if (textureRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error downloading image: " + textureRequest.error);
                }
                else
                {
                    // Load the image into a Texture2D
                    Texture2D texture = DownloadHandlerTexture.GetContent(textureRequest);

                    TexReturned?.Invoke(texture);
                }
            }
            else
            {
                Debug.LogError("No images found in the response.");
            }
        }
    }

    [System.Serializable]
    private class ResponseData
    {
        public ImageData[] images;
        public Timings timings;
        public long seed;
        public bool[] has_nsfw_concepts;
        public string prompt;
    }

    [System.Serializable]
    private class ImageData
    {
        public string url;
        public int width;
        public int height;
        public string content_type;
    }

    [System.Serializable]
    private class Timings
    {
        public float inference;
    }
}
