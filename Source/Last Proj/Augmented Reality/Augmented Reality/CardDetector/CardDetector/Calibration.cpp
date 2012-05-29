#include "stdafx.h"
#include "Calibration.h"
#include "Model.h"

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
	//CvMat *rotation = cvCreateMat (1, 3, CV_32FC1);
	//CvMat *translation = cvCreateMat (1, 3, CV_32FC1);

	CvMat *PMatrix = cvCreateMat (3, 4, CV_32FC1);
	CvMat *tmp33 = cvCreateMat(3,3,CV_32FC1);
	CvMat *tmp31 = cvCreateMat(3,1,CV_32FC1);
	CvMat *roTrans = cvCreateMat(3,3,CV_32FC1);
	CvMat *translationTrans = cvCreateMat(3,1,CV_32FC1);

	// (1)
	for (i = 0; i < nImages; i++) {
		char buf[32];
		sprintf (buf, "%d.png", i);
		if ((src_img[i] = cvLoadImage (buf, CV_LOAD_IMAGE_COLOR)) == NULL) {
			fprintf (stderr, "cannot load image file : %s\n", buf);
		}
	}

	// (2)
	for (i = 0; i < nImages; i++) {
		for (j = 0; j < nChessRow; j++) {
			for (k = 0; k < nChessCol; k++) {
				objects[i * nChessSize + j * nChessCol + k].x = j * nSquareSize;
				objects[i * nChessSize + j * nChessCol + k].y = k * nSquareSize;
				objects[i * nChessSize + j * nChessCol + k].z = 0.0;
			}
		}
	}
	cvInitMatHeader (&object_points, nAllPoints, 3, CV_32FC1, objects);

	// (3)
	int found_num = 0;
	//cvNamedWindow ("Calibration", CV_WINDOW_AUTOSIZE);
	for (i = 0; i < nImages; i++) {
		found = cvFindChessboardCorners (src_img[i], pattern_size, &corners[i * nChessSize], &corner_count);
		fprintf (stderr, "%02d...", i);
		if (found) {
			fprintf (stderr, "ok\n");
			found_num++;
		}
		else {
			fprintf (stderr, "fail\n");
		}
		// (4)
		IplImage *src_gray = cvCreateImage (cvGetSize (src_img[i]), IPL_DEPTH_8U, 1);
		cvCvtColor (src_img[i], src_gray, CV_BGR2GRAY);
		cvFindCornerSubPix (src_gray, &corners[i * nChessSize], corner_count,
			cvSize (3, 3), cvSize (-1, -1), cvTermCriteria (CV_TERMCRIT_ITER | CV_TERMCRIT_EPS, 20, 0.03));
		p_count[i] = corner_count;
		cvWaitKey (0);
	}

	if (found_num != nImages)
		return -1;
	cvInitMatHeader (&image_points, nAllPoints, 1, CV_32FC2, corners);
	cvInitMatHeader (&point_counts, nImages, 1, CV_32SC1, p_count);

	// (5)
	cvCalibrateCamera2 (&object_points, &image_points, &point_counts, cvSize (640, 480), intrinsic, distortion);

	/*  (6) ***************** TEST EXTRINSIC PARAMETERS ******************
	CvMat sub_image_points, sub_object_points;
	int base = 0;
	cvNamedWindow ("Calibration", CV_WINDOW_AUTOSIZE);
	CModel myBox("box.txt");
	for (i=0; i<nImages; i++)
	{
		cvGetRows (&image_points, &sub_image_points, base * nChessSize, (base + 1) * nChessSize);
		cvGetRows (&object_points, &sub_object_points, base * nChessSize, (base + 1) * nChessSize);
		cvFindExtrinsicCameraParams2 (&sub_object_points, &sub_image_points, intrinsic, distortion, rotation, translation);

		CvMat *rotMat = cvCreateMat(3, 3, CV_32FC1);
		cvRodrigues2(rotation, rotMat);

		//cvTranspose(rotMat,roTrans);
		cvTranspose(translation,translationTrans);
		CvMat* tmp34 = cvCreateMat(3,4,CV_32FC1);
		for (int i = 0; i < 3; i++)
			for (int j=0; j<3; j++)
				cvmSet(tmp34,i,j,cvmGet(rotMat,i,j));
		for (int i = 0; i < 3; i++)
			cvmSet(tmp34,i,3,cvmGet(translationTrans,i,0));
		cvMatMul(intrinsic,tmp34,PMatrix);

		myBox.Draw(src_img[i], PMatrix, cvScalar(0, 255, 0), 2);

		cvShowImage ("Calibration", src_img[i]);
		cvWaitKey(0);
		base++;
	}*/
	cvDestroyWindow("Calibration");
	// Luu ket qua
/*	CvFileStorage *fs;
	fs = cvOpenFileStorage ("camera.xml", 0, CV_STORAGE_WRITE);
	cvWrite (fs, "intrinsic", intrinsic);
	cvWrite (fs, "rotation", rotation);
	cvWrite (fs, "translation", translation);
	cvWrite (fs, "distortion", distortion);
	cvReleaseFileStorage (&fs);*/

	for (i = 0; i < nImages; i++) {
		cvReleaseImage (&src_img[i]);
	}
	delete p_count;
	delete src_img;
	delete objects;
	return 1;
}

int CCalibration::CalibrateFromCAMorAVI( int nImages, int nChessRow, int nChessCol, int nSquareSize, int nSkip, CvMat* intrinsic, CvMat* distortion, int fromCamera, char* fName)
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
	CvMat *rotation = cvCreateMat (1, 3, CV_32FC1);
	CvMat *translation = cvCreateMat (1, 3, CV_32FC1);

	// (1)
	for (i = 0; i < nImages; i++) {
		for (j = 0; j < nChessRow; j++) {
			for (k = 0; k < nChessCol; k++) {
				objects[i * nChessSize + j * nChessCol + k].x = (float)j * nSquareSize;
				objects[i * nChessSize + j * nChessCol + k].y = (float)k * nSquareSize;
				objects[i * nChessSize + j * nChessCol + k].z = 0.0;  
			}
		}
	}
	cvInitMatHeader (&object_points, nAllPoints, 3, CV_32FC1, objects);

	// (2)
	CvCapture *capture = NULL;
	if (fromCamera)
		capture = cvCaptureFromCAM(0);
	else
		capture = cvCaptureFromAVI(fName);
	assert(capture);

	int found_num = 0;
	cvNamedWindow ("Calibration", CV_WINDOW_AUTOSIZE);
	cvNamedWindow ("Webcam", CV_WINDOW_AUTOSIZE);
	int c = 0;
	for (i = 0; i < nImages; i++) 
	{
		IplImage * frame;
		while (true)
		{
			frame = cvQueryFrame(capture);
			cvShowImage("Webcam", frame);

			if (c++ % nSkip == 0)
			{
				found = cvFindChessboardCorners (frame, pattern_size, &corners[i * nChessSize], &corner_count);
				if (found) 
				{
					char s[100];
					sprintf(s, "%d.png", i);
					cvSaveImage(s, frame);
					src_img[i] = cvCloneImage(frame);
					fprintf (stderr, "ok\n");
					found_num++;
					break;
				}
			}
			cvWaitKey(5);
		}
		fprintf (stderr, "%02d...", i);

		// (4)
		IplImage *src_gray = cvCreateImage (cvGetSize (src_img[i]), IPL_DEPTH_8U, 1);
		cvCvtColor (src_img[i], src_gray, CV_BGR2GRAY);
		cvFindCornerSubPix (src_gray, &corners[i * nChessSize], corner_count,
			cvSize (3, 3), cvSize (-1, -1), cvTermCriteria (CV_TERMCRIT_ITER | CV_TERMCRIT_EPS, 20, 0.03));
		cvDrawChessboardCorners (src_img[i], pattern_size, &corners[i * nChessSize], corner_count, found);
		p_count[i] = corner_count;
		cvShowImage ("Calibration", src_img[i]);
		//cvWaitKey (0);
	}
	cvDestroyWindow ("Calibration");

	if (found_num != nImages)
		return -1;
	cvInitMatHeader (&image_points, nAllPoints, 1, CV_32FC2, corners);
	cvInitMatHeader (&point_counts, nImages, 1, CV_32SC1, p_count);
	// (5)
	cvCalibrateCamera2 (&object_points, &image_points, &point_counts, cvSize (640, 480), intrinsic, distortion);

	// (6)
	/*
	CvMat sub_image_points, sub_object_points;
	int base = 0;
	cvGetRows (&image_points, &sub_image_points, base * nChessSize, (base + 1) * nChessSize);
	cvGetRows (&object_points, &sub_object_points, base * nChessSize, (base + 1) * nChessSize);
	cvFindExtrinsicCameraParams2 (&sub_object_points, &sub_image_points, intrinsic, distortion, rotation, translation);
	*/
	// (7)ToXML
	/*
	CvFileStorage *fs;
	fs = cvOpenFileStorage ("camera.xml", 0, CV_STORAGE_WRITE);
	cvWrite (fs, "intrinsic", intrinsic);
	cvWrite (fs, "rotation", rotation);
	cvWrite (fs, "translation", translation);
	cvWrite (fs, "distortion", distortion);
	cvReleaseFileStorage (&fs);*/

	for (i = 0; i < nImages; i++) 
		cvReleaseImage (&src_img[i]);
	delete p_count;
	delete src_img;
	delete objects;
	return 1;
}