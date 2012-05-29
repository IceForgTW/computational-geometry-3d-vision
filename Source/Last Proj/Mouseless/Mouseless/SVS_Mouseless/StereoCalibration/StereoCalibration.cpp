#include <iostream>
#include <highgui.h>
#include <cv.h>
#include <cxcore.h>
#include <windows.h>

using namespace std;

#define SQR(x) ((x)*(x))

int IMAGE_NUM = 20;
int SKIP_NUM = 30;
int PAT_ROW = 6;
int PAT_COL = 9;
int PAT_SIZE;
int ALL_POINTS;
double CHESS_SIZE = 24.5;
int IMAGE_WIDTH = 320;
int IMAGE_HEIGHT = 240;
int iCoGhiFile = 0;
char strFileName[100];

double dist(double a1, double b1, double c1, double a2, double b2, double c2)
{
	return sqrt(SQR(a1-a2)+SQR(b1-b2)+SQR(c1-c2));
}


void dumpToFile(CvMat *mat, char *name)
{
	FILE *file;
	file = fopen(name, "w");
	
	for (int i = 0; i < mat->rows; i++)
	{
		for (int j = 0; j < mat->cols; j++)
		{
			int idx = i*mat->cols + j;
			fprintf(file, "%f\t%f\t%f\n", mat->data.fl[idx*3], mat->data.fl[idx*3+1], mat->data.fl[idx*3+2]);
		}
	}
	fclose(file);
}

void showMat(char *name, CvMat* mat)
{
	printf("%s\n",name);
	for (int i = 0; i < mat->rows; i++)
	{
		for (int j = 0; j < mat->cols; j++)
		{
			printf("%f\t", mat->data.db[i*mat->cols + j]);
		}
		printf("\n");
	}
	printf("\n");
	
}

//width, height, pat_row, pat_col, chess_size
int main(int argc, char* argv[])
{
	if (argc != 10)
		return -1;

	IMAGE_WIDTH = atoi(argv[1]);
	IMAGE_HEIGHT = atoi(argv[2]);

	PAT_ROW = atoi(argv[3]);
	PAT_COL = atoi(argv[4]);

	CHESS_SIZE = atof(argv[5]);
	IMAGE_NUM = atoi(argv[6]);
	SKIP_NUM = atoi(argv[7]);
	iCoGhiFile = atoi(argv[8]);
	strcpy(strFileName, argv[9]);


	// xac dinh tham so
	PAT_SIZE = PAT_ROW*PAT_COL;
	ALL_POINTS = IMAGE_NUM*PAT_SIZE;

	// declare variables
	int i,j,k;
	int cnt = 0;
	int found1, found2;
	int corner_count1, corner_count2;
	CvPoint3D32f* objects = new CvPoint3D32f[ALL_POINTS];
	CvMat object_points;
	CvMat image_points1, image_points2;
	CvSize pattern_size = cvSize (PAT_COL, PAT_ROW);
	CvPoint2D32f *corners1 = new CvPoint2D32f[ALL_POINTS];
	CvPoint2D32f *corners2 = new CvPoint2D32f[ALL_POINTS];
	CvMat *points_count = cvCreateMat(1, IMAGE_NUM, CV_32SC1);

	CvMat *_M1 = cvCreateMat(3, 3, CV_64F);
    CvMat *_M2 = cvCreateMat(3, 3, CV_64F);
    CvMat *_D1 = cvCreateMat(1, 5, CV_64F);
    CvMat *_D2 = cvCreateMat(1, 5, CV_64F);
    CvMat *matR = cvCreateMat(3, 3, CV_64F);
    CvMat *matT = cvCreateMat(3, 1, CV_64F);
    CvMat *matE = cvCreateMat(3, 3, CV_64F);
    CvMat *matF = cvCreateMat(3, 3, CV_64F);
	CvMat *matQ = cvCreateMat(4, 4, CV_64F);

	// init points' 3d coord
	for (i = 0; i < IMAGE_NUM; i++) {
		for (j = 0; j < PAT_ROW; j++) 
		{
			for (k = 0; k < PAT_COL; k++) 
			{
				objects[i * PAT_SIZE + j * PAT_COL + k].x = j * CHESS_SIZE;
				objects[i * PAT_SIZE + j * PAT_COL + k].y = k * CHESS_SIZE;
				objects[i * PAT_SIZE + j * PAT_COL + k].z = 0.0;
			}
		}
	}
	cvInitMatHeader (&object_points, 1, ALL_POINTS, CV_32FC3, objects);

	// init pointsCount
	for (i = 0; i < IMAGE_NUM; i++)
	{
		points_count->data.i[i] = PAT_SIZE;
	}

	// init camera
	IplImage *frame1, *frame2;
	CvCapture *capture1, *capture2;
	capture1 = cvCaptureFromCAM(0);
	cvSetCaptureProperty( capture1, CV_CAP_PROP_FRAME_WIDTH, IMAGE_WIDTH);
	cvSetCaptureProperty( capture1, CV_CAP_PROP_FRAME_HEIGHT, IMAGE_HEIGHT);
	capture2 = cvCaptureFromCAM(1);
	cvSetCaptureProperty( capture2, CV_CAP_PROP_FRAME_WIDTH, IMAGE_WIDTH);
	cvSetCaptureProperty( capture2, CV_CAP_PROP_FRAME_HEIGHT, IMAGE_HEIGHT);

	

	cvNamedWindow ("Cam 1", CV_WINDOW_AUTOSIZE);
	cvNamedWindow ("Cam 2", CV_WINDOW_AUTOSIZE);
	cvNamedWindow ("Calib Image 1", CV_WINDOW_AUTOSIZE);
	cvNamedWindow ("Calib Image 2", CV_WINDOW_AUTOSIZE);

	bool bStop = false;
	for (i = 0; i < IMAGE_NUM; i++) 
	{
		bStop = false;
		while (true)
		{
			frame1 = cvQueryFrame(capture1);
			cvShowImage("Cam 1", frame1);

			frame2 = cvQueryFrame(capture2);
			cvShowImage("Cam 2", frame2);

			if (cnt++ % SKIP_NUM == 0)
			{
				found1 = cvFindChessboardCorners (frame1, pattern_size, &corners1[i * PAT_SIZE], &corner_count1);
				found2 = cvFindChessboardCorners (frame2, pattern_size, &corners2[i * PAT_SIZE], &corner_count2);
				if (found1 && found2)
				{
					if (iCoGhiFile != 0)
					{
						char s[100];
						sprintf(s, "Cam1_%d.png",i);
						cvSaveImage(s, frame1);
						sprintf(s, "Cam2_%d.png",i);
						cvSaveImage(s, frame2);
					}
						printf("%d:\tOK\n",i);
					break;
				}
			}
			if (cvWaitKey(5) == 27)
			{
				bStop = true;
				break;
			}
		}
		if (bStop)
			break;

		IplImage *src_gray1 = cvCreateImage (cvGetSize (frame1), IPL_DEPTH_8U, 1);
		cvCvtColor (frame1, src_gray1, CV_BGR2GRAY);
		cvFindCornerSubPix (src_gray1, &corners1[i * PAT_SIZE], corner_count1,
			cvSize (3, 3), cvSize (-1, -1), cvTermCriteria (CV_TERMCRIT_ITER | CV_TERMCRIT_EPS, 20, 0.03));
		cvDrawChessboardCorners (frame1, pattern_size, &corners1[i * PAT_SIZE], corner_count1, found1);
		cvShowImage ("Calib Image 1", frame1);

		IplImage *src_gray2 = cvCreateImage (cvGetSize (frame2), IPL_DEPTH_8U, 1);
		cvCvtColor (frame2, src_gray2, CV_BGR2GRAY);
		cvFindCornerSubPix (src_gray2, &corners2[i * PAT_SIZE], corner_count2,
			cvSize (3, 3), cvSize (-1, -1), cvTermCriteria (CV_TERMCRIT_ITER | CV_TERMCRIT_EPS, 20, 0.03));
		cvDrawChessboardCorners (frame2, pattern_size, &corners2[i * PAT_SIZE], corner_count2, found2);
		cvShowImage ("Calib Image 2", frame2);

		cvReleaseImage(&src_gray1); 
		cvReleaseImage(&src_gray2);
	}
	if(bStop)
		goto end;
	cvInitMatHeader (&image_points1, 1, ALL_POINTS, CV_32FC2, corners1);
	cvInitMatHeader (&image_points2, 1, ALL_POINTS, CV_32FC2, corners2);

	// stereo calibrate's initialization
	cvSetIdentity(_M1);
    cvSetIdentity(_M2);
    cvZero(_D1);
    cvZero(_D2);

	printf("Running stereo calibration ...");
    fflush(stdout);
	cvStereoCalibrate( &object_points, &image_points1,
        &image_points2, points_count,
        _M1, _D1, _M2, _D2,
		cvSize(IMAGE_WIDTH, IMAGE_HEIGHT), matR, matT, matE, matF,
        cvTermCriteria(CV_TERMCRIT_ITER+
		CV_TERMCRIT_EPS, 100, 1e-5),
        CV_CALIB_FIX_ASPECT_RATIO +
        CV_CALIB_ZERO_TANGENT_DIST +
        CV_CALIB_FIX_K3);
	printf(" done\n\n");

	printf("Dumping to XML ...");
    fflush(stdout);
	CvFileStorage *fs;
	fs = cvOpenFileStorage (strFileName, 0, CV_STORAGE_WRITE);
	cvWrite (fs, "K1", _M1);
	cvWrite (fs, "K2", _M2);
	cvWrite (fs, "D1", _D1);
	cvWrite (fs, "D2", _D2);
	cvWrite (fs, "R", matR);
	cvWrite (fs, "T", matT);
	cvWrite (fs, "E", matE);
	cvWrite (fs, "F", matF);
	cvReleaseFileStorage (&fs);

	// Release
end:
	cvReleaseCapture(&capture1); 
    cvReleaseCapture(&capture2);
	cvDestroyWindow("Cam 1"); 
    cvDestroyWindow("Cam 2");
	cvDestroyWindow ("Calib Image 1");
	cvDestroyWindow ("Calib Iamge 2");

	cvReleaseMat(&points_count);
	cvReleaseMat(&_M1);
    cvReleaseMat(&_M2);
    cvReleaseMat(&_D1);
    cvReleaseMat(&_D2);
    cvReleaseMat(&matR);
    cvReleaseMat(&matT);
    cvReleaseMat(&matE);
    cvReleaseMat(&matF);
	cvReleaseMat(&matQ);

	delete[] objects;
	delete[] corners1;
	delete[] corners2;
}