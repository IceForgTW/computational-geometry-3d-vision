#include <stdio.h>
#include <iostream>
#include <fstream>
#include <cv.h>
#include <highgui.h>
#include <cvaux.h>
#include <vector>
#include <cxcore.h>
//#include <omp.h>
#include "Calibration.h"


using namespace std;

void ShowMatrix(CvMat* m)
{
	for (int i = 0; i < m->height; i++)
	{
		for (int j = 0; j < m->width; j++)
			cout << CV_MAT_ELEM(*m, float, i, j) << " ";
		cout << endl;
	}
}

int main (int argc, char *argv[])
{
	// Calibrate
	CvMat *intrinsic = cvCreateMat (3, 3, CV_32FC1);
	CvMat *distortion = cvCreateMat (1, 4, CV_32FC1);
	CCalibration myCalib;
	myCalib.CalibrateFromFiles(12, 6, 9, 24.2f, intrinsic, distortion);
	ShowMatrix(intrinsic);
}