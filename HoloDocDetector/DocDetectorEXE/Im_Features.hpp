#pragma once

#include <opencv2/core.hpp>

class Im_Features
{
public:
	cv::Mat _Src, _Gray, _HSV, _Quantized;

	const int _HistoBins = 10;
	std::vector<double> _Histogram;
	std::vector<double> _Stats;

	Im_Features();
	virtual ~Im_Features();

	void setImage(const cv::Mat &image);
	void ExtractFeatures();
	void ExtractFeatures(const cv::Mat &image);

	friend std::ostream &operator <<(std::ostream &os, const Im_Features &obj);

private:
	void mixChannels(const cv::Mat &in, cv::Mat &out, std::vector<double> alpha) const;
	void colorQuantization(const int k = 5);
	void findDominantColor();
};

