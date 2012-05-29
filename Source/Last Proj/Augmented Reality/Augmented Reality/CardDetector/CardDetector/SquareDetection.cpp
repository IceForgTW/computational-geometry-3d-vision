#include "stdafx.h"
#include "SquareDetection.h"

CSquareDetection::CSquareDetection(int CannyThresh, float CosineThresh, float EdgeThresh, float minPropotionArea, float maxPropotionArea)
{
	_CannyThresh = CannyThresh;
	_CosineThresh = CosineThresh;
	_EdgeThresh = EdgeThresh;
	_minPropotionArea = minPropotionArea;
	_maxPropotionArea = maxPropotionArea;
	storage = cvCreateMemStorage(0);
}

CSquareDetection::~CSquareDetection(void)
{
	cvReleaseMemStorage(&storage);
}

int CSquareDetection::Check4Vertexes(CvSeq* result, double consine_thres, double ratio_thres)
{
	CvPoint* V[4];
	for (int i = 0; i < 4; i++)
		V[i] = (CvPoint*)cvGetSeqElem( result, i );

	double s = 0;

	for(int i = 0; i < 4; i++ )
	{
		// find minimum angle between joint
		// edges (maximum of cosine)
		double t = fabs(MathHelper.angle(V[i], V[(i+2)%4], V[(i+1)%4]));
		s = s > t ? s : t;
	}

	// if cosines of all angles are small
	// (all angles are ~90 degree) then write quandrange
	// vertices to resultant sequence 
	if( s > consine_thres )
		return 0;

	// check do dai canh co tuong duong ko
	float ratio = MathHelper.sqrDistance(V[0], V[1])/(float)MathHelper.sqrDistance(V[1], V[2]);
	if (ratio > 4.0f || ratio < 0.25f)
		return 0;
	return 1;
}


CvSeq* CSquareDetection::FindSquares( IplImage* tgray )
{
	CvSeq* contours;
	int i, l, N = 11;
	double imgArea = tgray->width*tgray->height;
	CvSize sz = cvSize( tgray->width & -2, tgray->height & -2 );
	IplImage* gray = cvCreateImage( sz, 8, 1 ); 
	IplImage* pyr = cvCreateImage( cvSize(sz.width/2, sz.height/2), 8, 1 );
	CvSeq* result;
	// create empty sequence that will contain points -
	// 4 points per square (the square's vertices)
	CvSeq* squares = cvCreateSeq( 0, sizeof(CvSeq), sizeof(CvPoint), storage );

	// select the maximum ROI in the image
	// with the width and height divisible by 2
	cvSetImageROI( tgray, cvRect( 0, 0, sz.width, sz.height ));

	// down-scale and upscale the image to filter out the noise
	//cvPyrDown( tgray, pyr, 7 );
	//cvPyrUp( pyr, tgray, 7 );

	// try several threshold levels
	cvCanny( tgray, gray, 0, _CannyThresh, 5 );
	cvDilate( gray, gray, 0, 1 );

	for( l = 1; l < N-4; l++ )
	{
		cvThreshold( tgray, gray, (l+1)*255/N, 255, CV_THRESH_BINARY );
		// find contours and store them all as a list
		cvFindContours( gray, storage, &contours, sizeof(CvContour),
			CV_RETR_LIST, CV_CHAIN_APPROX_SIMPLE, cvPoint(0,0) );

		// test each contour
		while( contours )
		{
			// approximate contour with accuracy proportional
			// to the contour perimeter
			result = cvApproxPoly( contours, sizeof(CvContour), storage,
				CV_POLY_APPROX_DP, cvContourPerimeter(contours)*0.02, 0 );
			// square contours should have 4 vertices after approximation
			// relatively large area (to filter out noisy contours)
			// and be convex.
			// Note: absolute value of an area is used because
			// area may be positive or negative - in accordance with the
			// contour orientation
			double area = fabs(cvContourArea(result,CV_WHOLE_SEQ));
			if( result->total == 4 &&
				area < _maxPropotionArea*imgArea &&
				area > _minPropotionArea*imgArea &&
				cvCheckContourConvexity(result) )
			{
				// Kiem tra va sap xep lai vi tri dinh
				if (Check4Vertexes(result, _CosineThresh, _EdgeThresh))
				{
					// Dau vao mang ket qua
					for( i = 0; i < 4; i++ )
						cvSeqPush( squares,(CvPoint*)cvGetSeqElem( result, i ));
				}
			}

			// take the next contour
			contours = contours->h_next;
		}
	}
	// Loc lai
	int delta_thres = 30;
	int* flags = new int[squares->total/4];
	for (int i = 0; i < squares->total/4; i++)
		flags[i] = 0;

	CvSeq* sqrSeq = cvCreateSeq( 0, sizeof(CvSeq), sizeof(CvPoint), storage );
	CvPoint* V[4], *Vp[4];

	for (int i = 0; i < squares->total; i+=4)
	{
		if (!flags[i/4])
		{
			V[0] = (CvPoint*)cvGetSeqElem( squares, i );
			V[1] = (CvPoint*)cvGetSeqElem( squares, i+1 );
			V[2] = (CvPoint*)cvGetSeqElem( squares, i+2 );
			V[3] = (CvPoint*)cvGetSeqElem( squares, i+3 );

			for (int j = i+4; j < squares->total; j+= 4)
			{
				if (!flags[j/4])
				{
					Vp[0] = (CvPoint*)cvGetSeqElem( squares, j );
					Vp[1] = (CvPoint*)cvGetSeqElem( squares, j+1 );
					Vp[2] = (CvPoint*)cvGetSeqElem( squares, j+2 );
					Vp[3] = (CvPoint*)cvGetSeqElem( squares, j+3 );
					// xac dinh trung diem
					CvPoint M;
					M.x = (Vp[0]->x+Vp[2]->x)/2;
					M.y = (Vp[0]->y+Vp[2]->y)/2;

					if (MathHelper.ktNamTrong(V, 4, &M))
					{
						int d1 = max(MathHelper.sqrDistance(V[0], V[1]), MathHelper.sqrDistance(V[1], V[2]));
						int d2 = max(MathHelper.sqrDistance(Vp[0], Vp[1]), MathHelper.sqrDistance(Vp[1], Vp[2]));
						
						if ( d1 > d2)
						{
							V[0]->x = Vp[0]->x; V[0]->y = Vp[0]->y;
							V[1]->x = Vp[1]->x; V[1]->y = Vp[1]->y;
							V[2]->x = Vp[2]->x; V[2]->y = Vp[2]->y;
							V[3]->x = Vp[3]->x; V[3]->y = Vp[3]->y;
						}
						flags[j/4] = 1;
					}
					
				}
			}
		}
	}

	for (int i = 0; i < squares->total; i+=4)
	{
		if (!flags[i/4])
		{
			V[0] = (CvPoint*)cvGetSeqElem( squares, i );
			V[1] = (CvPoint*)cvGetSeqElem( squares, i+1 );
			V[2] = (CvPoint*)cvGetSeqElem( squares, i+2 );
			V[3] = (CvPoint*)cvGetSeqElem( squares, i+3 );

			// Kiem tra co nguoc chieu kim dong ho ko
			// Neu khong nguoc chieu kim dong ho thi hoan doi
			// Chinh lai huong cua la bai
			Line* l = MathHelper.ptDuongThang(V[0], V[1]);
			if (MathHelper.thePointLenLine(l, V[3]) > 0)
			{
				int temp = V[1]->x; V[1]->x = V[3]->x; V[3]->x  = temp;
					temp = V[1]->y; V[1]->y = V[3]->y; V[3]->y  = temp;
			}
			//MathHelper.SapDongHo(V);
			cvSeqPush(sqrSeq, V[0]);
			cvSeqPush(sqrSeq, V[1]);
			cvSeqPush(sqrSeq, V[2]);
			cvSeqPush(sqrSeq, V[3]);
		}
	}

	//cvClearSeq(squares);
	// release all the temporary images
	cvReleaseImage( &gray );
	cvReleaseImage( &pyr );
	//cvReleaseImage( &tgray );

	cvClearMemStorage(storage);
	return sqrSeq;
}


int CSquareDetection::CountSubSquares( IplImage* img )
{
	CvSeq* contours;
	int i, l, N = 11;
	double imgArea = img->width*img->height;
	CvSize sz = cvSize( img->width & -2, img->height & -2 );
	IplImage* tgray = cvCreateImage(cvSize(img->width, img->height), 8, 1);
	if (img->nChannels == 3)
		cvCvtColor(img, tgray, CV_BGR2GRAY);
	else
		cvCopyImage(img, tgray);
	IplImage* gray = cvCreateImage( sz, 8, 1 ); 
	IplImage* pyr = cvCreateImage( cvSize(sz.width/2, sz.height/2), 8, 1 );
	CvSeq* result;
	// create empty sequence that will contain points -
	// 4 points per square (the square's vertices)
	CvSeq* squares = cvCreateSeq( 0, sizeof(CvSeq), sizeof(CvPoint), storage );

	// select the maximum ROI in the image
	// with the width and height divisible by 2
	cvSetImageROI( tgray, cvRect( 0, 0, sz.width, sz.height ));

	// down-scale and upscale the image to filter out the noise
	//cvPyrDown( tgray, pyr, 7 );
	//cvPyrUp( pyr, tgray, 7 );

	// try several threshold levels
	cvCanny( tgray, gray, 0, _CannyThresh, 5 );
	cvDilate( gray, gray, 0, 1 );

	for( l = 1; l < N-4; l++ )
	{
		cvThreshold( tgray, gray, (l+1)*255/N, 255, CV_THRESH_BINARY );
		//cvNamedWindow("a");
		//cvShowImage("a", gray);
		//cvWaitKey(0);
		// find contours and store them all as a list
		cvFindContours( gray, storage, &contours, sizeof(CvContour),
			CV_RETR_LIST, CV_CHAIN_APPROX_SIMPLE, cvPoint(0,0) );

		// test each contour
		while( contours )
		{
			// approximate contour with accuracy proportional
			// to the contour perimeter
			result = cvApproxPoly( contours, sizeof(CvContour), storage,
				CV_POLY_APPROX_DP, cvContourPerimeter(contours)*0.09, 0 );
			// square contours should have 4 vertices after approximation
			// relatively large area (to filter out noisy contours)
			// and be convex.
			// Note: absolute value of an area is used because
			// area may be positive or negative - in accordance with the
			// contour orientation
			double area = fabs(cvContourArea(result,CV_WHOLE_SEQ));
			if( result->total == 4 &&
				area < _maxPropotionArea*imgArea &&
				area > _minPropotionArea*imgArea &&
				cvCheckContourConvexity(result) )
			{
				// Kiem tra va sap xep lai vi tri dinh
				if (Check4Vertexes(result, 0.3, 0.2))
				{
					// Dau vao mang ket qua
					for( i = 0; i < 4; i++ )
						cvSeqPush( squares,(CvPoint*)cvGetSeqElem( result, i ));
				}
			}

			// take the next contour
			contours = contours->h_next;
		}
	}
	// release all the temporary images
	cvReleaseImage( &gray );
	cvReleaseImage( &pyr );
	cvReleaseImage( &tgray );

	// Loc lai
	int delta_thres = 30;
	int* flags = new int[squares->total/4];
	for (int i = 0; i < squares->total/4; i++)
		flags[i] = 0;

	CvPoint* V[4], *Vp[4];

	for (int i = 0; i < squares->total; i+=4)
	{
		if (!flags[i/4])
		{
			V[0] = (CvPoint*)cvGetSeqElem( squares, i );
			V[1] = (CvPoint*)cvGetSeqElem( squares, i+1 );
			V[2] = (CvPoint*)cvGetSeqElem( squares, i+2 );
			V[3] = (CvPoint*)cvGetSeqElem( squares, i+3 );

			for (int j = i+4; j < squares->total; j+= 4)
			{
				if (!flags[j/4])
				{
					Vp[0] = (CvPoint*)cvGetSeqElem( squares, j );
					Vp[1] = (CvPoint*)cvGetSeqElem( squares, j+1 );
					Vp[2] = (CvPoint*)cvGetSeqElem( squares, j+2 );
					Vp[3] = (CvPoint*)cvGetSeqElem( squares, j+3 );
					// xac dinh trung diem
					CvPoint M;
					M.x = (Vp[0]->x+Vp[2]->x)/2;
					M.y = (Vp[0]->y+Vp[2]->y)/2;
					if (MathHelper.ktNamTrong(V, 4, &M))
					{
						if (MathHelper.sqrDistance(V[0], V[1]) > MathHelper.sqrDistance(Vp[0], Vp[1]))
						{
							V[0]->x = Vp[0]->x; V[0]->y = Vp[0]->y;
							V[1]->x = Vp[1]->x; V[1]->y = Vp[1]->y;
							V[2]->x = Vp[2]->x; V[2]->y = Vp[2]->y;
							V[3]->x = Vp[3]->x; V[3]->y = Vp[3]->y;
						}
						flags[j/4] = 1;
					}
				}
			}
		}
	}

	int totalSubSquare = 0;
	for (int i = 0; i < squares->total; i+=4)
		if (!flags[i/4])
			totalSubSquare++;

	//cvClearSeq(squares);
	cvClearMemStorage(storage);
	return totalSubSquare;
}

void CSquareDetection::DrawSquares( IplImage* img, CvSeq* squares, int* flag, CvScalar* color)
{
	int i;
	// read 4 sequence elements at a time (all vertices of a square)
	for( i = 0; i < squares->total; i += 4 )
	{
		if (flag[i/4] > -1)
		{
			CvPoint pt[4], *rect = pt;
			int count = 4;

			// read 4 vertices
			pt[0] = *((CvPoint*)cvGetSeqElem(squares, i));
			pt[1] = *((CvPoint*)cvGetSeqElem(squares, i+1));
			pt[2] = *((CvPoint*)cvGetSeqElem(squares, i+2));
			pt[3] = *((CvPoint*)cvGetSeqElem(squares, i+3));

			// draw the square as a closed polyline 
			cvPolyLine( img, &rect, &count, 1, 1, color[flag[i/4]], 1, CV_AA, 0 );
		}
	}
}