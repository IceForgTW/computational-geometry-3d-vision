#include "stdafx.h"
#include "SURFFeature.h"

CSURFFeature::CSURFFeature(CvSURFParams surpParam, int minimax)
{
	_params = surpParam;
	_minimax = minimax;
}
	
CSURFFeature::~CSURFFeature(void)
{
	cvReleaseMemStorage(&storage);
}

double CSURFFeature::SURFDescriptorDistance( const float* d1, const float* d2, double best, int length )
{
	double total_cost = 0;
	assert( length % 4 == 0 );
	for( int i = 0; i < length; i += 4 )
	{
		double t0 = d1[i] - d2[i];
		double t1 = d1[i+1] - d2[i+1];
		double t2 = d1[i+2] - d2[i+2];
		double t3 = d1[i+3] - d2[i+3];
		total_cost += t0*t0 + t1*t1 + t2*t2 + t3*t3;
		if( total_cost > best )
			break;
	}
	return total_cost;
}


int CSURFFeature::NaiveNearestNeighbor( const float* vec, int laplacian, const CvSeq* model_keypoints, const CvSeq* model_descriptors )
{
	int length = (int)(model_descriptors->elem_size/sizeof(float));
	int i, neighbor = -1;
	double d, dist1 = 1e6, dist2 = 1e6;
	CvSeqReader reader, kreader;
	cvStartReadSeq( model_keypoints, &kreader, 0 );
	cvStartReadSeq( model_descriptors, &reader, 0 );

	for( i = 0; i < model_descriptors->total; i++ )
	{
		const CvSURFPoint* kp = (const CvSURFPoint*)kreader.ptr;
		const float* mvec = (const float*)reader.ptr;
		CV_NEXT_SEQ_ELEM( kreader.seq->elem_size, kreader );
		CV_NEXT_SEQ_ELEM( reader.seq->elem_size, reader );
		if( laplacian != kp->laplacian )
			continue;
		d = SURFDescriptorDistance( vec, mvec, dist2, length );
		if( d < dist1 )
		{
			dist2 = dist1;
			dist1 = d;
			neighbor = i;
		}
		else if ( d < dist2 )
			dist2 = d;
	}
	if ( dist1 < 0.6*dist2 )
		return neighbor;
	return -1;
}

void CSURFFeature::FindPairs(int objectID, const CvSeq* imageKeypoints, const CvSeq* imageDescriptors, vector<int>& ptpairs )
{
	int i;
	CvSeqReader reader, kreader;
	cvStartReadSeq( objectKeypoints[objectID], &kreader );
	cvStartReadSeq( objectDescriptors[objectID], &reader );
	ptpairs.clear();

	for( i = 0; i < objectDescriptors[objectID]->total; i++ )
	{
		const CvSURFPoint* kp = (const CvSURFPoint*)kreader.ptr;
		const float* descriptor = (const float*)reader.ptr;
		CV_NEXT_SEQ_ELEM( kreader.seq->elem_size, kreader );
		CV_NEXT_SEQ_ELEM( reader.seq->elem_size, reader );
		int nearest_neighbor = NaiveNearestNeighbor( descriptor, kp->laplacian, imageKeypoints, imageDescriptors );
		if( nearest_neighbor >= 0 )
		{
			ptpairs.push_back(i);
			ptpairs.push_back(nearest_neighbor);
		}
	}
}

int CSURFFeature::CountPairs(int objectID, const CvSeq* imageKeypoints, const CvSeq* imageDescriptors)
{
	int i;
	int count = 0;
	CvSeqReader reader, kreader;
	cvStartReadSeq( objectKeypoints[objectID], &kreader );
	cvStartReadSeq( objectDescriptors[objectID], &reader );

	int nObjKeypoint = objectKeypoints[objectID]->total;
	//CvPoint2D32f* pts = new CvPoint2D32f[nObjKeypoint ];
	//CvPoint2D32f* mpts = new CvPoint2D32f[nObjKeypoint ];
	for( i = 0; i < nObjKeypoint; i++ )
	{
		const CvSURFPoint* kp = (const CvSURFPoint*)kreader.ptr;
		const float* descriptor = (const float*)reader.ptr;
		CV_NEXT_SEQ_ELEM( kreader.seq->elem_size, kreader );
		CV_NEXT_SEQ_ELEM( reader.seq->elem_size, reader );
		int nearest_neighbor = NaiveNearestNeighbor( descriptor, kp->laplacian, imageKeypoints, imageDescriptors );
		if( nearest_neighbor >= 0 )
		{
			//CvSURFPoint* tmp = (CvSURFPoint*)cvGetSeqElem(objectKeypoints[objectID], i);
			//pts[count].x = tmp->pt.x; pts[count].y = tmp->pt.y;
			//tmp = (CvSURFPoint*)cvGetSeqElem(imageKeypoints, nearest_neighbor);
			//mpts[count].x = tmp->pt.x; mpts[count].y = tmp->pt.y;
			count++;
		}
	}
	// Tinh ma tran homography
	//delete []pts;
	//delete []mpts;
	return count;
}

int CSURFFeature::ImportObjectFeature(IplImage** arrObjects, int nObject)
{
	_nObject = nObject;
	objectKeypoints = new CvSeq *[nObject];
	objectDescriptors = new CvSeq *[nObject];
	storage = cvCreateMemStorage(0);

	for (int i = 0; i < nObject; i++)
		cvExtractSURF( arrObjects[i], 0, &objectKeypoints[i], &objectDescriptors[i], storage, _params);

	return 1;
}

int CSURFFeature::FindMostMatching(IplImage* gray_img)
{
	CvSeq *imgKeypoints, *imgDescriptors;
	CvMemStorage* tmpStorage = cvCreateMemStorage(0);
	cvExtractSURF( gray_img, 0, &imgKeypoints, &imgDescriptors, tmpStorage, _params );

	// So keypoint qua it
	int maxMatches = 0;
	int maxMatchingIDObj = -1;

	if (imgDescriptors->total < 20)
		goto end;
	
	for (int i = 0; i < _nObject; i++)
	{
		///////////////////////////////////////////////////////////////////////////////////////////////
		// Kiem tra lai A <-> B hay B <-> A tuy thuoc vao so luong dac trung, truong hop mat dien thoai
		///////////////////////////////////////////////////////////////////////////////////////////////
		///////////////////////////////////////////////////////////////////////////////////////////////
		//int nMatches = CountPairs(i, imgKeypoints, imgDescriptors);
		int nMatches = CountPairs(i, imgKeypoints, imgDescriptors);
		//nMatches = min(nMatches, imgKeypoints->total);

		if (nMatches > maxMatches)
		{
			maxMatches = nMatches;
			maxMatchingIDObj = i;
		}
	}
	if (maxMatchingIDObj > -1 && maxMatches  >= _minimax )
	{
		cout << maxMatches << endl;
		//cvClearSeq(imgKeypoints); 
		//cvClearSeq(imgDescriptors);
		cvReleaseMemStorage(&tmpStorage);
		return maxMatchingIDObj;
	}
end:
	//cvClearSeq(imgKeypoints); 
	//cvClearSeq(imgDescriptors);
	cvReleaseMemStorage(&tmpStorage);
	return -1;
}