#pragma once

#include "opencv2/core.hpp"

#include <vector>

const std::vector<cv::Scalar> COLORS = {
	cv::Scalar(0, 0, 0), cv::Scalar(125, 125, 125), cv::Scalar(255, 255, 255),	// Noir		Gris		Blanc
	cv::Scalar(255, 0, 0), cv::Scalar(0, 255, 0), cv::Scalar(0, 0, 255),		// Rouge	Vert		Bleu
	cv::Scalar(0, 255, 255), cv::Scalar(255, 0, 255), cv::Scalar(255, 255, 0),	// Cyan		Magenta		Jaune
	cv::Scalar(255, 125, 0), cv::Scalar(0, 255, 125), cv::Scalar(125, 0, 255),	// Orange	Turquoise	Indigo
	cv::Scalar(255, 0, 125), cv::Scalar(125, 255, 0), cv::Scalar(0, 125, 255),	// Fushia	Lime		Azur
	cv::Scalar(125, 0, 0), cv::Scalar(0, 125, 0), cv::Scalar(0, 0, 125)			// Blood	Grass		Deep
};

//**************************
//***** DRAW FONCTIONS *****
//**************************
void DrawBinaryLines(const cv::Size &s, cv::Mat &dst, const std::vector<cv::Vec4i> &lines);
void DrawCont(const cv::Mat &src, cv::Mat &dst, const std::vector<std::vector<cv::Point>> &contours, bool fill = false);

//***************************
//***** PRINT FONCTIONS *****
//***************************
void printContour(const std::vector<std::vector<cv::Point>> &contours);
void printContourSize(const std::vector<std::vector<cv::Point>> &contours);

//**************************
//***** DIST FUNCTIONS *****
//**************************
cv::Point GetCenter(const std::vector<cv::Point> &v);
int SquaredDist(const cv::Point &p);
int SquaredDist(const cv::Point &p1, const cv::Point &p2);
double Dist(const cv::Point &p);
double Dist(const cv::Point &p1, const cv::Point &p2);
bool SortArea(const std::vector<cv::Point> &a, const std::vector<cv::Point> &b);
bool inQuad(const std::vector<cv::Point> &quad, const cv::Point &point);