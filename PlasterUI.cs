using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlasterUI : MonoBehaviour
{
    public FalApiRequest fal;
    public Image img;

    public TMP_InputField input;

    public Post2Stability post2stability;

    void Start()
    {
        fal.TexReturned += TexReturned;
       
    }

    public void SubmitInputField()
    {
        fal.ActuallySendPrompt(input.text);
    }

    // Update is called once per frame
    void TexReturned(Texture2D tex)
    {
        img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),Vector2.zero);

        post2stability.ActuallyPostImage(tex);
    }
}
