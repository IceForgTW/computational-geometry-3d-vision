#include "stdafx.h"
#include "SubSquareFeature.h"
#include "SquareDetection.h"

CSubSquareFeature::CSubSquareFeature(void)
{
}

CSubSquareFeature::~CSubSquareFeature(void)
{
}

int CSubSquareFeature::ImportObjectFeature(IplImage** arrObjects, int nObject)
{
	_nObject = nObject;
	objectFeature.resize(nObject);
	CSquareDetection squareDetection(50, 0.5f, 0.3f, 0.001f, 0.36f);
	for (int i = 0; i < nObject; i++)
		objectFeature[i] = squareDetection.CountSubSquares(arrObjects[i]);
	return 1;
}

int CSubSquareFeature::FindMostMatching(IplImage* img)
{
	CSquareDetection squareDetection(50, 0.5f, 0.3f, 0.001f, 0.36f);
	for (int i = 0; i < _nObject; i++)
		if (objectFeature[i] == squareDetection.CountSubSquares(img))
			return i;
	return -1;
}
