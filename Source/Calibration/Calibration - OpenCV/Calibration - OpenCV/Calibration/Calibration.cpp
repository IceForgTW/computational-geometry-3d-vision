#include "Calibration.h"

CCalibration::CCalibration(void)
{
}

CCalibration::~CCalibration(void)
{
}

int CCalibration::CalibrateFromFiles(int nImages, int nChessRow, int nChessCol, float nSquareSize, CvMat* intrinsic, CvMat* distortion)
{
	int nChessSize = nChessRow*nChessCol;
	int nAllPoints = nImages*nChessSize;
	int i, j, k;
	int corner_count, found;
	int* p_count = new int[nImages];
	IplImage **src_img = new IplImage*[nImages];
	CvSize pattern_size = cvSize (nChessCol, nChessRow);
	CvPoint3D32f* objects = new CvPoint3D32f[nAllPoints];
	CvPoint2D32f *corners = (CvPoint2D32f *) cvAlloc (sizeof (CvPoint2D32f) * nAllPoints);
	CvMat object_points;
	CvMat image_points;
	CvMat point_counts;

	CvMat *PMatrix = cvCreateMat (3, 4, CV_32FC1);
	CvMat *tmp33 = cvCreateMat(3,3,CV_32FC1);
	CvMat *tmp31 = cvCreateMat(3,1,CV_32FC1);
	CvMat *roTrans = cvCreateMat(3,3,CV_32FC1);
	CvMat *translationTrans = cvCreateMat(3,1,CV_32FC1);

	// (1) Load Images
	for (i = 0; i < nImages; i++) 
	{
		char buf[32];
		sprintf (buf, "%d.png", i);
		if ((src_img[i] = cvLoadImage (buf, CV_LOAD_IMAGE_COLOR)) == NULL)
			fprintf (stderr, "cannot load image file : %s\n", buf);
	}

	// (2) Load 3D Object points
	for (i = 0; i < nImages; i++) {
		for (j = 0; j < nChessRow; j++) {
			for (k = 0; k < nChessCol; k++) 
			{
				objects[i * nChessSize + j * nChessCol + k].x = j * nSquareSize;
				objects[i * nChessSize + j * nChessCol + k].y = k * nSquareSize;
				objects[i * nChessSize + j * nChessCol + k].z = 0.0; // You know why?
			}
		}
	}
	cvInitMatHeader (&object_points, nAllPoints, 3, CV_32FC1, objects);

	// (3) Find chesk board corners
	int found_num = 0;
	//cvNamedWindow ("Calibration", CV_WINDOW_AUTOSIZE);
	for (i = 0; i < nImages; i++) 
	{
		found = cvFindChessboardCorners (src_img[i], pattern_size, &corners[i * nChessSize], &corner_count);
		fprintf (stderr, "%02d...", i);
		if (found) 
		{
			fprintf (stderr, "ok\n");
			found_num++;
		}
		else 
		{
			fprintf (stderr, "fail\n");
		}
		IplImage *src_gray = cvCreateImage (cvGetSize (src_img[i]), IPL_DEPTH_8U, 1);
		cvCvtColor (src_img[i], src_gray, CV_BGR2GRAY);
		cvFindCornerSubPix (src_gray, &corners[i * nChessSize], corner_count, cvSize (3, 3), cvSize (-1, -1), cvTermCriteria (CV_TERMCRIT_ITER | CV_TERMCRIT_EPS, 20, 0.03));
		p_count[i] = corner_count;
		cvWaitKey (0);
	}

	if (found_num != nImages)
		return -1;
	cvInitMatHeader (&image_points, nAllPoints, 1, CV_32FC2, corners);
	cvInitMatHeader (&point_counts, nImages, 1, CV_32SC1, p_count);

	// (4) Calibrate
	cvCalibrateCamera2 (&object_points, &image_points, &point_counts, cvSize (640, 480), intrinsic, distortion);

	cvDestroyWindow("Calibration");
	// Release all temp files
	for (i = 0; i < nImages; i++)
		cvReleaseImage (&src_img[i]);
	delete p_count;
	delete src_img;
	delete objects;
	return 1;
}