#pragma once
#include <highgui.h>
#include <cv.h>
#include <cxcore.h>

class CFeature
{
public:
	virtual int ImportObjectFeature(IplImage** arrObjects, int nObject) = 0;
	virtual int FindMostMatching(IplImage* img) = 0;
};
