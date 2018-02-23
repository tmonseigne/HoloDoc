#include "pch.h"
#include "DocDetector.hpp"

#include <opencv2/imgproc.hpp>

// Meant to disapear (used for simple document detection just to be able to print)
#include <opencv2/highgui.hpp>
#include <opencv2/core.hpp>

// Meant to disapear (used to compute perfomance)
#include <ctime>

using namespace std;
using namespace cv;


//************************************
//********** Miscs Computes **********
//************************************
static bool sortArea(const vector<Point> &a, const vector<Point> &b)
{
	return (contourArea(a) > contourArea(b));
}

static bool sortPointsX(const Point &a, const Point &b)
{
	return (a.x < b.x);
}

static bool sortPointsY(const Point &a, const Point &b)
{
	return (a.y > b.y);
}

//********************************
//********** Unity Link **********
//********************************

extern "C" int __declspec(dllexport) __stdcall DocumentDetection(uint width, uint height, Color32* image,
	uint maxDocumentsCount, uint* outDocumentsCount, int* outDocumentsCorners) {
	Mat src, edgeDetect;
	vector<Vec4i> lines;
	vector<Vec8i> docs;
	int errCode = UnityToOpenCVMat(image, height, width, src);
	if (errCode != NO_ERRORS) return errCode;

	errCode = BinaryEdgeDetector(src, edgeDetect);
	if (errCode != NO_ERRORS) return errCode;
	errCode = SegmentsDetector(edgeDetect, lines);
	if (errCode != NO_ERRORS) return errCode;
	errCode = LinesToDocs(lines, docs);
	if (errCode != NO_ERRORS) return errCode;

	// Unity already pre allocated the memory of outDocumentCorners to be maxDocumentCount * 8 * sizeof(int)
	errCode = DocsToUnity(docs, outDocumentsCorners, maxDocumentsCount, *outDocumentsCount);
	return errCode;
}

extern "C" int __declspec(dllexport) __stdcall SimpleDocumentDetection(Color32* image, uint width, uint height, byte* result, uint maxDocumentsCount, uint* outDocumentsCount, int* outDocumentsCorners)
{
	//const std::clock_t start = std::clock();

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
	vector<Vec8i> docsPoints;
	for (const vector<Point> &contour : contours) {
		const double peri = arcLength(contour, true);
		approxPolyDP(contour, approx, 0.02 * peri, true);
		if (approx.size() == 4) {
			// Sorting points
			std::sort(approx.begin(), approx.end(), sortPointsX);
			std::sort(approx.begin() + 1, approx.end() - 1, sortPointsY);

			Vec8i points = { approx.at(0).x, (int)height - approx.at(0).y,
				approx.at(1).x, (int)height - approx.at(1).y,
				approx.at(3).x, (int)height - approx.at(3).y,
				approx.at(2).x, (int)height - approx.at(2).y };

			docsPoints.emplace_back(points);

			if (docsPoints.size() == maxDocumentsCount) {
				break;
			}

			viableContoursIndexes.push_back(index);
		}
		index++;
	}

	//const double duration = (std::clock() - start) / double(CLOCKS_PER_SEC);

	for (int viableContoursIndex : viableContoursIndexes) {
		drawContours(src, contours, viableContoursIndex, Scalar(0, 255, 0), 2);
	}

	*outDocumentsCount = docsPoints.size();

	OpenCVMatToUnity(src, result);

	int errCode = DocsToUnity(docsPoints, outDocumentsCorners, maxDocumentsCount, *outDocumentsCount);
	return errCode;
}

//******************************
//********** Computes **********
//******************************
int UnityToOpenCVMat(Color32* image, uint height, uint width, Mat& dst)
{
	dst = Mat(height, width, CV_8UC4, image);
	if (dst.empty())	return EMPTY_MAT;

	cvtColor(dst, dst, CV_RGBA2BGR);
	return NO_ERRORS;
}


int OpenCVMatToUnity(const Mat& input, byte* output)
{
	if (input.empty())	return EMPTY_MAT;
	Mat tmp;

	cvtColor(input, tmp, CV_BGR2RGB);

	memcpy(output, tmp.data, tmp.rows * tmp.cols * 3);
	return NO_ERRORS;
}

int DocsToUnity(std::vector<Vec8i> &docs, int* dst, uint maxDocumentsCount, uint& nbDocuments)
{
	if (docs.empty())	return NO_DOCS;

	//nbDocuments = std::min(maxDocumentsCount, (uint) docs.size());

	uint index = 0;
	for (uint i = 0; i < nbDocuments; i++) {
		Vec8i doc = docs[i];
		for (uint j = 0; j < 8; j++) {
			dst[index++] = doc[j];
		}
	}

	return NO_ERRORS;
}

int BinaryEdgeDetector(const Mat& src, Mat& dst, const int min_tresh, const int max_tresh, const int aperture)
{
	if (src.empty())	return EMPTY_MAT;
	if (src.type() != CV_8UC1 && src.type() != CV_8UC3)	return TYPE_MAT;
	Mat gray;
	if (src.type() == CV_8UC3) {
		// Convert image to gray and blur it
		cvtColor(src, gray, CV_BGR2GRAY);
	}
	else	gray = src.clone();

	blur(gray, gray, Size(3, 3));
	Canny(gray, dst, min_tresh, max_tresh, aperture);
	return NO_ERRORS;
}

int SegmentsDetector(const Mat& src, vector<Vec4i>& lines,
	const double rho, const double theta, const int threshold,
	const double minLineLength, const double maxLineGap)
{
	if (src.empty())	return EMPTY_MAT;
	if (src.type() != CV_8UC1)	return TYPE_MAT;
	// Hough LineP Method (create segment)
	HoughLinesP(src, lines, rho, theta, threshold, minLineLength, maxLineGap);
	return NO_ERRORS;
}

int LinesDetector(const Mat& src, vector<Vec4i>& lines,
	const double rho, const double theta, const int threshold)
{
	if (src.empty())	return EMPTY_MAT;
	if (src.type() != CV_8UC1)	return TYPE_MAT;
	// Hough Line Method (create lines) only for test 
	vector<Vec2f> line;
	HoughLines(src, line, rho, theta, threshold);
	lines.clear();
	for (auto &l : line) {
		const float r = l[0], t = l[1];
		const double a = cos(t), b = sin(t),
			x0 = a * r, y0 = b * r;
		lines.emplace_back(cvRound(x0 + 1000 * (-b)), cvRound(y0 + 1000 * (a)),
			cvRound(x0 - 1000 * (-b)), cvRound(y0 - 1000 * (a)));
	}
	return NO_ERRORS;
}

int LinesToDocs(const vector<Vec4i>& lines, vector<Vec8i>& docs)
{
	if (lines.empty())	return NO_LINES;
	// Temporary line to have something to get inside Unity
	docs.emplace_back(0, 0, 100, 0, 0, 100, 100, 100);

	// Step 1 Merging of the collinear segments
	// Step 2 Transformation of segments into lines 
	//			(to create intersections especially on segments not going to the corners of documents)
	// Step 3 Locate the perpendicular intersection (with a threshold)
	// Step 4 find a document satisfying the following conditions: 4 lines forming a rectangle 
	//			(4 perpendicular corners and adjacent side not necessarily of the same size)
	return NO_ERRORS;
}

//******************************
//********** Drawings **********
//******************************
const vector<Scalar> COLORS = {
	Scalar(0,   0,   0), Scalar(125, 125, 125), Scalar(255, 255, 255),	// Noir		Gris		Blanc
	Scalar(255,   0,   0), Scalar(0, 255,   0), Scalar(0,   0, 255),	// Rouge	Vert		Bleu
	Scalar(0, 255, 255), Scalar(255,   0, 255), Scalar(255, 255,   0),	// Cyan		Magenta		Jaune
	Scalar(255, 125,   0), Scalar(0, 255, 125), Scalar(125,   0, 255),	// Orange	Turquoise	Indigo
	Scalar(255,   0, 125), Scalar(125, 255,   0), Scalar(0, 125, 255),	// Fushia	Lime		Azur
	Scalar(125,   0,   0), Scalar(0, 125,   0), Scalar(0,   0, 125)		// Blood	Grass		Deep
};

int DrawLines(const Mat& src, Mat& dst, const vector<Vec4i>& lines)
{
	if (src.empty())	return EMPTY_MAT;
	if (src.type() != CV_8UC3)	return TYPE_MAT;
	dst = src.clone();

	for (size_t i = 0; i < lines.size(); ++i) {
		line(dst, Point(lines[i][0], lines[i][1]), Point(lines[i][2], lines[i][3]), COLORS[i % (COLORS.size() - 4) + 3], 3);
	}
	return NO_ERRORS;
}

int DrawDocShape(const Mat& src, Mat& dst, const vector<Vec8i>& docs)
{
	if (src.empty())	return EMPTY_MAT;
	if (src.type() != CV_8UC3)	return TYPE_MAT;

	dst = src.clone();

	for (size_t i = 0; i < docs.size(); ++i) {
		const auto col = COLORS[i % (COLORS.size() - 4) + 3];
		line(dst, Point(docs[i][0], docs[i][1]), Point(docs[i][2], docs[i][3]), col, 3);
		line(dst, Point(docs[i][2], docs[i][3]), Point(docs[i][4], docs[i][5]), col, 3);
		line(dst, Point(docs[i][4], docs[i][5]), Point(docs[i][6], docs[i][7]), col, 3);
		line(dst, Point(docs[i][6], docs[i][7]), Point(docs[i][0], docs[i][1]), col, 3);
	}
	return NO_ERRORS;
}

