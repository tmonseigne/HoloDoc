#include "DocDetector.hpp"
#include <opencv2/imgproc.hpp>

// Meant to disapear (used for simple document detection just to be able to print)
#include <opencv2/highgui.hpp>
#include <opencv2/core.hpp>

// Meant to disapear (used to compute perfomance)
#include <ctime>

using namespace std;
using namespace cv;
//********************************
//********** Unity Link **********
//********************************

extern "C" int __declspec(dllexport) __stdcall DocumentDetection(uint width, uint height, Color32* image, uint maxDocumentsCount, uint* outDocumentsCount, int* outDocumentsCorners) {
	Mat src, edgeDetect;
	vector<Vec4i> lines;
	vector<Vec8i> docs;
	int errCode = UnityToOpenCVMat(image, height, width, src);
	if (errCode) {
		return 1;
	}
	BinaryEdgeDetector(src, edgeDetect);
	LinesDetector(edgeDetect, lines);
	LinesToDocs(lines, docs);

	// Unity already pre allocated the memory of outDocumentCorners to be maxDocumentCount * 8 * sizeof(int)
	DocsToUnity(docs, outDocumentsCorners, maxDocumentsCount, *outDocumentsCount);
	return 1;
}


static bool sortArea(vector<Point> a, vector<Point> b) {
	return (contourArea(a) > contourArea(b));
}

extern "C" double __declspec(dllexport) __stdcall SimpleDocumentDetection(Color32* image, uint width, uint height, byte* result) {

	// Timing things just to test perfomances really quick
	double duration;
	std::clock_t start;
	start = std::clock();

	Mat src, edgeDetect;
	UnityToOpenCVMat(image, height, width, src);
	BinaryEdgeDetector(src, edgeDetect);

	vector<vector<Point>> contours;
	findContours(edgeDetect, contours, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE);

	// Sorting the contour depending on their area
	std::sort(contours.begin(), contours.end(), sortArea);

	int index = 0;
	vector<int> viableContoursIndexes;
	vector<Point> approx;
	for (vector<Point> contour : contours) {
		double peri = arcLength(contour, true);
		approxPolyDP(contour, approx, 0.02 * peri, true);
		if (approx.size() >= 4) {
			viableContoursIndexes.push_back(index);
			// TODO: Remove magic number : nb of document detected
			if (viableContoursIndexes.size() == 10) {
				break;
			}
		}
		index++;
	}

	duration = (std::clock() - start) / (double)CLOCKS_PER_SEC;

	for (int i = 0; i < viableContoursIndexes.size(); i++) {
		drawContours(src, contours, viableContoursIndexes[i], Scalar(0, 255, 0), 2);
	}

	// Useless display (just to check if it worked)
	//cvNamedWindow("DocFound", CV_WINDOW_AUTOSIZE);
	//imshow("DocFound", src);
	//cvWaitKey(0);
	//cvDestroyAllWindows();

	OpenCVMatToUnity(src, result);

	return duration;
}

//******************************
//********** Computes **********
//******************************
int UnityToOpenCVMat(Color32* image, uint height, uint width, cv::Mat& dst)
{
	dst = Mat(height, width, CV_8UC4, image);
	if (dst.empty()) {
		return 1;
	}

	// Unity sends reversed image so we need to flip it around the x axis.
	flip(dst, dst, 0);
	if (dst.empty()) {
		return 1;
	}

	cvtColor(dst, dst, CV_RGBA2BGR);
	return 0;
}


int OpenCVMatToUnity(cv::Mat input, byte* output) 
{

	flip(input, input, 0);
	if (input.empty()) {
		return 1;
	}

	cvtColor(input, input, CV_BGR2RGB);

	memcpy(output, input.data, input.rows * input.cols * 3);
}

void DocsToUnity(std::vector<cv::Vec8i> &docs, int* dst, uint maxDocumentsCount, uint& nbDocuments)
{
	nbDocuments = std::min(maxDocumentsCount, (uint) docs.size());

	// If no docs are found, avoid useless computations
	if (nbDocuments == 0) {
		return;
	}

	uint index = 0;
	for (uint i = 0; i < nbDocuments; i++) {
		cv::Vec8i doc = docs[i];
		for (uint j = 0; j < 8; j++) {
			dst[index++] = doc[j];
		}
	}
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
	// Temporary line to have something to get inside Unity
	docs.push_back(Vec8i(0, 0, 100, 0, 0, 100, 100, 100));

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

