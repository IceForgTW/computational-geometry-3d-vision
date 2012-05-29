#pragma once
#include <highgui.h>
#include <cxcore.h>
#include <cv.h>
#include <fstream>
#include <vector>



class CCalibration
{
public:
	CCalibration(void);
	~CCalibration(void);
	int CalibrateFromFiles(int nImages, int nChessRow, int nChessCol, float nSquareSize, CvMat* intrinsic, CvMat* distortion);
	int CalibrateFromCAMorAVI(int nImages, int nChessRow, int nChessCol, int nSquareSize, int nSkip, CvMat* intrinsic, CvMat* distortion, int fromCamera, char* fName = NULL);
};
