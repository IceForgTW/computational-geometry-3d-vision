// CardDetector.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "SquareDetection.h"
#include "FeatureManager.h"
#include "Model.h"
#include <highgui.h>
#include <cv.h>
#include <cxcore.h>
#include <vector>

using namespace std;

#ifdef _MANAGED
#pragma managed(push, off)
#endif


CvMat *intrinsic;
CvMat *distortion;
CSquareDetection SquareDetection(50 /*Canny threshold*/, 
								 0.5f/*Cosine Thresh*/, 
								 0.3f/*Edge Ratio Thresh*/, 
								 0.008f /*min area*/, 
								 0.15f /*max area*/);
CFeatureManager FeatureManager(cvSURFParams(500, 1), 5);
CModel myBox("box.txt");


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
    return TRUE;
}


void SetCalibrationInfoFromXML(char* xml)
{
	CvFileStorage *fs;
	CvFileNode *param;

	fs = cvOpenFileStorage (xml, 0, CV_STORAGE_READ);
	param = cvGetFileNodeByName (fs, NULL, "intrinsic");
	intrinsic = (CvMat *) cvRead (fs, param);
	param = cvGetFileNodeByName (fs, NULL, "distortion");
	distortion = (CvMat *) cvRead (fs, param);
	cvReleaseFileStorage (&fs);
}

void SetCalibrationInfo(float* intrin, float* distor)
{
	if (intrinsic)
		cvReleaseMat(&intrinsic);
	if (distortion)
		cvReleaseMat(&distortion);
	intrinsic = cvCreateMat (3, 3, CV_32FC1);
	distortion = cvCreateMat (1, 4, CV_32FC1);

	for (int i = 0; i < 9; i++)
		cvSetReal2D(intrinsic, i/3, i%3, intrin[i]);
	for (int i = 0; i < 4; i++)
		cvSetReal2D(distortion, 0, i, distor[i]);
}

void LearnObject(char **objFile, int* CardSizeWidth, int* CardSizeHeight, int nObject)
{
	IplImage** object = new IplImage*[nObject];
	int* arrRealObjWidth = new int[nObject];
	int* arrRealObjHeight = new int[nObject];

	for (int i = 0; i < nObject; i++)
	{
		object[i] = cvLoadImage( objFile[i], CV_LOAD_IMAGE_GRAYSCALE );
		if( !object[i])
			exit(-1);
		arrRealObjWidth[i] = CardSizeWidth[i];
		arrRealObjHeight[i] = CardSizeHeight[i];
	}
	
	FeatureManager.ImportObjectFeature(object, nObject, arrRealObjWidth, arrRealObjHeight);
}

void DetectObject(IplImage* frame, int &nDetectedObject, int* ObjectID, int* ObjectPositions, int bFromCam)
{
	IplImage* gray_frame = cvCreateImage(cvSize(frame->width, frame->height), 8, 1);
	if (bFromCam)
		cvCvtColor(frame, gray_frame, CV_BGR2GRAY);
	else
		cvCvtColor(frame, gray_frame, CV_RGB2GRAY);
	nDetectedObject = 0;
	int idObject = -1;
	CvSeq *squares = SquareDetection.FindSquares(gray_frame );

	// Get image from squares info
	FeatureManager.BeginMatching();
	for (int sqrIdx = 0; sqrIdx < squares->total; sqrIdx+=4)
	{
		CvPoint* V[4];
		V[0] = (CvPoint*)cvGetSeqElem(squares, sqrIdx);
		V[1] = (CvPoint*)cvGetSeqElem(squares, sqrIdx+1);
		V[2] = (CvPoint*)cvGetSeqElem(squares, sqrIdx+2);
		V[3] = (CvPoint*)cvGetSeqElem(squares, sqrIdx+3);
		int top = min(min(min(V[0]->y, V[1]->y), V[2]->y),V[3]->y);
		int left = min(min(min(V[0]->x, V[1]->x), V[2]->x),V[3]->x);
		int bottom = max(max(max(V[0]->y, V[1]->y), V[2]->y),V[3]->y);
		int right = max(max(max(V[0]->x, V[1]->x), V[2]->x),V[3]->x);

		// Lay anh cua la bai ra
		CvRect region = cvRect(left, top, right-left, bottom-top);
		cvSetImageROI(gray_frame , region);
		IplImage *candidate = cvCreateImage(cvSize(region.width, region.height), 8, 1);
		cvCopyImage(gray_frame , candidate);
		cvResetImageROI(gray_frame);

		// Tim xem no match voi cai nao
		//int idObject = FeatureManager.FindMostMatching(candidate);
		idObject = FeatureManager.FindMostMatching(candidate, top, left, bottom, right, V);
		cvReleaseImage(&candidate);
		
		if (idObject > -1)
		{
			ObjectID[nDetectedObject] = idObject;
			ObjectPositions[nDetectedObject*8] = V[0]->x;
			ObjectPositions[nDetectedObject*8+1] = V[0]->y;
			ObjectPositions[nDetectedObject*8+2] = V[1]->x;
			ObjectPositions[nDetectedObject*8+3] = V[1]->y;
			ObjectPositions[nDetectedObject*8+4] = V[2]->x;
			ObjectPositions[nDetectedObject*8+5] = V[2]->y;
			ObjectPositions[nDetectedObject*8+6] = V[3]->x;
			ObjectPositions[nDetectedObject*8+7] = V[3]->y;
			nDetectedObject++;
		}
	}
	FeatureManager.EndMatching();
}

void DrawBox(IplImage* frame, int nDetectedObject, int* ObjectID, int* ObjectPosition)
{
	vector<CvMat*> v = FeatureManager.EstimateProjectiveMatrix(intrinsic, distortion, nDetectedObject, ObjectID, ObjectPosition);
	for (int i = 0; i < nDetectedObject; i++)
		myBox.Draw(frame, v[i], CV_RGB(255,0,0), 2);
}

void GetProjectionMatrix(int ObjectID, int* ObjectPosition, 
				  float* m11, float* m12, float* m13, float* m14,
				  float* m21, float* m22, float* m23, float* m24,
				  float* m31, float* m32, float* m33, float* m34)
{
	FeatureManager.GetProMatrix(intrinsic, distortion, ObjectID, ObjectPosition,
		m11, m12, m13, m14,
		m21, m22, m23, m24,
		m31, m32, m33, m34);
}

#ifdef _MANAGED
#pragma managed(pop)
#endif

