using UnityEngine;
using System.Runtime.InteropServices;

// Define the functions which can be called from the .dll.
internal static class OpenCVInterop {
	[DllImport("DocDetector")]
	internal static extern unsafe int SimpleDocumentDetection(ref Color32 image, uint width, uint height, ref byte result,
		uint maxDocumentsCount, ref uint outDocumentsCount, ref int outDocumentsCorners);
}