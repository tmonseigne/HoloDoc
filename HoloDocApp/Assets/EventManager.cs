using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class EventManager : MonoBehaviour, IInputClickHandler
{ 
    public GameObject outputBloc;
    public GameObject outputColor;

    public void PickColor(Texture2D photo)
    {
        float percentage = 0.5f;
        int blockWidth = (int) (photo.width * percentage);
        int blockHeight = (int) (photo.height * percentage);

        if (blockWidth > photo.height || blockWidth > blockHeight)
        {
            blockWidth = blockHeight;
        }

        if (blockHeight > photo.width || blockHeight > blockWidth) ;
        {
            blockHeight = blockWidth;
        }

        Debug.Log(blockHeight);
        Debug.Log(blockWidth);
        int startX = photo.width / 2 - blockWidth / 2;
        int startY = photo.height / 2 - blockHeight / 2;
        Color[] background = photo.GetPixels(startX, startY, blockWidth, blockHeight);

        Texture2D extracted = new Texture2D(blockWidth, blockHeight);
        extracted.SetPixels(background);
        extracted.Apply(true);

        outputBloc.GetComponent<Renderer>().material.mainTexture = extracted;

        float r = 0;
        float g = 0;
        float b = 0;
        foreach ( Color pixel in background)
        {
            r += pixel.r;
            g += pixel.g;
            b += pixel.b;
        }

        r /= background.Length;
        g /= background.Length;
        b /= background.Length;

        Color finalColor = new Color(r, g, b);

        Debug.LogFormat("Average background color is r:{0} g:{1} b:{2}", r, g, b);

        Texture2D test = new Texture2D(100, 100);
        Color[] pixels = test.GetPixels();
        for(int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = finalColor;
        }

        test.SetPixels(pixels);
        test.Apply(true);

        outputColor.GetComponent<Renderer>().material.mainTexture = test;
    }


    public void OnPhotoTaken(Texture2D photo, Matrix4x4 proj, Matrix4x4 world, bool projB, bool worldB)
    {
        Debug.Log(projB);
        Debug.Log(worldB);
        Debug.Log(proj.ToString());
        Debug.Log(world.ToString());

        //PickColor(photo);
        Color c = ColorPicker.Instance.AverageColor(photo);
        Debug.Log(c);

        Texture2D test = new Texture2D(1, 1);
        Color[] pixels = test.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = c;
        }

        test.SetPixels(pixels);
        test.Apply(true);

        outputColor.GetComponent<Renderer>().material.mainTexture = test;
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.Log("Air tap");
        PhotoCapturer.Instance.TakePhoto(OnPhotoTaken);
    }
}
