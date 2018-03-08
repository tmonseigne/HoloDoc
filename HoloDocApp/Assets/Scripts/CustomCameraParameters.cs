using UnityEngine;

public static class CustomCameraParameters
{
    public static Matrix4x4 ProjectionMatrix { get; set; }
    public static Matrix4x4 WorldMatrix { get; set; }
    public static Resolution Resolution { get; set; }
}