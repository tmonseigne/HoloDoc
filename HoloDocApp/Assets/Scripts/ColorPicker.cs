using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using HoloToolkit.Unity;

public class ColorPicker : Singleton<ColorPicker> {

    [Range(0.01f, 1.0f), 
     Tooltip("This property will force the color picking to be done in a specific area of the image. " +
        "By modifying this property you can control how much percentage of the image will be considered " +
        "when returning average color.")]
    public float PickingAreaPercentage = 0.5f;

    [Tooltip("This boolean is used to force the area to be squared.")]
    public bool forceSquared = false;

    public Color AverageColor(Texture2D texture)
    {
        int blockWidth = (int)(texture.width * PickingAreaPercentage);
        int blockHeight = (int)(texture.height * PickingAreaPercentage);

        if (forceSquared) {
            if (blockWidth > blockHeight) {
                blockWidth = blockHeight;
            }
            else {
                blockHeight = blockWidth;
            }
        }

        int startX = texture.width / 2 - blockWidth / 2;
        int startY = texture.height / 2 - blockHeight / 2;

        Color[] bloc = texture.GetPixels(startX, startY, blockWidth, blockHeight);
        Color average = new Color();
        foreach (Color c in bloc) {
            average += c;
        }

        average /= bloc.Length;
        return average;
    }
}
