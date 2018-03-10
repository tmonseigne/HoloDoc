#include "Im_Features.hpp"
#include <opencv2/imgproc.hpp>
#include <iostream>
#include <vector>

using namespace std;
using namespace cv;

string Type2Str(const int type)
{
	string r;

	uchar depth = type & CV_MAT_DEPTH_MASK,
		chans = uchar(1 + (type >> CV_CN_SHIFT));

	switch (depth) {
	case CV_8U: r = "8U";		break;
	case CV_8S: r = "8S";		break;
	case CV_16U: r = "16U";		break;
	case CV_16S: r = "16S";		break;
	case CV_32S: r = "32S";		break;
	case CV_32F: r = "32F";		break;
	case CV_64F: r = "64F";		break;
	default: r = "User";		break;
	}

	r += "C";
	r += chans + '0';

	return r;
}


Im_Features::Im_Features(){}

Im_Features::~Im_Features(){}

void Im_Features::setImage(const cv::Mat &image)
{
	_Src = image.clone();
	Mat HSV;
	cvtColor(_Src, _Gray, COLOR_BGR2GRAY);
	cvtColor(_Src, _HSV, COLOR_BGR2HSV);
	colorQuantization(5);
	findDominantColor();
}

void Im_Features::ExtractFeatures()
{
	const double nb_points = _Src.rows * _Src.cols;
	float range[] = { 0, 256 };
	const float* histRange = { range };
	Mat hist;
	calcHist(&_Gray, 1, nullptr, Mat(), hist, 1, &_HistoBins, &histRange);
	cout << "Type : " << Type2Str(hist.type()) << "\tNb Elem : " << hist.rows << "\tNb Points : " << nb_points<< endl;
	_Histogram.clear();
	_Histogram.reserve(_HistoBins);
	for (auto it = hist.begin<float>(); it != hist.end<float>(); ++it) {
		_Histogram.push_back((*it) / nb_points);
	}

	Scalar SrcMean, SrcSigma, HSVMean, HSVSigma;
	meanStdDev(_Src, SrcMean, SrcSigma);
	meanStdDev(_HSV, HSVMean, HSVSigma);
	if (_Stats.size() != 12)	_Stats.resize(12);
	for (int i = 0; i < 3; ++i) {
		_Stats[i] = SrcMean[i];
		_Stats[i+3] = SrcSigma[i];
		_Stats[i+6] = HSVMean[i];
		_Stats[i+9] = HSVSigma[i];
	}

}

void Im_Features::ExtractFeatures(const cv::Mat &image)
{
	setImage(image);
	ExtractFeatures();
}

void Im_Features::mixChannels(const cv::Mat &in, cv::Mat &out, std::vector<double> alpha) const
{
	if (in.type() == CV_8UC3 && alpha.size() == 3) {
		vector<Mat> Chan(3);
		split(in, Chan); // Split Chan:
		Mat tmp = Mat::zeros(in.rows, in.cols, CV_32FC1);

		for (auto c = 0; c < Chan.size(); ++c) {
			auto it2 = Chan[c].begin<uchar>();
			for (auto it = tmp.begin<float>(); it != tmp.end<float>(); ++it, ++it2) {
				*it += float(alpha[c] * *it2);
			}
		}
		tmp.convertTo(out, CV_8UC1);
	}
}

void Im_Features::colorQuantization(const int k)
{
	Mat samples(_Src.rows * _Src.cols, 3, CV_32F);
	for (int y = 0; y < _Src.rows; y++){
		for (int x = 0; x < _Src.cols; x++) {
			for (int z = 0; z < 3; z++) {
				samples.at<float>(y + x * _Src.rows, z) = _Src.at<Vec3b>(y, x)[z];
			}
		}
	}

	Mat labels;
	const int attempts = 5;
	Mat centers;
	kmeans(samples, k, labels, TermCriteria(CV_TERMCRIT_ITER | CV_TERMCRIT_EPS, 5, 1), attempts, KMEANS_PP_CENTERS, centers);


	_Quantized = Mat(_Src.size(), _Src.type());
	for (int y = 0; y < _Src.rows; y++){
		for (int x = 0; x < _Src.cols; x++)
		{
			const int cluster_idx = labels.at<int>(y + x * _Src.rows, 0);
			_Quantized.at<Vec3b>(y, x)[0] = uchar(centers.at<float>(cluster_idx, 0));
			_Quantized.at<Vec3b>(y, x)[1] = uchar(centers.at<float>(cluster_idx, 1));
			_Quantized.at<Vec3b>(y, x)[2] = uchar(centers.at<float>(cluster_idx, 2));
		}
	}
}

void Im_Features::findDominantColor()
{
	if (_Quantized.empty())	colorQuantization();
	//calcHist(&_Quantized, 1, nullptr, Mat(), hist, 1, &_HistoBins, &histRange);

}

std::ostream & operator<<(std::ostream &os, const Im_Features &obj)
{
	os << "Histogramm : [";
	for (const double &h : obj._Histogram) {
		os << h << ", ";
	}
	os << "\b\b] = " << endl;
	
	os << "Stats : [";
	for (const double &s : obj._Stats) {
		os << s << ", ";
	}
	os << "\b\b]" << endl;

	return os;

}
