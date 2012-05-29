#include "stdafx.h"
#include "Model.h"


CModel::~CModel(void)
{
}

CModel::CModel(char * fName)
{
	ifstream f;
	int i, n;
	f.open(fName);
	f >> n;
	// Khoi tao mang Vertex 3D homogenous coordinate
	Vertexes.resize(n);
	for (i = 0; i < n; i++)
		Vertexes[i] = cvCreateMat(4,1,CV_32FC1);

	for (i = 0; i < n; i++)
	{
		f >> CV_MAT_ELEM(*Vertexes[i], float, 0, 0);
		f >> CV_MAT_ELEM(*Vertexes[i], float, 1, 0);
		f >> CV_MAT_ELEM(*Vertexes[i], float, 2, 0);
		CV_MAT_ELEM(*Vertexes[i], float, 3, 0) = 1;
	}

	f >> n;
	Edges.resize(n);
	for (i = 0; i < n; i++)
		f >> Edges[i].idxFrm >> Edges[i].idxTo;

	f.close();
}

void CModel::Draw(IplImage* img, CvMat* CamMat, CvScalar color, int thickness)
{
	int nEdges = Edges.size();
	int nVertexes = Vertexes.size();

	// Tao mang anh chieu
	vector<CvMat*> ProjectedVertex;

	ProjectedVertex.resize(Vertexes.size());
	for (int i = 0; i < nVertexes; i++)
		ProjectedVertex[i] = cvCreateMat(3,1,CV_32FC1);

	// Thuc hien phep chieu cac dinh
	for (int i = 0; i < nVertexes; i++)
	{
		cvMatMul(CamMat,Vertexes[i],ProjectedVertex[i]);
		CV_MAT_ELEM(*ProjectedVertex[i],float,0,0) = 
			CV_MAT_ELEM(*ProjectedVertex[i],float,0,0)/CV_MAT_ELEM(*ProjectedVertex[i],float,2,0);
		CV_MAT_ELEM(*ProjectedVertex[i],float,1,0) = 
			CV_MAT_ELEM(*ProjectedVertex[i],float,1,0)/CV_MAT_ELEM(*ProjectedVertex[i],float,2,0);
	}

	for (int i = 0; i < nEdges; i++)
	{
		cvLine(img, 
			cvPoint(CV_MAT_ELEM(*ProjectedVertex[Edges[i].idxFrm],float,0,0), CV_MAT_ELEM(*ProjectedVertex[Edges[i].idxFrm],float,1,0)),
			cvPoint(CV_MAT_ELEM(*ProjectedVertex[Edges[i].idxTo], float,0,0), CV_MAT_ELEM(*ProjectedVertex[Edges[i].idxTo], float,1,0)),
			color, thickness);
	}
	for (int i = 0; i < nVertexes; i++)
		cvReleaseMat(&ProjectedVertex[i]);
	ProjectedVertex.clear();
}
