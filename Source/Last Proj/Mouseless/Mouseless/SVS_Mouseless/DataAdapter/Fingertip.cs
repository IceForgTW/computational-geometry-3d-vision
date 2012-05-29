using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAdapter
{

    public struct POINT3D
    {
        public double X, Y, Z;
    }

    public struct POINT2D
    {
        public double X, Y;
    }

    /// <summary>
    /// Store the fingertip's data
    /// </summary>
    public class Fingertip
    {
        private POINT3D _Point3D;               // 3D position of fingertip

        public POINT3D Point3D
        {
            get { return _Point3D; }
            set { _Point3D = value; }
        }

        private POINT2D _Point2D1, _Point2D2;   // 2Dposition of fingertip in 2 image

        public POINT2D Point2D2
        {
            get { return _Point2D2; }
            set { _Point2D2 = value; }
        }

        public POINT2D Point2D1
        {
            get { return _Point2D1; }
            set { _Point2D1 = value; }
        }
        private bool _Status;                // check if fingertip touch plane

        public bool Status
        {
            get { return _Status; }
            set { _Status = value; }
        }
        private double _Height;                 // distance from the fingertip to plane

        public double Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="X">X</param>
        /// <param name="Y">Y</param>
        /// <param name="Z">Z</param>
        /// <param name="Status">touch plane or not</param>
        /// <param name="Height">distance from fingertip to plane</param>
        public Fingertip(POINT3D Point3D, POINT2D Point2D1, POINT2D Point2D2, bool Status, double Height)
        {
            _Point3D = Point3D;
            _Point2D1 = Point2D1;
            _Point2D2 = Point2D2;
            _Status = Status;
            _Height = Height;
        }


        public static Fingertip[] FromArray(
            double[] point2D1x, double[] point2D1y,
            double[] point2D2x, double[] point2D2y,
            double[] point3Dx, double[] point3Dy, double[] point3Dz,
            bool[] status, double[] height,
            int N, int delta)
        {
            Fingertip[] rsl = new Fingertip[N];
            POINT3D p3D;
            POINT2D p2D1;
            POINT2D p2D2;
            
            for (int i = 0; i < N; i++)
            {
                p3D.X = point3Dx[i + delta];
                p3D.Y = point3Dy[i + delta];
                p3D.Z = point3Dz[i + delta];

                p2D1.X = point2D1x[i + delta];
                p2D1.Y = point2D1y[i + delta];

                p2D2.X = point2D2x[i + delta];
                p2D2.Y = point2D2y[i + delta];

                rsl[i] = new Fingertip(p3D, p2D1, p2D2, status[i + delta], height[i + delta]);
            }
            return rsl;
        }
    }
} 