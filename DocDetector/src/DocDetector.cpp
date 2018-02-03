#include "DocDetector.hpp"
#include <opencv2/imgproc.hpp>

using namespace std;
using namespace cv;

void DocDetector(/*const image unity en entrée& in, */std::vector<int>& docs)
{
	Mat src, EdgeDetect;
	std::vector<cv::Vec4i> lines;
	UnityToOpenCVMat(/*in,*/ src);
	BinaryEdgeDetector(src, EdgeDetect);
	LinesDetector(EdgeDetect, lines);
	LinesToDocs(lines, docs);
}

void UnityToOpenCVMat(/*const blabla& src,*/ cv::Mat& dst)
{
}

void BinaryEdgeDetector(const cv::Mat& src, cv::Mat& dst, int min_tresh, int max_tresh, int aperture)
{
	Mat src_gray;
	// Convert image to gray and blur it
	cvtColor(src, src_gray, CV_BGR2GRAY);
	blur(src_gray, src_gray, Size(3, 3));
	Canny(src_gray, dst, min_tresh, max_tresh, aperture);
}


void LinesDetector(const cv::Mat& src, std::vector<cv::Vec4i>& lines)
{
}

void LinesToDocs(const std::vector<cv::Vec4i>& lines, std::vector<int>& docs)
{
}

void DrawLines(const cv::Mat& src, const std::vector<cv::Vec4i>& lines, cv::Mat& dst)
{
}

void DrawDocShape(const cv::Mat& src, const std::vector<int>& docs, cv::Mat& dst)
{
}
