#include "Misc.hpp"
#include <opencv2/highgui.hpp>
#include <opencv2/imgproc.hpp>
#include <iostream>
#include <set>

using namespace std;
using namespace cv;

//**************************
//***** DRAW FONCTIONS *****
//**************************
void DrawBinaryLines(const Size &s, Mat &dst, const vector<Vec4i> &lines)
{
	dst = Mat::zeros(s, CV_8UC1);
	for (const auto &l : lines) {
		line(dst, Point(l[0], l[1]), Point(l[2], l[3]), COLORS[2], 1);
	}
}

void DrawCont(const Mat &src, Mat &dst, const vector<vector<Point>> &contours, const bool fill)
{
	//dst = fill ? Mat::zeros(src.size(), CV_8UC3) : src.clone();
	dst = src.clone();
	for (int i = 0; i < contours.size(); i++) {
		const Scalar color = COLORS[i % (COLORS.size() - 4) + 3];
		const int thickness = fill ? -1 : 5;
		drawContours(dst, contours, i, color, thickness);
		if (fill) {
			addWeighted(src, 0.05, dst, 0.95, 0, dst);
		}

	}
}


//***************************
//***** PRINT FONCTIONS *****
//***************************
void printContour(const vector<vector<Point>> &contours)
{
	if (!contours.empty()) {
		for (int i = 0; i < contours.size(); ++i) {
			cout << "Contour " << i << ", " << contours[i].size() << " points : [";
			for (auto &p : contours[i]) {
				cout << "(" << p.x << ", " << p.y << ")\t";
			}
			cout << "]" << endl;
		}
	}
}

void printContourSize(const vector<vector<Point>> &contours)
{
	if (!contours.empty()) {
		cout << "[" << contours[0].size();
		for (int i = 1; i < contours.size(); ++i) {
			cout << ", " << contours[i].size();
		}
		cout << "]";
	}
}

//**************************
//***** DIST FUNCTIONS *****
//**************************
Point GetCenter(const vector<Point> &v)
{
	const int Nb_points = int(v.size());
	Point Center(0, 0);
	for (const Point &p : v) {
		Center += p;
	}
	Center.x /= Nb_points;
	Center.y /= Nb_points;
	return Center;
}

int SquaredDist(const Point &p) { return p.x * p.x + p.y * p.y; }
int SquaredDist(const Point &p1, const Point &p2) { return SquaredDist(p2 - p1); }
double Dist(const Point &p) { return sqrt(SquaredDist(p)); }
double Dist(const Point &p1, const Point &p2) { return sqrt(SquaredDist(p1, p2)); }

bool SortArea(const vector<Point> &a, const vector<Point> &b)
{
	return contourArea(a) > contourArea(b);
}

bool inQuad(const vector<Point> &quad, const Point &point)
{
	Point V = quad[3] - quad[0];
	int cross[2] = { 0, 0 };
	for (int i = 0; i < 3; ++i) {
		const int sign = V.x * point.y - V.y * point.x >= 0 ? 1 : 0;
		cross[sign]++;
		V = quad[i + 1] - quad[i];
	}
	return cross[0] == 0 || cross[1] == 0;
}
