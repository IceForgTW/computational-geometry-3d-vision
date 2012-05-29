#pragma once
#include <highgui.h>
#include <cv.h>

class CCalibration
{
public:
	CCalibration(void);
	~CCalibration(void);
	int CalibrateFromFiles(int nImages, int nChessRow, int nChessCol, float nSquareSize, CvMat* intrinsic, CvMat* distortion);
};