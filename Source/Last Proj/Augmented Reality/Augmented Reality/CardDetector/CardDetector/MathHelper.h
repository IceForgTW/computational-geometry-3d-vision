#pragma once
#include <highgui.h>
#include <cv.h>
#include <cxcore.h>

// pt duong thang
struct Line
{
	int a;
	int b;
	int c;
};

#define min(a,b) ((a)>(b)?(b):(a))
#define max(a,b) ((a)>(b)?(a):(b))



class CMathHelper
{
public:
	CMathHelper(void);
	~CMathHelper(void);
	Line* ptDuongThang(CvPoint *A, CvPoint* B)
	{
		Line* l = new Line;
		l->a = A->y - B->y;
		l->b = B->x - A->x;
		l->c = A->x*B->y - A->y*B->x;
		return l;
	}

	int thePointLenLine(Line* l, CvPoint* p)
	{
		return l->a*p->x + l->b*p->y + l->c;
	}

	int sqrDistance(CvPoint* A, CvPoint* B)
	{
		return (A->x - B->x)*(A->x - B->x)+(A->y - B->y)*(A->y - B->y);
	}

	int ktNamTrong(CvPoint** V, int n, CvPoint* M)
	{
		int flag = 1;
		for (int i = 0; i < n; i++)
		{
			Line* l = ptDuongThang(V[i], V[(i+1)%4]);
			int t1 = thePointLenLine(l, V[(i+2)%4]) > 0 ? 1 : -1;
			int t2 = thePointLenLine(l, M) > 0 ? 1 : -1;
			if (t1*t2 < 0)
				flag = 0;
			delete l;
		}
		return flag;
	}

	unsigned int TongTichLuy(IplImage* img, CvPoint **V)
	{
		unsigned char* data = reinterpret_cast<unsigned char*>(img->imageData);
		unsigned int s = 0;
		int top = min(min(min(V[0]->y, V[1]->y), V[2]->y),V[3]->y);
		int left = min(min(min(V[0]->x, V[1]->x), V[2]->x),V[3]->x);
		int bottom = max(max(max(V[0]->y, V[1]->y), V[2]->y),V[3]->y);
		int right = max(max(max(V[0]->x, V[1]->x), V[2]->x),V[3]->x);
		int step  = img->widthStep;

		data += top*step;
		for (int i = top; i <= bottom; i++)
		{
			for (int j = left; j <= right; j++)
				if (ktNamTrong(V, 4, &cvPoint(j, i)))
					s += data[j];
			data += step;
		}
		return s;
	}

	void SapDongHo(CvPoint** V)
	{
		Line* l = ptDuongThang(V[0], V[1]);
		if (thePointLenLine(l, V[3]) > 0)
		{
			int temp = V[1]->x; V[1]->x = V[3]->x; V[3]->x  = temp;
				temp = V[1]->y; V[1]->y = V[3]->y; V[3]->y  = temp;
		}
		delete l;
	}

	int SapHuong(IplImage* img, CvPoint** V)
	{
		// Sap nguoc chieu dong ho
		Line* l = ptDuongThang(V[0], V[1]);
		if (thePointLenLine(l, V[3]) > 0)
		{
			int temp = V[1]->x; V[1]->x = V[3]->x; V[3]->x  = temp;
				temp = V[1]->y; V[1]->y = V[3]->y; V[3]->y  = temp;
		}

		// Lay ra cac trung M01, M12, M23, M30, M
		CvPoint M01 = cvPoint( (V[0]->x+V[1]->x)/2 , (V[0]->y+V[1]->y)/2 );
		CvPoint M12 = cvPoint( (V[1]->x+V[2]->x)/2 , (V[1]->y+V[2]->y)/2 );
		CvPoint M23 = cvPoint( (V[2]->x+V[3]->x)/2 , (V[2]->y+V[3]->y)/2 );
		CvPoint M30 = cvPoint( (V[3]->x+V[0]->x)/2 , (V[3]->y+V[0]->y)/2 );
		CvPoint M = cvPoint( (V[0]->x+V[1]->x+V[2]->x+V[3]->x)/4 , (V[0]->y+V[1]->y+V[2]->y+V[3]->y)/4 );
		
		// Tinh tong tich luy grayscale cac hinh chu nhat nho
		//	cvLine(img, *P[3], *P[0], CV_RGB(255,0,0));

		double acc[4];
		CvPoint* P[4];
		
		P[0] = V[0]; P[1] = &M01; P[2] = &M; P[3] = &M30;
		acc[0] = TongTichLuy(img, P);
		
		P[0] = V[1]; P[1] = &M12; P[2] = &M; P[3] = &M01;
		acc[1] = TongTichLuy(img, P);
		
		
		P[0] = V[2]; P[1] = &M23; P[2] = &M; P[3] = &M12;
		acc[2] = TongTichLuy(img, P);

		P[0] = V[3]; P[1] = &M30; P[2] = &M; P[3] = &M23;
		acc[3] = TongTichLuy(img, P);

		// Gan lai
		int flag = 0;
		for (int t = 0; t < 4; t++)
			if (min(acc[t], acc[(t+1)%4]) > max(acc[(t+2)%4], acc[(t+3)%4]))
			{
				int k;
				if (t != 0)
				{
					flag = 1-flag;
					CvPoint Vp[4];
					for (k = 0; k < t; k++)
						Vp[k] = *V[k];
					for (k = 0; k < 4-t; k++)
						*V[k] = *V[k+t];
					for (k = 4-t; k < 4; k++)
						*V[k] = Vp[k-4+t];
				}
			}
		delete l;
		return flag;
	}

	// helper function:
	// finds a cosine of angle between vectors
	// from pt0->pt1 and from pt0->pt2 
	// the function calculates sift feature and matches them together
	double angle( CvPoint* pt1, CvPoint* pt2, CvPoint* pt0 )
	{
		double dx1 = pt1->x - pt0->x;
		double dy1 = pt1->y - pt0->y;
		double dx2 = pt2->x - pt0->x;
		double dy2 = pt2->y - pt0->y;
		return (dx1*dx2 + dy1*dy2)/sqrt((dx1*dx1 + dy1*dy1)*(dx2*dx2 + dy2*dy2) + 1e-10);
	}

};
