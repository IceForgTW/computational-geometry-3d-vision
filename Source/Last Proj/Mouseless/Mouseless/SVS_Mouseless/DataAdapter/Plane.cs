using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAdapter
{
    /// <summary>
    /// Plane's description
    /// </summary>
    public class Plane
    {
        private double _A, _B, _C, _D; // AX + BY + CZ + D = 0
        private double _Thres = 0;

        public Plane(double A, double B, double C, double D, double thres)
        {
            _A = A;
            _B = B;
            _C = C;
            _D = D;
            _Thres = thres;
        }

        public double DistanceFromPointToPlane(POINT3D point)
        {
            double tu = _A * point.X + _B * point.Y + _C * point.Z + _D;
            double mau = Math.Sqrt(_A * _A + _B * _B + _C * _C);
            return tu/mau;
        }

        public bool isTouchedPlane(double dist)
        {
            if (Math.Abs(dist) < _Thres)
                return true;
            return false;
        }
    }
}
