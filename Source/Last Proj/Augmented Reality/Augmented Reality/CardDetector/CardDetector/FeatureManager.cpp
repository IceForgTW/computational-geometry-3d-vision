#include "stdafx.h"
#include "FeatureManager.h"

CFeatureManager::CFeatureManager(CvSURFParams surpParam, int minimax)
: m_CardPosThres(20)
{
	features.push_back(new CSURFFeature(surpParam, minimax) );
	//features.push_back(new CSubSquareFeature());
	//features.push_back(new COrgSURFFeature());
	currFeature = 0;
}

CFeatureManager::~CFeatureManager(void)
{
}

void CFeatureManager::BeginMatching()
{
	PrevState.clear();
	int sz = (int)PrevCards.size();
	PrevState.resize(sz, 0);
}

void CFeatureManager::EndMatching()
{
	vector<CARDInfo>::iterator it = PrevCards.begin();
	int sz = (int)PrevState.size();
	for (int i = sz-1; i >= 0 ; i--)
		if (!PrevState[i])
			PrevCards.erase(it+i);
}



int CFeatureManager::FindMostMatching( IplImage* img, int top, int left, int bottom, int right, CvPoint** V)
{
	// Kiem tra patch nay da cong trong kia chua
	// Duyet tren danh sach prev_card
	//		Lay ra mot prev_card
	//		Neu prev_card co vi tri gan giong card nay
	//			Cap nhat vi tri cua prev_card
	//			Cap nhat prev_state
	//			return objID
	int sz = (int)PrevState.size();
	for (int i = 0; i < sz; i++)
	{
		CARDInfo card = PrevCards[i];
		int flag = 1;
		if (abs(card.top-top) > m_CardPosThres)
			continue;
		if (abs(card.left-left) > m_CardPosThres)
			continue;
		if (abs(card.bottom-bottom) > m_CardPosThres)
			continue;
		if (abs(card.right-right) > m_CardPosThres)
			continue;
		PrevCards[i].left = left;
		PrevCards[i].top = top;
		PrevCards[i].bottom = bottom;
		PrevCards[i].right = right;
		// Dich trai mang V
		int t = 0, k;
		//IplImage *tmp = cvCreateImage(cvSize(img->width,img->height), 8, 3);
		//cvCvtColor(img, tmp, CV_GRAY2BGR);
		//cvNamedWindow("a");
		//cvShowImage("a", tmp);
		//cvWaitKey(0);

		for ( t = 0; t < 4; t++)
			if ( abs(V[t]->x - card.StartPoint.x) < m_CardPosThres &&
				 abs(V[t]->y - card.StartPoint.y) < m_CardPosThres)
			{
				PrevCards[i].StartPoint.x = V[t]->x;
				PrevCards[i].StartPoint.y = V[t]->y;
				break;
			}
		if (t != 0)
		{
			CvPoint Vp[4];
			for (k = 0; k < t; k++)
				Vp[k] = *V[k];
			for (k = 0; k < 4-t; k++)
				*V[k] = *V[k+t];
			for (k = 4-t; k < 4; k++)
				*V[k] = Vp[k-4+t];
		}

		PrevState[i] = 1;
		return card.objId;
	}
	
	// Neu chua co thi check
	int maxMatchingIDObj  = features[currFeature]->FindMostMatching(img);
	if (maxMatchingIDObj > -1)
	{
		// dinh huong cho card
		CvPoint* Vp[4];
		Vp[0] = new CvPoint(); Vp[0]->x = V[0]->x-left; Vp[0]->y = V[0]->y-top;
		Vp[1] = new CvPoint(); Vp[1]->x = V[1]->x-left; Vp[1]->y = V[1]->y-top;
		Vp[2] = new CvPoint(); Vp[2]->x = V[2]->x-left; Vp[2]->y = V[2]->y-top;
		Vp[3] = new CvPoint(); Vp[3]->x = V[3]->x-left; Vp[3]->y = V[3]->y-top;
		//IplImage *tmp = cvCreateImage(cvSize(img->width,img->height), 8, 3);
		//cvCvtColor(img, tmp, CV_GRAY2BGR);
		//cvLine(tmp, *Vp[0], *Vp[1], CV_RGB(255,0,0));
		//cvLine(tmp, *Vp[1], *Vp[2], CV_RGB(255,0,0));
		//cvLine(tmp, *Vp[2], *Vp[3], CV_RGB(255,0,0));
		//cvLine(tmp, *Vp[3], *Vp[0], CV_RGB(255,0,0));
		//cvNamedWindow("a");
		//cvShowImage("a", tmp);
		//cvWaitKey(0);
		MathHelper.SapHuong(img, Vp);
		V[0]->x = Vp[0]->x + left; V[0]->y = Vp[0]->y + top;
		V[1]->x = Vp[1]->x + left; V[1]->y = Vp[1]->y + top;
		V[2]->x = Vp[2]->x + left; V[2]->y = Vp[2]->y + top;
		V[3]->x = Vp[3]->x + left; V[3]->y = Vp[3]->y + top;
		delete Vp[0]; delete Vp[1]; delete Vp[2]; delete Vp[3];
		//cvShowImage("a", img);
		//cvWaitKey(0);
		CARDInfo card(maxMatchingIDObj, top, left, bottom, right, *V[0]);
		PrevCards.push_back(card);
		return maxMatchingIDObj;
	}
	return -1;
}


void CFeatureManager::ImportObjectFeature( IplImage** arrObjects, int nObject, int* arrRealObjWidth, int* arrRealObjHeight)
{
	int nFeature = 4;
	_nObject = nObject;

	features[currFeature]->ImportObjectFeature(arrObjects, nObject);
	
	int x, y, width, height;
	src_corners.resize(nObject);
	dst_corners.resize(nObject);
	_3D_corners.resize(nObject);
	for (int i = 0; i < nObject; i++)
	{
		int w = arrObjects[i]->width;
		int h = arrObjects[i]->height;
		src_corners[i].resize(nFeature);
		_3D_corners[i].resize(nFeature);
		width = arrRealObjWidth[i]; height = arrRealObjHeight[i];
		
		x = -w/2; y = -h/2;
		src_corners[i][0].x = x; src_corners[i][0].y = y;
		_3D_corners[i][0].x = width*(x/(float)w);
		_3D_corners[i][0].y = height*(y/(float)h);
		_3D_corners[i][0].z = 0;
		
		x = w/2; y = -h/2;
		src_corners[i][1].x = x; src_corners[i][1].y = y;
		_3D_corners[i][1].x = width*(x/(float)w);
		_3D_corners[i][1].y = height*(y/(float)h);
		_3D_corners[i][1].z = 0;
		
		x = w/2; y = h/2;
		src_corners[i][2].x = x; src_corners[i][2].y = y;
		_3D_corners[i][2].x = width*(x/(float)w);
		_3D_corners[i][2].y = height*(y/(float)h);
		_3D_corners[i][2].z = 0;

		x = -w/2; y = h/2;
		src_corners[i][3].x = x; src_corners[i][3].y = y;
		_3D_corners[i][3].x = width*(x/(float)w);
		_3D_corners[i][3].y = height*(y/(float)h);
		_3D_corners[i][3].z = 0;
	}
}

vector<CvMat*> CFeatureManager::EstimateProjectiveMatrix(CvMat *intrinsic, CvMat *distortion, int nDetectedObject, int* ObjectID, int *ObjectPosition)
{
	vector<CvMat*> v;
	CvMat *rotation = cvCreateMat (1, 3, CV_32FC1);
	CvMat *translation = cvCreateMat (1, 3, CV_32FC1);
	CvMat *translationTrans = cvCreateMat(3,1,CV_32FC1);

	for(int i = 0; i < nDetectedObject; i++)
	{
		CvMat *PMatrix = cvCreateMat (3, 4, CV_32FC1);

		int idObject = ObjectID[i];
		int sz = (int)src_corners[idObject].size();
		dst_corners[idObject].clear();
		for (int j = 0; j < sz; j++)
		{
			dst_corners[idObject].push_back(cvPoint(ObjectPosition[i*8], ObjectPosition[i*8+1]));
			dst_corners[idObject].push_back(cvPoint(ObjectPosition[i*8+2], ObjectPosition[i*8+3]));
			dst_corners[idObject].push_back(cvPoint(ObjectPosition[i*8+4], ObjectPosition[i*8+5]));
			dst_corners[idObject].push_back(cvPoint(ObjectPosition[i*8+6], ObjectPosition[i*8+7]));
			/*CvPoint M01 = cvPoint((ObjectPosition[i*8] + ObjectPosition[i*8+2])/2,
								  (ObjectPosition[i*8+1] + ObjectPosition[i*8+3])/2);
			CvPoint M12 = cvPoint((ObjectPosition[i*8+2] + ObjectPosition[i*8+4])/2,
								  (ObjectPosition[i*8+3] + ObjectPosition[i*8+5])/2);
			CvPoint M23 = cvPoint((ObjectPosition[i*8+4] + ObjectPosition[i*8+6])/2,
								  (ObjectPosition[i*8+5] + ObjectPosition[i*8+7])/2);
			CvPoint M30 = cvPoint((ObjectPosition[i*8+6] + ObjectPosition[i*8])/2,
								  (ObjectPosition[i*8+7] + ObjectPosition[i*8+1])/2);
			CvPoint M = cvPoint((ObjectPosition[i*8] + ObjectPosition[i*8+2] + ObjectPosition[i*8+4] + ObjectPosition[i*8+6])/4,
								(ObjectPosition[i*8+1] + ObjectPosition[i*8+3] + ObjectPosition[i*8+5] + ObjectPosition[i*8+7])/4);
			dst_corners[idObject].push_back(M01);
			dst_corners[idObject].push_back(M12);
			dst_corners[idObject].push_back(M23);
			dst_corners[idObject].push_back(M30);
			dst_corners[idObject].push_back(M);*/
		}
		// Tinh Extrinsic
		CvMat *object_points = cvCreateMat(sz, 3, CV_32FC1);
		for (int j = 0; j < sz; j++)
		{
			CV_MAT_ELEM(*object_points, float, j, 0) = _3D_corners[idObject][j].x;
			CV_MAT_ELEM(*object_points, float, j, 1) = _3D_corners[idObject][j].y;
			CV_MAT_ELEM(*object_points, float, j, 2) = _3D_corners[idObject][j].z;
		}
		CvMat *image_points = cvCreateMat(sz, 2, CV_32FC1);
		
		for (int j = 0; j < sz; j++)
		{
			CV_MAT_ELEM(*image_points, float, j, 0) = (float)dst_corners[idObject][j].x;
			CV_MAT_ELEM(*image_points, float, j, 1) = (float)dst_corners[idObject][j].y;
		}

		cvFindExtrinsicCameraParams2 (object_points, image_points, intrinsic, distortion, rotation, translation);

		CvMat *rotMat = cvCreateMat(3, 3, CV_32FC1);

		cvRodrigues2(rotation, rotMat);

		cvTranspose(translation,translationTrans);
		CvMat* tmp34 = cvCreateMat(3,4,CV_32FC1);

		for (int i = 0; i < 3; i++)
			for (int j=0; j<3; j++)
				cvmSet(tmp34,i,j,cvmGet(rotMat,i,j));
		for (int i = 0; i < 3; i++)
			cvmSet(tmp34,i,3,cvmGet(translationTrans,i,0));

		cvMatMul(intrinsic,tmp34,PMatrix);

		v.push_back(PMatrix);

		cvReleaseMat(&tmp34);
		cvReleaseMat(&rotMat);
		cvReleaseMat(&image_points);
		cvReleaseMat(&object_points);
	}
	cvReleaseMat(&rotation);
	cvReleaseMat(&translation);
	cvReleaseMat(&translationTrans);
	return v;
}

void CFeatureManager::GetProMatrix(CvMat* intrinsic, CvMat* distortion, int ObjectID, int* ObjectPosition, float* m11, float* m12, float* m13, float* m14, float* m21, float* m22, float* m23, float* m24, float* m31, float* m32, float* m33, float* m34)
{
	CvMat *rotation = cvCreateMat (1, 3, CV_32FC1);
	CvMat *translation = cvCreateMat (1, 3, CV_32FC1);
	CvMat *translationTrans = cvCreateMat(3,1,CV_32FC1);
	CvMat *PMatrix = cvCreateMat (3, 4, CV_32FC1);

	int sz = (int)src_corners[ObjectID].size();
	dst_corners[ObjectID].clear();
	for (int j = 0; j < sz; j++)
	{
		dst_corners[ObjectID].push_back(cvPoint(ObjectPosition[0], ObjectPosition[1]));
		dst_corners[ObjectID].push_back(cvPoint(ObjectPosition[2], ObjectPosition[3]));
		dst_corners[ObjectID].push_back(cvPoint(ObjectPosition[4], ObjectPosition[5]));
		dst_corners[ObjectID].push_back(cvPoint(ObjectPosition[6], ObjectPosition[7]));
	}
	// Tinh Extrinsic
	CvMat *object_points = cvCreateMat(sz, 3, CV_32FC1);
	for (int j = 0; j < sz; j++)
	{
		CV_MAT_ELEM(*object_points, float, j, 0) = _3D_corners[ObjectID][j].x;
		CV_MAT_ELEM(*object_points, float, j, 1) = _3D_corners[ObjectID][j].y;
		CV_MAT_ELEM(*object_points, float, j, 2) = _3D_corners[ObjectID][j].z;
	}
	CvMat *image_points = cvCreateMat(sz, 2, CV_32FC1);
	
	for (int j = 0; j < sz; j++)
	{
		CV_MAT_ELEM(*image_points, float, j, 0) = (float)dst_corners[ObjectID][j].x;
		CV_MAT_ELEM(*image_points, float, j, 1) = (float)dst_corners[ObjectID][j].y;
	}

	cvFindExtrinsicCameraParams2 (object_points, image_points, intrinsic, distortion, rotation, translation);

	CvMat *rotMat = cvCreateMat(3, 3, CV_32FC1);

	cvRodrigues2(rotation, rotMat);

	cvTranspose(translation,translationTrans);
	CvMat* tmp34 = cvCreateMat(3,4,CV_32FC1);

	for (int i = 0; i < 3; i++)
		for (int j=0; j<3; j++)
			cvmSet(tmp34,i,j,cvmGet(rotMat,i,j));
	for (int i = 0; i < 3; i++)
		cvmSet(tmp34,i,3,cvmGet(translationTrans,i,0));

	cvMatMul(intrinsic,tmp34,PMatrix);

	*m11 = CV_MAT_ELEM(*PMatrix, float, 0, 0);
	*m12 = CV_MAT_ELEM(*PMatrix, float, 0, 1);
	*m13 = CV_MAT_ELEM(*PMatrix, float, 0, 2);
	*m14 = CV_MAT_ELEM(*PMatrix, float, 0, 3);
	*m21 = CV_MAT_ELEM(*PMatrix, float, 1, 0);
	*m22 = CV_MAT_ELEM(*PMatrix, float, 1, 1);
	*m23 = CV_MAT_ELEM(*PMatrix, float, 1, 2);
	*m24 = CV_MAT_ELEM(*PMatrix, float, 1, 3);
	*m31 = CV_MAT_ELEM(*PMatrix, float, 2, 0);
	*m32 = CV_MAT_ELEM(*PMatrix, float, 2, 1);
	*m33 = CV_MAT_ELEM(*PMatrix, float, 2, 2);
	*m34 = CV_MAT_ELEM(*PMatrix, float, 2, 3);

	cvReleaseMat(&tmp34);
	cvReleaseMat(&rotMat);
	cvReleaseMat(&image_points);
	cvReleaseMat(&object_points);
	
	cvReleaseMat(&rotation);
	cvReleaseMat(&translation);
	cvReleaseMat(&translationTrans);
	cvReleaseMat(&PMatrix);
}
