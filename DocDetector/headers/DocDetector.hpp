#pragma once

#include <opencv2/core.hpp>

//********************************
//********** Unity Link **********
//********************************
/// <summary>
/// Documents detector.
/// </summary>
/// <param name="docs">All Documents.</param>
void DocDetector(/*const image unity en entrée& in, */ std::vector<int> &docs);

//******************************
//********** Computes **********
//******************************
/// <summary>
/// Unity frame to OpenCV Mat.
/// </summary>
/// <param name="dst">OpenCV Mat.</param>
void UnityToOpenCVMat(/*const blabla,*/ cv::Mat &dst);

/// <summary>
/// Binary edge detector.
/// </summary>
/// <param name="src">The source (8 bit 3 channels mat).</param>
/// <param name="dst">The destimation (binary 8 bit mat).</param>
/// <param name="min_tresh">The minimum treshold.</param>
/// <param name="max_tresh">The maximum treshold.</param>
/// <param name="aperture">The aperture size of Sobel.</param>
void BinaryEdgeDetector(const cv::Mat &src, cv::Mat &dst, int min_tresh = 100, int max_tresh = 205, int aperture = 3);

/// <summary>
/// Lines detector.
/// </summary>
/// <param name="src">The source (binary 8 bit mat).</param>
/// <param name="lines">The lines.</param>
void LinesDetector(const cv::Mat &src, std::vector<cv::Vec4i> &lines);

/// <summary>
/// Lines to docs detection.
/// </summary>
/// <param name="lines">The lines.</param>
/// <param name="docs">All Documents..</param>
void LinesToDocs(const std::vector<cv::Vec4i> &lines, std::vector<int> &docs);

//******************************
//********** Drawings **********
//******************************
/// <summary>
/// Draws the lines.
/// </summary>
/// <param name="src">The source.</param>
/// <param name="lines">The lines.</param>
/// <param name="dst">The destination.</param>
void DrawLines(const cv::Mat &src, const std::vector<cv::Vec4i> &lines, cv::Mat &dst);

/// <summary>
/// Draws the document shape.
/// </summary>
/// <param name="src">The source.</param>
/// <param name="docs">The docs.</param>
/// <param name="dst">The destination.</param>
void DrawDocShape(const cv::Mat &src, const std::vector<int> &docs, cv::Mat &dst);