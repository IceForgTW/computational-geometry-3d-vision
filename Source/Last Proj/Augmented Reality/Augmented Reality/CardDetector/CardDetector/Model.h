#pragma once
#include <highgui.h>
#include <cxcore.h>
#include <cv.h>
#include <fstream>
#include <vector>

using namespace std;


struct Edge
{
	int idxFrm;
	int idxTo;
};

class CModel
{
	vector<CvMat*> Vertexes;
	vector<Edge> Edges;

public:
	CModel(char * fName);
	CModel::~CModel(void);
	void Draw(IplImage* img, CvMat* CamMat, CvScalar color, int thickness);
};
