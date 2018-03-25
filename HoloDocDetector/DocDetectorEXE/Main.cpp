#include <chrono>
#include <conio.h>	// _getch
#include <cstdlib>
#include <string>
#include <iostream>
#include <fstream>
#include "DocDetector.hpp"
#include "Misc.hpp"
#include "Contours.hpp"
#include "Im_Features.hpp"


#include <set>
#include <opencv2/highgui.hpp>
#include <opencv2/imgproc.hpp>

using namespace std;
using namespace std::chrono;
using namespace cv;

//*****************
//***** CONST *****
//*****************
const string PATH = "../images/";
const vector<string> NAMES = {"A", "B", "C", "D", "E", "F", "G", "H", "I"};
const string EXT = ".jpg";
const vector<string> EDGE_TIMES_NAMES = { 
	"Read", "Gray", "Blur", 
	"Bilateral", "Adaptative Mean", "Adaptative Gaussian",
	"Background Tresh", "Adaptative on Bg Tresh", "Canny on Gray",
	"Canny on Blur", "Canny on Bilateral", "Canny on Bg Tresh" 
};

const vector<string> CONT_SRC = {"NBC", "BT", "BTA"};

const vector<string> CONT_TIMES_NAMES = {
	"Find Contour (Binary->C0)" ,
	"First Clean (C0->C1)",
	"Approx after First Clean (C1->C2)",
	"Convex Hull after Approx (C2->C3)" ,
	"Extract 4 Corners after Convex Hull (C3->C4)",
	"Last Clean (C4->C5)",
	"Convex Hull after Approx (C1->C6)",
	"Approx after Hull (C6->C7)",
	"Extract 4 Corners after Approx (C7->C8)",
	"Last Clean (C8->C9)",
	"Extract 4 Corners after First Clean (C1->C10)",
	"Last Clean (C10->C11)",
	"Rotated Rectangle after First Clean (C1->C12)",
	"Last Clean (C12->C13)"
};


//*******************
//***** VECTORS *****
//*******************
vector<vector<double>> EdgeDuration;
vector<vector<double>> ContourDuration;

void InitVector(int k)
{
	EdgeDuration.resize(k);
	ContourDuration.resize(k);

	for (int i = 0; i < k; ++i) {
		EdgeDuration[i].resize(EDGE_TIMES_NAMES.size());
		ContourDuration[i].resize(CONT_TIMES_NAMES.size()*3);
	}
}

void SaveCSV(int k)
{
	ofstream EdgeFile, ContourFile;
	cout << "Save Edge Duration...";
	EdgeFile.open(PATH + "EdgeDuration.csv");
	EdgeFile << "Step";
	for (int i = 0; i < k; ++i) {
		EdgeFile << ";" << NAMES[i];
	}
	EdgeFile << "\n";

	for (int j = 0; j < EDGE_TIMES_NAMES.size(); ++j){
		EdgeFile << EDGE_TIMES_NAMES[j];
		for (int i = 0; i < k; ++i) {
			EdgeFile << ";" << EdgeDuration[i][j];
		}
		EdgeFile << "\n";
	}
	EdgeFile.close();
	cout <<" Done"<<endl;


	cout << "Save Contour Duration...";
	ContourFile.open(PATH + "ContourDuration.csv");
	ContourFile << "Step";
	for (int i = 0; i < k; ++i) {
		ContourFile << ";" << NAMES[i];
	}
	ContourFile << "\n";

	for (int j = 0; j < CONT_TIMES_NAMES.size(); ++j) {
		for (int h = 0; h < 3; ++h) {
			ContourFile << CONT_TIMES_NAMES[j] <<" "<< CONT_SRC[h];
			for (int i = 0; i < k; ++i) {
				ContourFile << ";" << ContourDuration[i][j * 3 + h];
			}
			ContourFile << "\n";
		}
	}
	ContourFile.close();
	cout << " Done" << endl;

}

//*****************
//***** TESTS *****
//*****************
void TestsEdge(const int i = 0)
{
	cout << "===================================" << endl;
	cout << "===== Test Edge Image " << NAMES[i] << " Begin =====" << endl;

	Mat Im_gray_scale, Im_blur, Im_bilateral, Im_bg_tresh,
		Im_adapt_mean, Im_adapt_gauss, Im_adapt_mean_bg_tresh,
		Im_canny_gray, Im_canny_blur, Im_canny_bilateral, Im_canny_bg_tresh;

	int j = 0;

	auto T1 = high_resolution_clock::now();
	const Mat Src = imread(PATH + NAMES[i] + EXT, CV_LOAD_IMAGE_COLOR);
	if (Src.cols == 0 || Src.rows == 0) { return; }
	duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
	EdgeDuration[i][j] = Fp_ms.count();
	cout << "Time Read : \t\t\t" << EdgeDuration[i][j++] << " ms" << endl;

	T1 = high_resolution_clock::now();
	cvtColor(Src, Im_gray_scale, CV_BGR2GRAY);
	Fp_ms = high_resolution_clock::now() - T1;
	EdgeDuration[i][j] = Fp_ms.count();
	cout << "Time Gray : \t\t\t" << EdgeDuration[i][j++] << " ms" << endl;

	T1 = high_resolution_clock::now();
	blur(Im_gray_scale, Im_blur, Size(3, 3));
	Fp_ms = high_resolution_clock::now() - T1;
	EdgeDuration[i][j] = Fp_ms.count();
	cout << "Time Blur : \t\t\t" << EdgeDuration[i][j++] << " ms" << endl;

	T1 = high_resolution_clock::now();
	bilateralFilter(Im_gray_scale, Im_bilateral, 0, 20, 20);
	Fp_ms = high_resolution_clock::now() - T1;
	EdgeDuration[i][j] = Fp_ms.count();
	cout << "Time Bilateral : \t\t" << EdgeDuration[i][j++] << " ms" << endl;

	T1 = high_resolution_clock::now();
	adaptiveThreshold(Im_gray_scale, Im_adapt_mean, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY, 5, 4);
	bitwise_not(Im_adapt_mean, Im_adapt_mean);
	Fp_ms = high_resolution_clock::now() - T1;
	EdgeDuration[i][j] = Fp_ms.count();
	cout << "Time Adaptative Mean : \t\t" << EdgeDuration[i][j++] << " ms" << endl;

	T1 = high_resolution_clock::now();
	adaptiveThreshold(Im_gray_scale, Im_adapt_gauss, 255, CV_ADAPTIVE_THRESH_GAUSSIAN_C, CV_THRESH_BINARY, 5, 4);
	bitwise_not(Im_adapt_gauss, Im_adapt_gauss);
	Fp_ms = high_resolution_clock::now() - T1;
	EdgeDuration[i][j] = Fp_ms.count();
	cout << "Time Adaptative Gaussian : \t" << EdgeDuration[i][j++] << " ms" << endl;

	//Desk color approx (25,25,25) with 25 threshold approx (0, 0, 0)->(50, 50, 50)
	T1 = high_resolution_clock::now();
	inRange(Src, Scalar(0, 0, 0), Scalar(50, 50, 50), Im_bg_tresh);
	Fp_ms = high_resolution_clock::now() - T1;
	EdgeDuration[i][j] = Fp_ms.count();
	cout << "Time Background Tresh : \t" << EdgeDuration[i][j++] << " ms" << endl;

	T1 = high_resolution_clock::now();
	adaptiveThreshold(Im_bg_tresh, Im_adapt_mean_bg_tresh, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY, 5, 4);
	bitwise_not(Im_adapt_mean_bg_tresh, Im_adapt_mean_bg_tresh);
	Fp_ms = high_resolution_clock::now() - T1;
	EdgeDuration[i][j] = Fp_ms.count();
	cout << "Time Adaptative on Bg Tresh : \t" << EdgeDuration[i][j++] << " ms" << endl;

	T1 = high_resolution_clock::now();
	Canny(Im_gray_scale, Im_canny_gray, 50, 205, 3);
	Fp_ms = high_resolution_clock::now() - T1;
	EdgeDuration[i][j] = Fp_ms.count();
	cout << "Time Canny on Gray : \t\t" << EdgeDuration[i][j++] << " ms" << endl;

	T1 = high_resolution_clock::now();
	Canny(Im_blur, Im_canny_blur, 50, 205, 3);
	Fp_ms = high_resolution_clock::now() - T1;
	EdgeDuration[i][j] = Fp_ms.count();
	cout << "Time Canny on Blur : \t\t" << EdgeDuration[i][j++] << " ms" << endl;

	T1 = high_resolution_clock::now();
	Canny(Im_bilateral, Im_canny_bilateral, 50, 205, 3);
	Fp_ms = high_resolution_clock::now() - T1;
	EdgeDuration[i][j] = Fp_ms.count();
	cout << "Time Canny on Bilateral : \t" << EdgeDuration[i][j++] << " ms" << endl;

	T1 = high_resolution_clock::now();
	Canny(Im_bg_tresh, Im_canny_bg_tresh, 50, 205, 3);
	Fp_ms = high_resolution_clock::now() - T1;
	EdgeDuration[i][j] = Fp_ms.count();
	cout << "Time Canny on Bg Tresh : \t" << EdgeDuration[i][j] << " ms" << endl;


	//***** Save *****
	cout << "Saves... ";
	const string Path = PATH + "Edge_Tests/";
	imwrite(Path + NAMES[i] + "_Gray" + EXT, Im_gray_scale);
	imwrite(Path + NAMES[i] + "_Blur" + EXT, Im_blur);
	imwrite(Path + NAMES[i] + "_Bilat" + EXT, Im_bilateral);
	imwrite(Path + NAMES[i] + "_AdaptM" + EXT, Im_adapt_mean);
	imwrite(Path + NAMES[i] + "_AdaptG" + EXT, Im_adapt_gauss);
	imwrite(Path + NAMES[i] + "_Background_Tresh" + EXT, Im_bg_tresh);
	imwrite(Path + NAMES[i] + "_Background_Tresh_Adapt" + EXT, Im_adapt_mean_bg_tresh);
	imwrite(Path + NAMES[i] + "_Canny_Gray" + EXT, Im_canny_gray);
	imwrite(Path + NAMES[i] + "_Canny_Blur" + EXT, Im_canny_blur);
	imwrite(Path + NAMES[i] + "_Canny_Bilat" + EXT, Im_canny_bilateral);
	imwrite(Path + NAMES[i] + "_Canny_Bg_Tresh" + EXT, Im_canny_bg_tresh);
	cout << "Done" << endl;

	cout << "===== Test Edge Image " << NAMES[i] << " End =====" << endl;
	cout << "=================================" << endl << endl;
}

void TestsContour(const int i = 0)
{
	cout << "======================================" << endl;
	cout << "===== Test Contour Image " << NAMES[i] << " Begin =====" << endl;

	//***** Variables Inits *****
	int j = 0;
	vector<Mat> Init_Ims;
	vector<vector<Mat>> Contours_Ims;
	vector<vector<vector<vector<Point>>>> Contours;
	Init_Ims.resize(6);
	Contours_Ims.resize(14);
	Contours.resize(14);

	for (int k = 0; k < Contours.size(); ++k) {
		Contours[k].resize(3);
		Contours_Ims[k].resize(3);
	}

	const vector<string> Name_Type = {"_NBC", "_BT", "_BTA"};
	const vector<string> Print_Type = {"\tNDG + Blur + Canny :\t", "\tBackground Threshold : \t", "\tBT + Adaptative :\t"};
	const vector<string> Print_Methods = {
		"Originals", "1st Clean",
		"1 Approx", "1 Hull", "1 Extract", "1 Final",
		"2 Hull", "2 Approx", "2 Extract", "2 Final",
		"3 Extract", "3 Final",
		"4 Rects", "4 Final"
	};
	
	//***** Binary Image *****
	cout << "Create selected Binary Image..." << endl;
	Init_Ims[0] = imread(PATH + NAMES[i] + EXT, CV_LOAD_IMAGE_COLOR);
	if (Init_Ims[0].cols == 0 || Init_Ims[0].rows == 0) { return; }

	//Minimum length for a rectangle detected is 10% of image (perimeter = 0.1 * 2(H+L))
	const double Length_min = 0.2 * (Init_Ims[0].cols + Init_Ims[0].rows);
	//Maximum length for a rectangle detected is 70% of image (perimeter = 0.7 * 2(H+L))
	const double Length_max = 1.4 * (Init_Ims[0].cols + Init_Ims[0].rows);
	//Minimum distance between 2 contour center is 5% of Diagonal image (Diag = 0.1 * sqrt(H*H + L*L))
	const double Center_dist_min = 0.05 * SquaredDist(Point(Init_Ims[0].cols, Init_Ims[0].rows));
	cout << "Length min = " << Length_min << "\tLength max = " << Length_max << "\tDistance Center min = " <<
			Center_dist_min << endl;

	cout << "Binary Image... ";
	cvtColor(Init_Ims[0], Init_Ims[1], CV_BGR2GRAY);
	blur(Init_Ims[1], Init_Ims[2], Size(3, 3));
	Canny(Init_Ims[2], Init_Ims[3], 50, 205, 3);
	inRange(Init_Ims[0], Scalar(0, 0, 0), Scalar(50, 50, 50), Init_Ims[4]);
	adaptiveThreshold(Init_Ims[4], Init_Ims[5], 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY, 5, 4);
	bitwise_not(Init_Ims[5], Init_Ims[5]);
	cout << "Done." << endl << endl;

	//Contours
	cout << "Time Find Contour (Binary->C0) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		findContours(Init_Ims[k + 3], Contours[0][k], CV_RETR_LIST, CV_CHAIN_APPROX_SIMPLE);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		ContourDuration[i][j] = Fp_ms.count();
		cout << Print_Type[k] << ContourDuration[i][j++] << " ms" << endl;
	}

	//First Clean obligatory
	cout << "Time First Clean (C0->C1) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		CleanBasic(Contours[0][k], Contours[1][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		ContourDuration[i][j] = Fp_ms.count();
		cout << Print_Type[k] << ContourDuration[i][j++] << " ms" << endl;
	}

	//***** FIRST WAY ****
	cout << endl << "===== FIRST WAY =====" << endl;
	//Approx After First Clean
	cout << "Time Approx after First Clean (C1->C2) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Approxs(Contours[1][k], Contours[2][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		ContourDuration[i][j] = Fp_ms.count();
		cout << Print_Type[k] << ContourDuration[i][j++] << " ms" << endl;
	}

	//Convex Hull After Approx
	cout << "Time Convex Hull after Approx (C2->C3) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Hulls(Contours[2][k], Contours[3][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		ContourDuration[i][j] = Fp_ms.count();
		cout << Print_Type[k] << ContourDuration[i][j++] << " ms" << endl;
	}

	//Extract After Hull
	cout << "Time Extract 4 Corners after Convex Hull (C3->C4) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Extract4Corners(Contours[3][k], Contours[4][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		ContourDuration[i][j] = Fp_ms.count();
		cout << Print_Type[k] << ContourDuration[i][j++] << " ms" << endl;
	}

	//Final Clean
	cout << "Time Last Clean (C4->C5) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		FinalClean(Contours[4][k], Contours[5][k], Length_min, Length_max, Center_dist_min);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		ContourDuration[i][j] = Fp_ms.count();
		cout << Print_Type[k] << ContourDuration[i][j++] << " ms" << endl;
	}

	//***** SECOND WAY ****
	cout << endl << "===== SECOND WAY =====" << endl;
	//Convex Hull After First Clean
	cout << "Time Convex Hull after Approx (C1->C6) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Hulls(Contours[1][k], Contours[6][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		ContourDuration[i][j] = Fp_ms.count();
		cout << Print_Type[k] << ContourDuration[i][j++] << " ms" << endl;
	}

	//Approx After Convex Hull
	cout << "Time Approx after Hull (C6->C7) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Approxs(Contours[6][k], Contours[7][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		ContourDuration[i][j] = Fp_ms.count();
		cout << Print_Type[k] << ContourDuration[i][j++] << " ms" << endl;
	}

	//Extract After Approx
	cout << "Time Extract 4 Corners after Approx (C7->C8) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Extract4Corners(Contours[7][k], Contours[8][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		ContourDuration[i][j] = Fp_ms.count();
		cout << Print_Type[k] << ContourDuration[i][j++] << " ms" << endl;
	}

	//Final Clean
	cout << "Time Last Clean (C8->C9) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		FinalClean(Contours[8][k], Contours[9][k], Length_min, Length_max, Center_dist_min);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		ContourDuration[i][j] = Fp_ms.count();
		cout << Print_Type[k] << ContourDuration[i][j++] << " ms" << endl;
	}

	//***** THIRD WAY ****
	cout << endl << "===== THIRD WAY =====" << endl;
	//Extract After First Clean
	cout << "Time Extract 4 Corners after First Clean (C1->C10) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Extract4Corners(Contours[1][k], Contours[10][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		ContourDuration[i][j] = Fp_ms.count();
		cout << Print_Type[k] << ContourDuration[i][j++] << " ms" << endl;
	}

	//Final Clean
	cout << "Time Last Clean (C10->C11) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		FinalClean(Contours[10][k], Contours[11][k], Length_min, Length_max, Center_dist_min);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		ContourDuration[i][j] = Fp_ms.count();
		cout << Print_Type[k] << ContourDuration[i][j++] << " ms" << endl;
	}

	//***** FOURTH WAY ****
	cout << endl << "===== FOURTH WAY =====" << endl;
	//Rotated Rectangle After First Clean
	cout << "Time Rotated Rectangle after First Clean (C1->C12) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Rects(Contours[1][k], Contours[12][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		ContourDuration[i][j] = Fp_ms.count();
		cout << Print_Type[k] << ContourDuration[i][j++] << " ms" << endl;
	}

	//Final Clean
	cout << "Time Last Clean (C12->C13) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		FinalClean(Contours[12][k], Contours[13][k], Length_min, Length_max, Center_dist_min);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		ContourDuration[i][j] = Fp_ms.count();
		cout << Print_Type[k] << ContourDuration[i][j] << " ms" << endl;
	}

	//***** Prints *****
	cout << endl << "Nb contours :" << endl;
	for (j = 0; j < Contours.size(); ++j) {
		cout << Print_Methods[j] << " :\t";
		for (int k = 0; k < 3; ++k) {
			cout << "[" << j << "][" << k << "] : " << Contours[j][k].size() << "\t";
		}
		cout << endl;
	}

	cout << endl << "Nb Point On Contours : " << endl;
	for (j = 1; j < Contours.size(); ++j) {
		cout << Print_Methods[j] << " : " << endl;
		for (int k = 0; k < 3; ++k) {
			cout << "\t[" << j << "][" << k << "] : ";
			printContourSize(Contours[j][k]);
			cout << endl;
		}
	}

	//***** Draws *****
	cout << endl << "Draws... ";
	for (j = 1; j < Contours.size(); ++j) {
		for (int k = 0; k < 3; ++k) {
			DrawCont(Init_Ims[0], Contours_Ims[j][k], Contours[j][k], true);
		}
	}
	cout << "Done" << endl;

	//***** Save *****
	cout << "Saves... ";
	//Binary Images
	const string Path = PATH + "Contour_Tests/";
	for (int k = 0; k < 3; ++k) {
		imwrite(Path + NAMES[i] + Name_Type[k] + EXT, Init_Ims[k + 3]);
	}
	//Contour Images
	for (j = 1; j < Contours.size(); ++j) {
		for (int k = 0; k < 3; ++k) {
			imwrite(Path + NAMES[i] + Name_Type[k] + "_C" + to_string(j) + EXT, Contours_Ims[j][k]);
		}
	}
	cout << "Done" << endl;

	cout << "===== Test Contour Image " << NAMES[i] << " End =====" << endl;
	cout << "====================================" << endl << endl;
}

int TestsDLLFunction(const int i = 0)
{
	cout << "======================================" << endl;
	cout << "===== Test DLL Image " << NAMES[i] << " =====" << endl;

	Mat Im_Cont, Im_Doc;
	
	const Mat Src = imread(PATH + NAMES[i] + EXT, CV_LOAD_IMAGE_COLOR);
	if (Src.cols == 0 || Src.rows == 0) { return EMPTY_MAT; }
	vector<vector<Point>> Contours;
	vector<Point> Contour;
	const Scalar Background = COLORS[0];

	//Document Detection
	cout << endl << "Detection... " << endl;
	auto T1 = high_resolution_clock::now();
	int ErrCode = DocsDetection(Src, Background, Contours);
	duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;

	cout << "Time :\t" << Fp_ms.count() << " ms" << endl
		<< "errCode : \t" << ErrCode<<endl
		<< "Nb Contours : \t" << Contours.size()<<endl;
	printContour(Contours);

	if (ErrCode != NO_ERRORS) return ErrCode;
	cout << "Done" << endl;

	//Document Extraction
	cout << endl << "Extraction... " << endl;
	T1 = high_resolution_clock::now();
	ErrCode = DocExtraction(Src, Background, Contour, Im_Doc);
	Fp_ms = high_resolution_clock::now() - T1;

	cout << "Time :\t" << Fp_ms.count() << " ms" << endl
		<< "errCode : \t" << ErrCode << endl
		<< "Contour : \t[" << Contour[0] << ", " << Contour[1] << ", " << Contour[2] << ", " << Contour[3] << "]" << endl;

	if (ErrCode != NO_ERRORS) return ErrCode;
	cout << "Done" << endl;

	//Document Comparaison
	//...............

	//Draw and Write
	DrawCont(Src, Im_Cont, Contours, false);

	const string Path = PATH + "Final_Tests/";
	imwrite(Path + NAMES[i] + "_Cont" + EXT, Im_Cont);
	imwrite(Path + NAMES[i] + "_Doc" + EXT, Im_Doc);
	cout << "======================================" << endl << endl;

	return NO_ERRORS;
}

void TestsReco()
{
	const int num_im = 10;
	const string Path = PATH + "Reco_Tests/";
	Im_Features F[num_im];

	for(int i = 0; i < num_im; ++i) {
		cout << "==============================" << endl;
		cout << "===== Test Reco Image " << to_string(i+1) << " =====" << endl;
		const string tmp_name = Path + to_string(i + 1);
		cout << tmp_name << endl;
		const Mat Src = imread(tmp_name + EXT, CV_LOAD_IMAGE_COLOR);
		F[i].ExtractFeatures(Src);
		cout << F[i];
		F[i].ToCSV(tmp_name + ".csv");
		cout << "==============================" << endl << endl;
	}

	cout << "===========================================" << endl;
	cout << "===== C'est l'heure du Dududududuel ! =====" << endl;
	double Dists[num_im][num_im];
	for (int i = 0; i < num_im; ++i) {
		for (int j = 0; j < num_im; ++j) {
			Dists[i][j] = F[i].Distance(F[j]);
		}
	}
	
	for(int i = 0; i < num_im; ++i) {
		for (int j = i+1; j < num_im; ++j) {
			cout << i+1 << " VS " << j+1 << " Dist = " << Dists[i][j] << endl;
		}
		cout << endl;
	}
	cout << "===========================================" << endl << endl;

	cout << "==========================" << endl;
	cout << "===== Enregistrement =====" << endl;
	ofstream myfile;
	myfile.open(Path + "Distances.csv");
	//First Row
	myfile << "i\\j";
	for (int i = 0; i < num_im; ++i) {
		myfile << ";" << i + 1;
	}
	myfile << "\n";
	//Rows
	for (int i = 0; i < num_im; ++i) {
		myfile << i+1;
		for (int j = 0; j <= i;++j) {
			myfile << ";" << Dists[i][j];
		}
		for (int j = i+1; j < num_im; ++j) {
			myfile << "; ";
		}
		myfile << "\n";
	}

	myfile.close();
	cout << "==========================" << endl << endl;
	
}

int main(int argc, char *argv[])
{
	//***** Init *****
	(void)argc;
	(void)argv;

	cout.precision(5);
	cout << fixed;
	InitVector(NAMES.size());

	for (int i = 0; i < NAMES.size(); ++i) {
		TestsEdge(i);
		TestsContour(i);
		//TestsDLLFunction(i);
	}
	SaveCSV(NAMES.size());

	//TestsReco();
	cout << endl << "That's all Folks !" << endl;
	_getch();
	return EXIT_SUCCESS;
}
