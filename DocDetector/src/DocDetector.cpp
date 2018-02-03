#include "DocDetector.hpp"
#include <opencv2/imgproc.hpp>

using namespace std;
using namespace cv;

//********************************
//********** Unity Link **********
//********************************
void DocDetector(/*const image unity en entrée& in, */std::vector<int>& out)
{
	Mat src, EdgeDetect;
	vector<Vec4i> lines;
	vector<Vec8i> docs;
	UnityToOpenCVMat(/*in,*/ src);
	BinaryEdgeDetector(src, EdgeDetect);
	LinesDetector(EdgeDetect, lines);
	LinesToDocs(lines, docs);
	DocsToUnity(docs, out);
}

//******************************
//********** Computes **********
//******************************
void UnityToOpenCVMat(/*const blabla& src,*/ cv::Mat& dst)
{
	
}


void DocsToUnity(std::vector<cv::Vec8i> &docs, std::vector<int> &dst)
{
	
}

void BinaryEdgeDetector(const cv::Mat& src, cv::Mat& dst, const int min_tresh, const int max_tresh, const int aperture)
{
	assert(!src.empty() && (src.type() == CV_8UC1 || src.type() == CV_8UC3));
	Mat gray;
	if (src.type() == CV_8UC3) {
		// Convert image to gray and blur it
		cvtColor(src, gray, CV_BGR2GRAY);
	}
	else	gray = src.clone();

	blur(gray, gray, Size(3, 3));
	Canny(gray, dst, min_tresh, max_tresh, aperture);
}


void LinesDetector(const cv::Mat & src, std::vector<cv::Vec4i>& lines,
                   const double rho, const double theta, const int threshold,
                   const double minLineLength, const double maxLineGap)
{
	assert(!src.empty() && src.type() == CV_8UC1);
	// Hough LineP Method (create segment)
	HoughLinesP(src, lines, rho, theta, threshold, minLineLength, maxLineGap);
	// Hough Line Method (create lines) only for test 
	/*
	vector<Vec2f> l;
	HoughLines(src, l, rho, theta, threshold);
	lines.clear();
	for (size_t i = 0; i < l.size(); ++i)
	{
		const float r = l[i][0], t = l[i][1];
		const double a = cos(t), b = sin(t),
					 x0 = a * r, y0 = b * r;
		lines.emplace_back(cvRound(x0 + 1000 * (-b)), cvRound(y0 + 1000 * (a)), 
			cvRound(x0 - 1000 * (-b)), cvRound(y0 - 1000 * (a)));
	}
	*/
}

void LinesToDocs(const std::vector<cv::Vec4i>& lines, std::vector<cv::Vec8i>& docs)
{
	// Step 1 Merging of the collinear segments
	// Step 2 Transformation of segments into lines 
	//			(to create intersections especially on segments not going to the corners of documents)
	// Step 3 Locate the perpendicular intersection (with a threshold)
	// Step 4 find a document satisfying the following conditions: 4 lines forming a rectangle 
	//			(4 perpendicular corners and adjacent side not necessarily of the same size)
}

//******************************
//********** Drawings **********
//******************************
const vector<cv::Scalar> COLORS = {
	cv::Scalar(0, 0, 0), cv::Scalar(125, 125, 125), cv::Scalar(255, 255, 255),	// Noir		Gris		Blanc
	cv::Scalar(255, 0, 0), cv::Scalar(0, 255, 0), cv::Scalar(0, 0, 255),		// Rouge	Vert		Bleu
	cv::Scalar(0, 255, 255), cv::Scalar(255, 0, 255), cv::Scalar(255, 255, 0),	// Cyan		Magenta		Jaune
	cv::Scalar(255, 125, 0), cv::Scalar(0, 255, 125), cv::Scalar(125, 0, 255),	// Orange	Turquoise	Indigo
	cv::Scalar(255, 0, 125), cv::Scalar(125, 255, 0), cv::Scalar(0, 125, 255),	// Fushia	Lime		Azur
	cv::Scalar(125, 0, 0), cv::Scalar(0, 125, 0), cv::Scalar(0, 0, 125)			// Blood	Grass		Deep
};

void DrawLines(const cv::Mat& src, cv::Mat& dst, const std::vector<cv::Vec4i>& lines)
{
	assert(!src.empty() && src.type() == CV_8UC3);
	dst = src.clone();

	for (size_t i = 0; i < lines.size(); ++i)
	{
		line(dst, Point(lines[i][0], lines[i][1]), Point(lines[i][2], lines[i][3]), COLORS[i%(COLORS.size()-4)+3], 3);
	}
}

void DrawDocShape(const cv::Mat& src, cv::Mat& dst, const std::vector<cv::Vec8i>& docs)
{
	assert(!src.empty() && src.type() == CV_8UC3);
	dst = src.clone();

	for (size_t i = 0; i < docs.size(); ++i)
	{
		const auto col = COLORS[i % (COLORS.size() - 4) + 3];
		line(dst, Point(docs[i][0], docs[i][1]), Point(docs[i][2], docs[i][3]), col, 3);
		line(dst, Point(docs[i][2], docs[i][3]), Point(docs[i][4], docs[i][5]), col, 3);
		line(dst, Point(docs[i][4], docs[i][5]), Point(docs[i][6], docs[i][7]), col, 3);
		line(dst, Point(docs[i][6], docs[i][7]), Point(docs[i][0], docs[i][1]), col, 3);
	}
}
