#include <iostream>
#include <stdlib.h>
#include <conio.h>	// _getch
#include <opencv2/highgui.hpp>
#include <opencv2/core.hpp>
#include "DocDetector.hpp"
#include <opencv2/imgproc.hpp>
#include <ctime>

using namespace std;
using namespace cv;

const string PATH = "../res/";
const string NAME = "A";
const vector<string> NAMES = { "A","B","C","D","E","F","G" };
const string EXT = ".jpg";

const vector<Scalar> COLORS = {
	Scalar(0, 0, 0), Scalar(125, 125, 125), Scalar(255, 255, 255),	// Noir		Gris		Blanc
	Scalar(255, 0, 0), Scalar(0, 255, 0), Scalar(0, 0, 255),		// Rouge	Vert		Bleu
	Scalar(0, 255, 255), Scalar(255, 0, 255), Scalar(255, 255, 0),	// Cyan		Magenta		Jaune
	Scalar(255, 125, 0), Scalar(0, 255, 125), Scalar(125, 0, 255),	// Orange	Turquoise	Indigo
	Scalar(255, 0, 125), Scalar(125, 255, 0), Scalar(0, 125, 255),	// Fushia	Lime		Azur
	Scalar(125, 0, 0), Scalar(0, 125, 0), Scalar(0, 0, 125)			// Blood	Grass		Deep
};

void DrawBinaryLines(const Size &s, Mat & dst, const vector<Vec4i> &lines)
{
	dst = Mat::zeros(s, CV_8UC1);
	for (const auto &l : lines) {
		line(dst, Point(l[0], l[1]), Point(l[2], l[3]), COLORS[2], 1);
	}
}

void DrawCont(const Mat &src, Mat &dst, const vector<vector<Point>> &contours)
{
	dst = src.clone();
	for (int i = 0; i < contours.size(); i++)
	{
		const Scalar color = COLORS[i % (COLORS.size() - 4) + 3];
		drawContours(dst, contours, i, color, 2);
	}
}

void printContour(const vector<vector<Point>> &contours)
{
	for (int i = 0; i < contours.size(); ++i) {
		cout << "Contour "<<i<<", "<<contours[i].size()<<" points : [";
		for (auto &p : contours[i]) {
			cout << "("<< p.x << ", "<<p.y<<")\t";
		}
		cout << "]" << endl;
	}
}

// The first method use ApproxPoly with minimum perimeter
void CleanContour(	const vector<vector<Point>> &in, vector<vector<Point>> &out, 
					const double threshold = 200, const int method = 0)
{
	out.clear();
	for (const vector<Point> &contour : in) {
		const double peri = arcLength(contour, true);
		if (peri > threshold && contour.size() >= 4) {
			vector<Point> approx;
			switch (method) {
			case 0: default: { //Approx Poly		
				const double epsilon = 0.02 * arcLength(contour, true);
				approxPolyDP(contour, approx, epsilon, true);
				break;
			}
			case 1: {//Convex Hull		
				convexHull(contour, approx);
				break;
			}
			case 2: { //Bounding	
				RotatedRect box = minAreaRect(contour);
				Point2f vertices[4];
				box.points(vertices);
				for (auto &vertice : vertices)
					approx.emplace_back(cvRound(vertice.x), cvRound(vertice.y));
				break;
			}
			}
			// This is the maximum distance between the original curve and its approximation.
			const double peri2 = arcLength(approx, true);
			if (peri2 > threshold && approx.size() >= 4 && approx.size() <= 8) {
				out.push_back(approx);
				cout << "Old perimeter : " << cvRound(peri) << ",\t" << contour.size() << " points "
					<< "\t New perimeter : " << cvRound(peri2) << ",\t" << approx.size() << " points " << endl;
			}
		}
	}
	cout << endl << endl << "CONTOURS : " << endl;
	printContour(out);
}

// The first method is to approximate a polygon, convex hull or bounding rotated Rect
// from the contours detected directly from Canny
void Method1(const Mat &edge, vector<vector<Point>> &contours,
             const bool approxPoly = true, const bool convexHull = false, const bool bounding = false)
{
	cout << endl << "Method 1 : " << endl;
	const std::clock_t start = std::clock();
	double t_total = 0.0;

	//Contours
	findContours(edge, contours, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE);
	const double t_contours = (std::clock() - start) / double(CLOCKS_PER_SEC) - t_total;
	t_total += t_contours;
	cout << "\tTime Find Contour : " << t_contours << "\tTotal Time : " << t_total << endl;

	const double length_threshold = 0.25 * (edge.cols + edge.rows);
	cout << "\t\tLength threshold = " << length_threshold << endl;

	vector<vector<Point>> contours2;
	if (approxPoly) {
		CleanContour(contours, contours2, length_threshold, 0);
		const double t_clean = (std::clock() - start) / double(CLOCKS_PER_SEC) - t_total;
		t_total += t_clean;
		cout << "\tTime Clean Contour approx : " << t_clean << "\tTotal Time : " << t_total << endl;
		contours = contours2;
	}
	if (convexHull) {
		CleanContour(contours, contours2, length_threshold, 1);
		const double t_clean = (std::clock() - start) / double(CLOCKS_PER_SEC) - t_total;
		t_total += t_clean;
		cout << "\tTime Clean Contour Hull : " << t_clean << "\tTotal Time : " << t_total << endl;
		contours = contours2;
	}

	if (bounding) {
		CleanContour(contours, contours2, length_threshold, 2);
		const double t_clean = (std::clock() - start) / double(CLOCKS_PER_SEC) - t_total;
		t_total += t_clean;
		cout << "\tTime Clean Contour Bounding : " << t_clean << "\tTotal Time : " << t_total << endl;
		contours = contours2;
	}
}

// Tthe second method is to retrieve the lines (segments) of the image 
// Then find the rectangles present with these segments.
void Method2(const Mat &edge, Mat &binlines, vector<Vec4i> &lines, vector<vector<Point>> &contours)
{
	cout << endl << "Method 2 : " << endl;
	const std::clock_t start = std::clock();
	double t_total = 0.0;

	// Segment
	SegmentsDetector(edge, lines);
	const double t_lines = (std::clock() - start) / double(CLOCKS_PER_SEC) - t_total;
	t_total += t_lines;
	cout << "\tTime Lines : " << t_lines << "\t\tTotal Time : " << t_total << endl;

	//Draw Lines
	DrawBinaryLines(edge.size(), binlines, lines);
	const double t_draw_lines = (std::clock() - start) / double(CLOCKS_PER_SEC) - t_total;
	t_total += t_draw_lines;
	cout << "\tTime Draw Lines : " << t_draw_lines << "\t\tTotal Time : " << t_total << endl;

	//Contours doesn't work with segment 
	//WIth Hough and convex Hull we can detect only one document (the biggest combinaison of segements)
}

void Protocole(int i, bool show = true, bool save = true)
{
	cout << "Test " << i << " Begin..." << endl;
	const std::clock_t start = std::clock();
	double t_total = 0.0;
	const Mat src = imread(PATH + NAMES[i] + EXT, CV_LOAD_IMAGE_COLOR);
	if (src.cols == 0 || src.rows == 0)	return;
	const double t_read = (std::clock() - start) / double(CLOCKS_PER_SEC);
	t_total += t_read;
	cout << "Time Read : " << t_read << "\t\tTotal Time : " << t_total << endl;
	//***** Computes *****
	Mat edge, contdraws;
	//Mat binlines, linesdraw, contdraws2;
	//vector<Vec4i> lines;
	vector<vector<Point>> contours, contours2;

	//Edge
	BinaryEdgeDetector(src, edge);
	const double t_edge = (std::clock() - start) / double(CLOCKS_PER_SEC) - t_total;
	t_total += t_edge;
	cout << "Time Edge : " << t_edge << "\t\tTotal Time : " << t_total << endl;

	Method1(edge, contours, true, false, false);
	//Method2(edge, binlines, lines, contours2);

	//***** Draws *****
	DrawCont(src, contdraws, contours);
	//DrawLines(src, linesdraw, lines);
	//DrawCont(src, contdraws2, contours2);

	cout << "Test " << i << " End..." << endl << endl;

	//***** Show *****
	if (show) {
		cvNamedWindow("Src", CV_WINDOW_AUTOSIZE);
		imshow("Src", src);

		cvNamedWindow("Edge", CV_WINDOW_AUTOSIZE);
		imshow("Edge", edge);

		cvNamedWindow("Contours", CV_WINDOW_AUTOSIZE);
		imshow("Contours", contdraws);

		//cvNamedWindow("Binary Lines", CV_WINDOW_AUTOSIZE);
		//imshow("Binary Lines", binlines);

		//cvNamedWindow("Lines", CV_WINDOW_AUTOSIZE);
		//imshow("Lines", linesdraw);

		//cvNamedWindow("Contours 2", CV_WINDOW_AUTOSIZE);
		//imshow("Contours 2", contdraws2);
	}

	//**** Save *****
	if (save) {
		imwrite(PATH + NAMES[i] + "_Edges" + EXT, edge);
		imwrite(PATH + NAMES[i] + "_Contours" + EXT, contdraws);
		//imwrite(PATH + NAMES[i] + "_Lines" + EXT, linesdraw);
		//imwrite(PATH + NAMES[i] + "_Binary_Lines" + EXT, binlines);
		//imwrite(PATH + NAMES[i] + "_Contours_2" + EXT, contdraws2);
	}
	if (show) {
		cvWaitKey(0);			// Break the time the user presses a key
	}

	//TEST adaptative threshold
	Mat gray, adaptive;
	cvtColor(src, gray, CV_BGR2GRAY);
	adaptiveThreshold(gray, adaptive, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY, 5, 4);
	imwrite(PATH + NAMES[i] + "_Adaptative_Threshold" + EXT, adaptive);
}

int main(int argc, char *argv[])
{
	//***** Init *****
	(void)argc;
	(void)argv;
	for(int i = 0; i < NAMES.size(); ++i)
		Protocole(i, false, true);

	cvDestroyAllWindows();	// Destruction of windows
	_getch();
	return EXIT_SUCCESS;
}
