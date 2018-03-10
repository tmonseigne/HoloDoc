#include "Contours.hpp"
#include "Misc.hpp"
#include <opencv2/imgproc.hpp>
#include <set>

using namespace std;
using namespace cv;

//Basic fast clean only number of point and length
//Possibility to change arclength by boundingrect maybe some contour can be suppress in some case but its a little slower
void CleanBasic(const vector<vector<Point>> &in, vector<vector<Point>> &out,
	const double min_length, const double max_length)
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
	const double min_length, const double max_length)
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
	const double min_length, const double max_length)
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
	const double min_length, const double max_length)
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
	const double min_length, const double max_length)
{
	out.clear();
	for (const vector<Point> &cont : in) {
		const double Peri = arcLength(cont, true);
		if (min_length <= Peri && Peri <= max_length) {
			const int Nb_points = int(cont.size());
			if (Nb_points == 4) {
				out.push_back(cont);
			}
			else if (Nb_points > 4) {
				//Ids Order : 
				// 0 : Point on the left side of Diagonal
				// 1 : Point on the right side of Diagonal
				// 2 : First Point of Diag (Farest Point)
				// 3 : Second Point Of Diag (Farest of the First)
				int Ids[4] = { 0, 0, 0, 0 };
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
				double Areas_max[2] = { 0.0, 0.0 };
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
	const double min_length, const double max_length,
	const double min_center_dist, const double side_ratio)
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
