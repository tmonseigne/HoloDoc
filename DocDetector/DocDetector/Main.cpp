#include <iostream>
#include <stdlib.h>
#include <conio.h>	// _getch
#include <opencv2/highgui.hpp>
#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>
#include "DocDetector.hpp"

using namespace std;
using namespace cv;

const string path = "../res/";
const string NAME = "A.jpg";

int main(int argc, char *argv[])
{
	//***** Init *****
	(void)argc;
	(void)argv;
	cout << "Test Begin..." << endl;
	const Mat src = imread(path+NAME, CV_LOAD_IMAGE_COLOR);
	if (src.cols == 0 || src.rows == 0)	return EXIT_FAILURE;

	//***** Computes *****
	Mat edge, linesdraw, shapesdraws;
	BinaryEdgeDetector(src, edge);

	cout << "Test End..." << endl;

	//***** Show *****
	cvNamedWindow("Src", CV_WINDOW_AUTOSIZE);
	imshow("Src", src);

	cvNamedWindow("Edge", CV_WINDOW_AUTOSIZE);
	imshow("Edge", edge);

	//**** Save *****
	imwrite(path + "B.jpg", edge);

	cvWaitKey(0);			// Pause le temps que l'utilisateur appuie sur une touche
	cvDestroyAllWindows();	// Destruction des fenetres
	
	return EXIT_SUCCESS;
}
