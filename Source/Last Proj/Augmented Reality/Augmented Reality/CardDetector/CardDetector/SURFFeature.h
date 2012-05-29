#pragma once
#include "feature.h"
#include <highgui.h>
#include <cv.h>
#include <cxcore.h>
#include <vector>
#include <vector>
#include <iostream>

using namespace std;

class CSURFFeature :
	public CFeature
{
private:
	int _nObject;
	CvSURFParams _params;
	CvMemStorage* storage;
	CvSeq **objectKeypoints;
	CvSeq **objectDescriptors;
	int  _minimax;
public:
	CSURFFeature(CvSURFParams surpParam, int minimax);
	~CSURFFeature(void);
	virtual int ImportObjectFeature(IplImage** arrObjects, int nObject);
	virtual int FindMostMatching(IplImage* img);
	int NaiveNearestNeighbor( const float* vec, int laplacian, const CvSeq* model_keypoints, const CvSeq* model_descriptors );
	double SURFDescriptorDistance( const float* d1, const float* d2, double best, int length );
	int CountPairs(int objectID, const CvSeq* imageKeypoints, const CvSeq* imageDescriptors);
	void FindPairs(int objectID, const CvSeq* imageKeypoints, const CvSeq* imageDescriptors, vector<int>& ptpairs );
};
