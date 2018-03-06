#include <chrono>
#include <conio.h>	// _getch
#include <cstdlib>
#include <iostream>
#include "DocDetector.hpp"

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

const vector<Scalar> COLORS = {
	Scalar(0, 0, 0), Scalar(125, 125, 125), Scalar(255, 255, 255),	// Noir		Gris		Blanc
	Scalar(255, 0, 0), Scalar(0, 255, 0), Scalar(0, 0, 255),		// Rouge	Vert		Bleu
	Scalar(0, 255, 255), Scalar(255, 0, 255), Scalar(255, 255, 0),	// Cyan		Magenta		Jaune
	Scalar(255, 125, 0), Scalar(0, 255, 125), Scalar(125, 0, 255),	// Orange	Turquoise	Indigo
	Scalar(255, 0, 125), Scalar(125, 255, 0), Scalar(0, 125, 255),	// Fushia	Lime		Azur
	Scalar(125, 0, 0), Scalar(0, 125, 0), Scalar(0, 0, 125)			// Blood	Grass		Deep
};

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

void DrawCont(const Mat &src, Mat &dst, const vector<vector<Point>> &contours, const bool fill = false)
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
	int cross[2] = {0, 0};
	for (int i = 0; i < 3; ++i) {
		const int sign = V.x * point.y - V.y * point.x >= 0 ? 1 : 0;
		cross[sign]++;
		V = quad[i + 1] - quad[i];
	}
	return cross[0] == 0 || cross[1] == 0;
}


//***************************
//***** CLEAN FUNCTIONS *****
//***************************
//Basic fast clean only number of point and length
//Possibility to change arclength by boundingrect maybe some contour can be suppress in some case but its a little slower
void CleanBasic(const vector<vector<Point>> &in, vector<vector<Point>> &out,
				const double min_length = 600, const double max_length = 3600)
{
	out.clear();
	for (const vector<Point> &cont : in) {
		const double peri = arcLength(cont, true);
		if (cont.size() >= 4 && min_length <= peri && peri <= max_length) {
			out.push_back(cont);
		}
	}
}

void Hulls(const vector<vector<Point>> &in, vector<vector<Point>> &out,
		   const double min_length = 600, const double max_length = 3600)
{
	out.clear();
	for (const vector<Point> &cont : in) {
		vector<Point> H;
		convexHull(cont, H);
		const double peri = arcLength(H, true);
		if (H.size() >= 4 && min_length <= peri && peri <= max_length) {
			out.push_back(H);
		}
	}
}

void Approxs(const vector<vector<Point>> &in, vector<vector<Point>> &out,
			 const double min_length = 600, const double max_length = 3600)
{
	out.clear();
	for (const vector<Point> &cont : in) {
		const double peri = arcLength(cont, true);
		vector<Point> A;
		approxPolyDP(cont, A, 0.02 * peri, true);
		const double peri2 = arcLength(A, true);
		if (A.size() >= 4 && min_length <= peri2 && peri2 <= max_length) {
			out.push_back(A);
		}
	}
}

void Rects(const vector<vector<Point>> &in, vector<vector<Point>> &out,
		   const double min_length = 600, const double max_length = 3600)
{
	out.clear();
	for (const vector<Point> &cont : in) {
		RotatedRect box = minAreaRect(cont);
		const double peri = 2 * (box.size.height + box.size.width);
		if (min_length <= peri && peri <= max_length) {
			Point2f vertices[4];
			box.points(vertices);
			vector<Point> R;
			for (auto &v : vertices) {
				R.emplace_back(cvRound(v.x), cvRound(v.y));
			}
			out.push_back(R);
		}
	}
}

void Extract4Corners(const vector<vector<Point>> &in, vector<vector<Point>> &out,
					 const double min_length = 600, const double max_length = 3600)
{
	out.clear();
	for (const vector<Point> &cont : in) {
		const double Peri = arcLength(cont, true);
		if (min_length <= Peri && Peri <= max_length) {
			const int Nb_points = int(cont.size());
			if (Nb_points == 4) {
				out.push_back(cont);
			} else if (Nb_points > 4) {
				//Ids Order : 
				// 0 : Point on the left side of Diagonal
				// 1 : Point on the right side of Diagonal
				// 2 : First Point of Diag (Farest Point)
				// 3 : Second Point Of Diag (Farest of the First)
				int Ids[4] = {0, 0, 0, 0};
				set<int> Corners_id;			//Use set to avoid duplication

				//***** Get Center (mean not center of shape) ****
				Point Center(0, 0);
				for (const Point &p : cont) {
					Center += p;
				}
				Center.x /= Nb_points;
				Center.y /= Nb_points;

				//***** Get Diagonal (Maximize Distance) ****
				for (int k = 2; k < 4; ++k) {
					int Dist_max = 0;
					int Id_farest = 0;
					for (int i = 0; i < Nb_points; ++i) {
						const int dist = SquaredDist(cont[i], Center);
						if (dist > Dist_max) {
							Dist_max = dist;
							Id_farest = i;
						}
					}
					Corners_id.insert(Id_farest);
					Ids[k] = Id_farest;
					Center = cont[Id_farest];
				}

				//***** Find Other Points (Maximize Area) ****
				double Areas_max[2] = {0.0, 0.0};
				const Point AB = cont[Ids[3]] - cont[Ids[2]];
				for (int i = 0; i < Nb_points; ++i) {
					const Point AC = cont[i] - cont[Ids[2]],
								BC = cont[i] - cont[Ids[3]];
					const int d = AB.x * AC.y - AB.y * AC.x;
					//if (d = 0) C is on Diagonal
					if (d != 0) {
						const int side = d > 0 ? 0 : 1;
						const double Dist_AB = Dist(AB),
									 Dist_AC = Dist(AC),
									 Dist_BC = Dist(BC),
									 peri_2 = (Dist_AB + Dist_AC + Dist_BC) / 2;
						// False area based on Heron's formula without square root (maybe avoid on distance too...)
						const double area = peri_2 * (peri_2 - Dist_AC) * (peri_2 - Dist_BC) * (peri_2 - Dist_AB);
						if (area > Areas_max[side]) {
							Areas_max[side] = area;
							Ids[side] = i;
						}
					}
				}
				Corners_id.insert(Ids[0]);
				Corners_id.insert(Ids[1]);

				//***** Copy to out ****
				//In extrem situation maybe the diagonal is found as a side of polygone 
				//(a lot point in this side and very few elsewhere and concave polygon)
				// so D is never positive (or negative) and if point 0 is already on the corners it's not duplicated
				if (Corners_id.size() == 4) {
					vector<Point> Tmp;
					Tmp.reserve(4);
					Tmp.push_back(cont[Ids[2]]);	// First Point Of Diag
					Tmp.push_back(cont[Ids[0]]);	// Left Point
					Tmp.push_back(cont[Ids[3]]);	// Second Point of Diag
					Tmp.push_back(cont[Ids[1]]);	// Right Point
					const double Peri2 = arcLength(Tmp, true);
					if (min_length <= Peri2 && Peri2 <= max_length) {
						out.push_back(Tmp);
					}
				}
			}
		}
	}
}

//Keep only contours with : 
//	- 4 corners (obligatory extract 4 corner is launch before)
//	- Good length
//	- No doubles
//	- No contours inside an other
//	- No contours with a shape too far from a rectangle
void FinalClean(const vector<vector<Point>> &in, vector<vector<Point>> &out,
				const double min_length = 600, const double max_length = 3600,
				const double min_center_dist = 110, const double side_ratio = 0.5)
{
	out = in;
	if (!out.empty()) {
		//Sort by Area
		sort(out.begin(), out.end(), SortArea);

		const auto Begin = out.begin();
		//length check + Doubles & inside suppress 
		for (int i = 0; i < out.size() - 1; ++i) {
			const double Peri = arcLength(out[i], true);
			if (min_length <= Peri && Peri <= max_length) {
				const Point Center1 = GetCenter(out[i]);
				for (int j = i + 1; j < out.size(); ++j) {
					const Point Center2 = GetCenter(out[j]);
					//	if (inQuad(out[i], Center2)) //doesn't work
					const int Center_dist = SquaredDist(Center1, Center2);
					// Same Center or Center distance smallest than including circle radius (it works because contour is sort by area)
					if (Center_dist < min_center_dist || Center_dist < SquaredDist(Center1, out[i][0])) {
						out.erase(Begin + j);
						j--;
					}
				}
			}
		}

		//Shape Verification
		//the opposite side must have same distance (with treshold)
		const double Ratio_min = 1 - side_ratio, Ratio_max = 1 + side_ratio;
		for (int i = 0; i < out.size(); ++i) {
			int Sides[4];
			Sides[0] = SquaredDist(out[i][0], out[i][1]);
			Sides[1] = SquaredDist(out[i][2], out[i][3]);
			Sides[2] = SquaredDist(out[i][1], out[i][2]);
			Sides[3] = SquaredDist(out[i][3], out[i][0]);
			const double Ratio_1 = 1.0 * Sides[0] / Sides[1],
						 Ratio_2 = 1.0 * Sides[2] / Sides[3];
			if (Ratio_min > Ratio_1 || Ratio_1 > Ratio_max || Ratio_min > Ratio_2 || Ratio_2 > Ratio_max) {
				out.erase(Begin + i);
				i--;
			}
		}
	}
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

	auto T1 = high_resolution_clock::now();
	const Mat Src = imread(PATH + NAMES[i] + EXT, CV_LOAD_IMAGE_COLOR);
	if (Src.cols == 0 || Src.rows == 0) { return; }
	duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
	cout << "Time Read : \t\t\t" << Fp_ms.count() << " ms" << endl;

	T1 = high_resolution_clock::now();
	cvtColor(Src, Im_gray_scale, CV_BGR2GRAY);
	Fp_ms = high_resolution_clock::now() - T1;
	cout << "Time Gray : \t\t\t" << Fp_ms.count() << " ms" << endl;

	T1 = high_resolution_clock::now();
	blur(Im_gray_scale, Im_blur, Size(3, 3));
	Fp_ms = high_resolution_clock::now() - T1;
	cout << "Time Blur : \t\t\t" << Fp_ms.count() << " ms" << endl;

	T1 = high_resolution_clock::now();
	bilateralFilter(Im_gray_scale, Im_bilateral, 0, 20, 20);
	Fp_ms = high_resolution_clock::now() - T1;
	cout << "Time Bilateral : \t\t" << Fp_ms.count() << " ms" << endl;

	T1 = high_resolution_clock::now();
	adaptiveThreshold(Im_gray_scale, Im_adapt_mean, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY, 5, 4);
	bitwise_not(Im_adapt_mean, Im_adapt_mean);
	Fp_ms = high_resolution_clock::now() - T1;
	cout << "Time Adaptative Mean : \t\t" << Fp_ms.count() << " ms" << endl;

	T1 = high_resolution_clock::now();
	adaptiveThreshold(Im_gray_scale, Im_adapt_gauss, 255, CV_ADAPTIVE_THRESH_GAUSSIAN_C, CV_THRESH_BINARY, 5, 4);
	bitwise_not(Im_adapt_gauss, Im_adapt_gauss);
	Fp_ms = high_resolution_clock::now() - T1;
	cout << "Time Adaptative Gaussian : \t" << Fp_ms.count() << " ms" << endl;

	//Desk color approx (25,25,25) with 25 threshold approx (0, 0, 0)->(50, 50, 50)
	T1 = high_resolution_clock::now();
	inRange(Src, Scalar(0, 0, 0), Scalar(50, 50, 50), Im_bg_tresh);
	Fp_ms = high_resolution_clock::now() - T1;
	cout << "Time Background Tresh : \t" << Fp_ms.count() << " ms" << endl;

	T1 = high_resolution_clock::now();
	adaptiveThreshold(Im_bg_tresh, Im_adapt_mean_bg_tresh, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY, 5, 4);
	bitwise_not(Im_adapt_mean_bg_tresh, Im_adapt_mean_bg_tresh);
	Fp_ms = high_resolution_clock::now() - T1;
	cout << "Time Adaptative on Bg Tresh : \t" << Fp_ms.count() << " ms" << endl;

	T1 = high_resolution_clock::now();
	Canny(Im_gray_scale, Im_canny_gray, 50, 205, 3);
	Fp_ms = high_resolution_clock::now() - T1;
	cout << "Time Canny on Gray : \t\t" << Fp_ms.count() << " ms" << endl;

	T1 = high_resolution_clock::now();
	Canny(Im_blur, Im_canny_blur, 50, 205, 3);
	Fp_ms = high_resolution_clock::now() - T1;
	cout << "Time Canny on Blur : \t\t" << Fp_ms.count() << " ms" << endl;

	T1 = high_resolution_clock::now();
	Canny(Im_bilateral, Im_canny_bilateral, 50, 205, 3);
	Fp_ms = high_resolution_clock::now() - T1;
	cout << "Time Canny on Bilateral : \t" << Fp_ms.count() << " ms" << endl;

	T1 = high_resolution_clock::now();
	Canny(Im_bg_tresh, Im_canny_bg_tresh, 50, 205, 3);
	Fp_ms = high_resolution_clock::now() - T1;
	cout << "Time Canny on Bg Tresh : \t" << Fp_ms.count() << " ms" << endl;


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
		cout << Print_Type[k] << Fp_ms.count() << " ms" << endl;
	}

	//First Clean obligatory
	cout << "Time First Clean (C0->C1) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		CleanBasic(Contours[0][k], Contours[1][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		cout << Print_Type[k] << Fp_ms.count() << " ms" << endl;
	}

	//***** FIRST WAY ****
	cout << endl << "===== FIRST WAY =====" << endl;
	//Approx After First Clean
	cout << "Time Approx after First Clean (C1->C2) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Approxs(Contours[1][k], Contours[2][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		cout << Print_Type[k] << Fp_ms.count() << " ms" << endl;
	}

	//Convex Hull After Approx
	cout << "Time Convex Hull after Approx (C2->C3) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Hulls(Contours[2][k], Contours[3][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		cout << Print_Type[k] << Fp_ms.count() << " ms" << endl;
	}

	//Extract After Hull
	cout << "Time Extract 4 Corners after Convex Hull (C3->C4) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Extract4Corners(Contours[3][k], Contours[4][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		cout << Print_Type[k] << Fp_ms.count() << " ms" << endl;
	}

	//Final Clean
	cout << "Time Last Clean (C4->C5) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		FinalClean(Contours[4][k], Contours[5][k], Length_min, Length_max, Center_dist_min);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		cout << Print_Type[k] << Fp_ms.count() << " ms" << endl;
	}

	//***** SECOND WAY ****
	cout << endl << "===== SECOND WAY =====" << endl;
	//Convex Hull After First Clean
	cout << "Time Convex Hull after Approx (C1->C6) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Hulls(Contours[1][k], Contours[6][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		cout << Print_Type[k] << Fp_ms.count() << " ms" << endl;
	}

	//Approx After Convex Hull
	cout << "Time Approx after Hull (C6->C7) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Approxs(Contours[6][k], Contours[7][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		cout << Print_Type[k] << Fp_ms.count() << " ms" << endl;
	}

	//Extract After Approx
	cout << "Time Extract 4 Corners after Approx (C7->C8) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Extract4Corners(Contours[7][k], Contours[8][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		cout << Print_Type[k] << Fp_ms.count() << " ms" << endl;
	}

	//Final Clean
	cout << "Time Last Clean (C8->C9) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		FinalClean(Contours[8][k], Contours[9][k], Length_min, Length_max, Center_dist_min);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		cout << Print_Type[k] << Fp_ms.count() << " ms" << endl;
	}

	//***** THIRD WAY ****
	cout << endl << "===== THIRD WAY =====" << endl;
	//Extract After First Clean
	cout << "Time Extract 4 Corners after First Clean (C1->C10) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Extract4Corners(Contours[1][k], Contours[10][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		cout << Print_Type[k] << Fp_ms.count() << " ms" << endl;
	}

	//Final Clean
	cout << "Time Last Clean (C10->C11) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		FinalClean(Contours[10][k], Contours[11][k], Length_min, Length_max, Center_dist_min);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		cout << Print_Type[k] << Fp_ms.count() << " ms" << endl;
	}

	//***** FOURTH WAY ****
	cout << endl << "===== FOURTH WAY =====" << endl;
	//Rotated Rectangle After First Clean
	cout << "Time Rotated Rectangle after First Clean (C1->C12) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		Rects(Contours[1][k], Contours[12][k], Length_min, Length_max);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		cout << Print_Type[k] << Fp_ms.count() << " ms" << endl;
	}

	//Final Clean
	cout << "Time Last Clean (C12->C13) on : " << endl;
	for (int k = 0; k < 3; ++k) {
		const auto T1 = high_resolution_clock::now();
		FinalClean(Contours[12][k], Contours[13][k], Length_min, Length_max, Center_dist_min);
		const duration<double, std::milli> Fp_ms = high_resolution_clock::now() - T1;
		cout << Print_Type[k] << Fp_ms.count() << " ms" << endl;
	}

	//***** Prints *****
	cout << endl << "Nb contours :" << endl;
	for (int j = 0; j < Contours.size(); ++j) {
		cout << Print_Methods[j] << " :\t";
		for (int k = 0; k < 3; ++k) {
			cout << "[" << j << "][" << k << "] : " << Contours[j][k].size() << "\t";
		}
		cout << endl;
	}

	cout << endl << "Nb Point On Contours : " << endl;
	for (int j = 1; j < Contours.size(); ++j) {
		cout << Print_Methods[j] << " : " << endl;
		for (int k = 0; k < 3; ++k) {
			cout << "\t[" << j << "][" << k << "] : ";
			printContourSize(Contours[j][k]);
			cout << endl;
		}
	}

	//***** Draws *****
	cout << endl << "Draws... ";
	for (int j = 1; j < Contours.size(); ++j) {
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
	for (int j = 1; j < Contours.size(); ++j) {
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

int main(int argc, char *argv[])
{
	//***** Init *****
	(void)argc;
	(void)argv;

	cout.precision(6);
	cout << fixed;
	for (int i = 0; i < NAMES.size(); ++i) {
		//TestsEdge(i);
		//TestsContour(i);
		TestsDLLFunction(i);
	}
	cout << endl << "That's all Folks !" << endl;
	_getch();
	return EXIT_SUCCESS;
}
