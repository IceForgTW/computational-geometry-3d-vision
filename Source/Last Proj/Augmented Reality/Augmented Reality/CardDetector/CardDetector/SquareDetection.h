#pragma once
#include <highgui.h>
#include <cxcore.h>
#include <cv.h>
#include "MathHelper.h"


class CSquareDetection
{
	int		_CannyThresh;
	float	_CosineThresh;
	float	_EdgeThresh;
	float	_minPropotionArea;
	float	_maxPropotionArea;
	CvMemStorage* storage;
	CMathHelper MathHelper;
public:
	CSquareDetection(int CannyThresh, float CosineThresh, float EdgeThresh, float minPropotionArea, float maxPropotionArea);
	~CSquareDetection(void);

	// returns sequence of squares detected on the image.
	// the sequence is stored in the specified memory storage
	CvSeq* FindSquares( IplImage* img);

	int CountSubSquares( IplImage* img);

	// Check 4 vertexes of a polygon that is a square or not
	int Check4Vertexes(CvSeq* result, double consine_thres, double ratio_thres);

	// the function draws all the squares in the image
	void DrawSquares( IplImage* img, CvSeq* squares, int* flag, CvScalar* color);
};
