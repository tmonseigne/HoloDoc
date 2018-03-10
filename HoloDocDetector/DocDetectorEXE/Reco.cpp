	#include "Reco.hpp"
#include "opencv2/xfeatures2d.hpp"

#include <opencv2/highgui.hpp>
#include <iostream>

using namespace std;
using namespace cv::xfeatures2d;
using namespace cv;


void ExtractFeatures(const cv::Mat &src)
{
	vector<KeyPoint> Keypoints;
	Mat Descriptors;
	//-- Step 1: Detect the keypoints using SURF Detector
	const int minHessian = 400;
	Ptr<SURF> detector = SURF::create(minHessian);
	detector->detectAndCompute(src, noArray(), Keypoints, Descriptors);
	//detector->detect(src, Keypoints);
	cout << "Nb Keypoints : " << Keypoints.size() << endl;
	cout << "Size Descriptors : " << Descriptors.size << endl;
	cout << "Type Descriptors : " << Descriptors.type() << endl;
	//-- Draw keypoints
	Mat Im_Key;

	drawKeypoints(src, Keypoints, Im_Key, Scalar::all(-1), DrawMatchesFlags::DEFAULT);

	//-- Show detected (drawn) keypoints
	imshow("Keypoints", Im_Key);
	waitKey(0);
	/*
	surf.detect(src, keypoints1);
	drawKeypoints(src, keypoints1, outImg1, Scalar(255, 255, 255), DrawMatchesFlags::DRAW_RICH_KEYPOINTS);

	namedWindow("SURF detector img1");
	imshow("SURF detector img1", outImg1);

	xfeatures2d::SurfDescriptorExtractor surfDesc;
	Mat descriptors1, descriptors2;
	surfDesc.compute(image1, keypoints1, descriptors1);
	surfDesc.compute(image2, keypoints2, descriptors2);
	*/
}

