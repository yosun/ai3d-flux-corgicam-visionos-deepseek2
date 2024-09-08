using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using GLTFast; // Ensure you have the GLTFast package installed

public class Post2Stability : MonoBehaviour
{
    private string apiUrl = "https://api.stability.ai/v2beta/3d/stable-fast-3d";
    private string apiKey = "Bearer stability API KEY HERE"; // Replace with your actual API key
     

    Texture2D LoadTextureFromFile(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
        {
            return tex;
        }
        return null;
    }

    public void ActuallyPostImage(Texture2D texture)
    {
        StartCoroutine(PostImage(texture));
    }

    IEnumerator PostImage(Texture2D texture)
    {
        // Convert the Texture2D to a byte array
        byte[] imageBytes = texture.EncodeToPNG();

        // Create a UnityWebRequest to post the image
        UnityWebRequest request = UnityWebRequest.PostWwwForm(apiUrl, "POST");
        request.SetRequestHeader("Authorization", apiKey);

        // Create a multipart form data section
        string boundary = "----CustomBoundary" + System.DateTime.Now.Ticks.ToString("x");
        string formData = $"--{boundary}\r\nContent-Disposition: form-data; name=\"image\"; filename=\"cat-statue.png\"\r\nContent-Type: image/png\r\n\r\n";
        byte[] formDataBytes = System.Text.Encoding.UTF8.GetBytes(formData);
        byte[] boundaryBytes = System.Text.Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");

        // Combine the form data, image bytes, and boundary bytes
        byte[] combinedBytes = new byte[formDataBytes.Length + imageBytes.Length + boundaryBytes.Length];
        System.Buffer.BlockCopy(formDataBytes, 0, combinedBytes, 0, formDataBytes.Length);
        System.Buffer.BlockCopy(imageBytes, 0, combinedBytes, formDataBytes.Length, imageBytes.Length);
        System.Buffer.BlockCopy(boundaryBytes, 0, combinedBytes, formDataBytes.Length + imageBytes.Length, boundaryBytes.Length);

        // Set the content type to multipart/form-data with the correct boundary
        request.uploadHandler = new UploadHandlerRaw(combinedBytes);
        request.SetRequestHeader("Content-Type", $"multipart/form-data; boundary={boundary}");

        // Set the download handler to receive the binary response
        request.downloadHandler = new DownloadHandlerBuffer();

        // Send the request
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text); // Log the response for more details
        }
        else
        {
            // Save the binary response to a file
            string outputFilePath = Path.Combine(Application.persistentDataPath, "3d-cat-statue.glb");
            File.WriteAllBytes(outputFilePath, request.downloadHandler.data);
            Debug.Log("Response saved to: " + outputFilePath);

            // Load the glTF asset using GLTFast
            LoadGlbAsset(outputFilePath);
        }
    }

    static GameObject previousGO; 

    void LoadGlbAsset(string filePath)
    {
        // Create a new GameObject to hold the glTF asset
        GameObject gltfContainer = new GameObject("GLTF Container");

        // Load the glTF asset using GLTFast
        GltfAsset gltfAsset = gltfContainer.AddComponent<GltfAsset>();
        gltfAsset.Url = filePath; // Use the file path directly

        gltfAsset.transform.localScale *= .5f;

        if (previousGO != null)
        {
            if(previousGO.transform.position.magnitude < Mathf.Epsilon)
            {
                previousGO.transform.position = new Vector3(9999, 9999, 9999);
            }
        }

        previousGO = gltfAsset.gameObject;

        //previousGO.AddComponent<PieceSelectionBehaviour>();

        // Optionally, you can subscribe to the loading events
        //  gltfAsset.onLoadComplete += OnLoadComplete;
    }

    void OnLoadComplete(bool success)
    {
        if (success)
        {
            Debug.Log("GLTF asset loaded successfully.");
        }
        else
        {
            Debug.LogError("Failed to load GLTF asset.");
        }
    }
}
