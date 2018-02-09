using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Frame
{
	public Color32[] Data { get; set; }
	public Resolution Resolution { get; set; }

	public Frame(Resolution resolution, Color32[] data)
	{
		this.Resolution = resolution;
		this.Data = data;
	}
}