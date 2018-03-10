#pragma once

#include "opencv2/core.hpp"
#include <vector>

//Basic fast clean only number of point and length
//Possibility to change arclength by boundingrect maybe some contour can be suppress in some case but its a little slower
void CleanBasic(const std::vector<std::vector<cv::Point>> &in, std::vector<std::vector<cv::Point>> &out,
				double min_length = 600, double max_length = 3600);

void Hulls(const std::vector<std::vector<cv::Point>> &in, std::vector<std::vector<cv::Point>> &out,
		   double min_length = 600, double max_length = 3600);

void Approxs(const std::vector<std::vector<cv::Point>> &in, std::vector<std::vector<cv::Point>> &out,
			 double min_length = 600, double max_length = 3600);

void Rects(const std::vector<std::vector<cv::Point>> &in, std::vector<std::vector<cv::Point>> &out,
		   double min_length = 600, double max_length = 3600);

void Extract4Corners(const std::vector<std::vector<cv::Point>> &in, std::vector<std::vector<cv::Point>> &out,
					 double min_length = 600, double max_length = 3600);

void FinalClean(const std::vector<std::vector<cv::Point>> &in, std::vector<std::vector<cv::Point>> &out,
				double min_length = 600, double max_length = 3600,
				double min_center_dist = 110, double side_ratio = 0.5);
