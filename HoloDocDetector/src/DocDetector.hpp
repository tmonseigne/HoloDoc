#pragma once

#include <opencv2/core.hpp>

#define DLL_EXPORT extern "C" int __declspec(dllexport) __stdcall

const enum ERROR_CODE
{
	NO_ERRORS = 0,
	EMPTY_MAT,
	TYPE_MAT,
	NO_DOCS,
	INVALID_DOC,
};

//********************************
//********** C# Types ************
//********************************
typedef uchar byte;

struct Color32
{
	byte r, g, b, a;
};

//********************************
//********** Unity Link **********
//********************************

/// <summary>
/// Simple document detection function.
/// </summary>
/// <param name="image">Unity image.</param>
/// <param name="width">Image width.</param>
/// <param name="height">Image height.</param>
/// <return>Error code </return>
DLL_EXPORT SimpleDocsDetection(Color32 *image, uint width, uint height,
							   byte *result, uint maxDocsCount, uint *outDocsCount, int *outDocsPoints);

/// <summary>
/// Documents detector.
/// </summary>
/// <param name="in">Unity image.</param>
/// <param name="out">Documents definition for Unity.</param>
DLL_EXPORT DocsDetection(Color32 *image, uint width, uint height,
						 Color32 background, uint *outDocsCount, int *outDocsPoints);

DLL_EXPORT DocExtraction(Color32 *image, uint width, uint height,
						 Color32 background, int *outDocPoints);
//*****************************
//********** Methods **********
//*****************************
/// <summary>Documents detection.</summary>
/// <param name="src">The source.</param>
/// <param name="contours">The contours.</param>
/// <param name="background">The background.</param>
/// <returns></returns>
int DocsDetection(const cv::Mat &src, const cv::Scalar &background, std::vector<std::vector<cv::Point>> &contours);

int DocExtraction(const cv::Mat &src, const cv::Scalar &background, std::vector<cv::Point> &contour, cv::Mat &dst);

int FeaturesExtraction(const cv::Mat &src/*, features*/);

int CompareDocs(const cv::Mat &im1, const cv::Mat &im2, double &similarity);
int CompareFeatures(/*features1, features2,*/double &similarity);
//******************************
//********** Computes **********
//******************************

/// <summary>
/// Binary edge detector.
/// </summary>
/// <param name="src">single or tri-channel 8-bit input image.</param>
/// <param name="dst">8-bit, single-channel binary image.</param>
/// <param name="min_tresh">first threshold for the hysteresis procedure.</param>
/// <param name="max_tresh">second threshold for the hysteresis procedure.</param>
/// <param name="aperture">aperture size for the Sobel operator.</param>
int BinaryEdgeDetector(const cv::Mat &src, cv::Mat &dst,
					   int min_tresh = 50, int max_tresh = 205, int aperture = 3);
