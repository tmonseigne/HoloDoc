#pragma once

#include <opencv2/core.hpp>

class Im_Features
{
public:
	const int _HistoChans = 6;
	int _HistoBins,
		_HOGBins;
	std::vector<double> _Histograms;
	std::vector<double> _HOG;

	explicit Im_Features(int histo_bins = 10, int hog_bins = 10);
	virtual ~Im_Features();

	void ExtractFeatures(const cv::Mat &image);
	double Distance(const Im_Features &features, std::vector<double> coefs = {0,2,1,2,1,1,1});

	void ToCSV(const std::string &filename);
	friend std::ostream &operator <<(std::ostream &os, const Im_Features &obj);

private:
	void extractHistograms(const cv::Mat &image);
	void extractHOG(const cv::Mat &image);
};

