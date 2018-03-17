using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HoloToolkit.Unity;

using UnityEngine;

public class ColorPicker : Singleton<ColorPicker> {

    [Range(0.01f, 1.0f), 
     Tooltip("This property will force the color picking to be done in a specific area of the image. " +
        "By modifying this property you can control how much percentage of the image will be considered " +
        "when returning average color.")]
    public float PickingAreaPercentage = 0.1f;

    [Tooltip("This boolean is used to force the area to be squared.")]
    public bool ForceSquared = false;

    public Color AverageColor(Texture2D texture) {
        int blockWidth = (int)(texture.width * this.PickingAreaPercentage);
        int blockHeight = (int)(texture.height * this.PickingAreaPercentage);

        if (this.ForceSquared) {
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
        Color averageColor = new Color();
        foreach (Color color in bloc) {
            averageColor += color;
        }

        averageColor /= bloc.Length;
        return averageColor;
    }
}
