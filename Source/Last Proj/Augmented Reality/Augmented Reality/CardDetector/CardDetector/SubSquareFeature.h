#pragma once
#include "feature.h"
#include <highgui.h>
#include <cv.h>
#include <cxcore.h>
#include <vector>
#include <vector>
#include <iostream>

using namespace std;

class CSubSquareFeature :
	public CFeature
{
	int _nObject;
	vector<int> objectFeature;
public:
	CSubSquareFeature(void);
	~CSubSquareFeature(void);
	virtual int ImportObjectFeature(IplImage** arrObjects, int nObject);
	virtual int FindMostMatching(IplImage* img);
};
