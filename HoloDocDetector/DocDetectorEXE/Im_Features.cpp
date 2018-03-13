#include "Im_Features.hpp"
#include <opencv2/imgproc.hpp>
#include <vector>
#include <iostream>
#include <fstream>

using namespace std;
using namespace cv;

Im_Features::Im_Features(const int histo_bins, const int hog_bins): _HistoBins(histo_bins), _HOGBins(hog_bins)
{
	_Histograms = vector<double>(_HistoChans*_HistoBins, 0.0);
	_HOG = vector<double>(_HOGBins, 0.0);
}

Im_Features::~Im_Features()
{
	_HOG.clear();
	_Histograms.clear();
}

void Im_Features::ExtractFeatures(const cv::Mat &image)
{
	extractHistograms(image);
	extractHOG(image);
}

double Im_Features::Distance(const Im_Features &features, vector<double> coefs)
{

	if (coefs.size() != _HistoChans + 1) coefs = vector<double>(_HistoChans + 1, 1.0);
	vector<double> Dists_Val(_HistoChans + 1, 0.0);
 	for (int i = 0; i < _HOG.size(); ++i) {
		const double Val = _HOG[i] - features._HOG[i];
		Dists_Val[0] += Val * Val;
	}
	
	for (int i = 0; i < _HistoChans; ++i) {
		const int idx = i * _HistoBins;
		for (int j = 0; j < _HistoBins; ++j) {
			const double Val = _Histograms[idx + j] - features._Histograms[idx + j];
			Dists_Val[i+1] += Val * Val;
		}
	}

	double Res = 0.0, Coefs = 0.0;
	for (int i = 0; i < _HistoChans + 1; ++i) {
		Res += sqrt(Dists_Val[i]) * coefs[i];
		Coefs += coefs[i];
	}

	return ((1 - ((Res / Coefs) / sqrt(2))) * 100);
}

void Im_Features::ToCSV(const std::string &filename)
{
	ofstream myfile;
	myfile.open(filename);
	if (!_HOG.empty()) {
		for (int i = 0; i < _HOG.size(); ++i) {
			myfile << _HOG[i] << ";";
		}
	}
	myfile << "\n";
	if (!_Histograms.empty()) {
		for (int i = 0; i < _HistoChans; ++i) {
			const int idx = i * _HistoBins;
			for (int j = 0; j < _HistoBins; ++j) {
				myfile << _Histograms[idx + j] << ";";
			}
			myfile << "\n";
		}
	}
	myfile << "\n";
	myfile.close();
}

void Im_Features::extractHistograms(const cv::Mat &image)
{
	const int Nb_Channels = 3;
	const double Nb_Points = image.rows * image.cols;
	const float Range[] = { 0, 256 };
	const float* Hist_Range = { Range };
	Mat HSV;
	cvtColor(image, HSV, COLOR_BGR2HSV);
	vector<Mat> BGRs(3), HSVs(3), Hists;
	split(image, BGRs);
	split(HSV, HSVs);
	Hists.resize(_HistoChans);
	for (int i = 0; i < Nb_Channels; ++i) {
		calcHist(&HSVs[i], 1, nullptr, Mat(), Hists[i], 1, &_HistoBins, &Hist_Range);
		calcHist(&BGRs[i], 1, nullptr, Mat(), Hists[i + 3], 1, &_HistoBins, &Hist_Range);
	}

	for(int i = 0; i < _HistoChans; ++i) {
		int idx = i * _HistoBins;
		for (auto it = Hists[i].begin<float>(); it != Hists[i].end<float>(); ++it) {
			_Histograms[idx++] = *it / Nb_Points;
		}
	}
	BGRs.clear();
	HSVs.clear();
	Hists.clear();
}

void Im_Features::extractHOG(const cv::Mat &image)
{
	Mat Gray, Gx, Gy, Mag, Angle;
	cvtColor(image, Gray, COLOR_BGR2GRAY);
	Gray.convertTo(Gray, CV_32F, 1 / 255.0);
	// Calculate gradients gx, gy
	Sobel(Gray, Gx, CV_32F, 1, 0, 1);
	Sobel(Gray, Gy, CV_32F, 0, 1, 1);
	// Calculate gradient magnitude and direction(in degrees)
	cartToPolar(Gx, Gy, Mag, Angle, true);
	// angle is in [0,360] 
	const int Step = 360 / _HOGBins;

	auto it_Mag = Mag.begin<float>(),	it_Angle = Angle.begin<float>();
	const auto End = Mag.end<float>();
	double Sum_Mag = 0.0;
	for (; it_Mag != End; ++it_Mag, ++it_Angle) {
		if (*it_Mag > 0.0) {
			//Find the two bins to add magnitude
			const float Bins_Range = *it_Angle / Step;
			const int Idx = int(floor(Bins_Range));
			float Ratio = Bins_Range - Idx;
			if (Ratio > 0.5) Ratio -= 0.5;
			if (!(0 <= Idx && Idx < _HOGBins)) cout << "WTH : " << Idx<<endl;
			_HOG[Idx] += Ratio * *it_Mag;
			_HOG[(Idx==_HOGBins-1) ? 0: Idx + 1] += (1.0 - Ratio) * *it_Mag;
			Sum_Mag += *it_Mag;
		}
	}

	if (Sum_Mag != 0.0) {
		for (int i = 0; i < _HOG.size(); ++i) {
			_HOG[i] /= Sum_Mag;
		}
	}
}


std::ostream & operator<<(std::ostream &os, const Im_Features &obj)
{
	os << "HOG : [";
	if (!obj._HOG.empty()) {
		for (int i = 0; i < obj._HOG.size(); ++i) {
			os << obj._HOG[i] << ", ";
		}
		os << "\b\b";
	}
	os << "]" << endl;

	const vector<string> Name_Channels = { "H","S","V","B","G","R" };
	for (int i = 0; i < obj._HistoChans; ++i) {
		const int idx = i * obj._HistoBins;
		os << "Histogramm " << Name_Channels[i] << " : [";
		if (!obj._Histograms.empty()) {
			for (int j = 0; j < obj._HistoBins; ++j) {
				os << obj._Histograms[idx + j] << ", ";
			}
			os << "\b\b";
		}
		os << "]" << endl;
	}



	return os;

}
