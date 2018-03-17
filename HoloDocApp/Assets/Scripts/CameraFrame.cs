using UnityEngine;

public struct CameraFrame {

	public Color32[] Data { get; set; }
	public Resolution Resolution { get; set; }

	public CameraFrame(Resolution resolution, Color32[] data) {
		this.Resolution = resolution;
		this.Data = data;
	}
}