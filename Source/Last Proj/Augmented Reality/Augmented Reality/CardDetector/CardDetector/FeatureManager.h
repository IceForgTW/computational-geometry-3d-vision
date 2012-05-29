#pragma once
#include <highgui.h>
#include <cv.h>
#include <cxcore.h>
#include <vector>
#include <iostream>
#include "feature.h"
#include "SURFFeature.h"
#include "MathHelper.h"

using namespace std;

struct CARDInfo
{
	int objId;
	int top;
	int left;
	int bottom;
	int right;
	CvPoint StartPoint;
	CARDInfo(int _objId, int _top, int _left, int _bottom, int _right, CvPoint _StartPoint)
	{
		objId = _objId;
		top = _top;
		left = _left;
		bottom = _bottom;
		right = _right;
		StartPoint = _StartPoint;
	}
};

class CFeatureManager
{
private:
	int _nObject;
	int currFeature;
	vector<CARDInfo> PrevCards;
	vector<int> PrevState;
	int m_CardPosThres;
	vector<vector<CvPoint> >		src_corners;
	vector<vector<CvPoint> >		dst_corners;
	vector<vector<CvPoint3D32f> >	_3D_corners;
	vector<CFeature*>				features;
	CMathHelper						MathHelper;

public:
	CFeatureManager(CvSURFParams surpParam, int minimax);
	~CFeatureManager(void);
	void ImportObjectFeature(IplImage** arrObjects, int nObject, int* arrRealObjWidth, int* arrRealObjHeight);
	int FindMostMatching( IplImage* img, int top, int left, int bottom, int right, CvPoint** V);
	void EndMatching();
	void BeginMatching();
	vector<CvMat*> EstimateProjectiveMatrix(CvMat *intrinsic, CvMat *distortion, int nDetectedObject, int* ObjectID, int *ObjectPosition);
	void GetProMatrix(CvMat* intrinsic, CvMat* distortion, int ObjectID, int* ObjectPosition, 
		float* m11, float* m12, float* m13, float* m14, 
		float* m21, float* m22, float* m23, float* m24, 
		float* m31, float* m32, float* m33, float* m34);
};
