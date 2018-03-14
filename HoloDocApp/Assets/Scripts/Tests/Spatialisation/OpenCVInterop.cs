using UnityEngine;

using System.Runtime.InteropServices;

internal static class OpenCVInterop {
	[DllImport("DocDetector")]
	internal static extern unsafe int SimpleDocsDetection(ref Color32 image, uint width, uint height, 
		ref byte result, uint maxDocumentsCount, ref uint outDocumentsCount, ref int outDocumentsCorners);

	[DllImport("DocDetector")]
	internal static extern unsafe int DocsDetection(ref Color32 image, uint width, uint height, 
		Color32 background, ref uint outDocumentsCount, ref int outDocumentsCorners);
}
