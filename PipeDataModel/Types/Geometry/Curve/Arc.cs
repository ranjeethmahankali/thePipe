﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipeDataModel.Utils;

namespace PipeDataModel.Types.Geometry.Curve
{
    [Serializable]
    public class Arc : Curve
    {
        #region-fields
        private Plane _plane;
        private double _radius;
        private double _startAngle, _endAngle;
        #endregion

        #region-properties
        public Plane Plane
        {
            get { return _plane; }
            set { _plane = value; }
        }
        public double Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }
        public double StartAngle
        {
            get { return _startAngle; }
            set { _startAngle = value; }
        }
        public double EndAngle
        {
            get { return _endAngle; }
            set { _endAngle = value; }
        }

        public override Vec StartPoint
        {
            get { return _plane.GlobalCoordinatesOf(StartAngle, Radius); }
        }

        public override Vec EndPoint
        {
            get { return _plane.GlobalCoordinatesOf(EndAngle, Radius); }
        }
        #endregion

        #region-constructors
        public Arc(Plane pl, double rad, double startAng, double endAng)
        {
            if(!(0 <= startAng && startAng <= 2*Math.PI && 0 <= endAng && endAng <= 2 * Math.PI
                && startAng != endAng && rad > 0))
            {
                throw new ArgumentException("Invalid parameters provided for the Arc constructor");
            }
            _plane = pl;
            _radius = rad;
            _startAngle = startAng;
            _endAngle = endAng;
            EnsureAlignment();
        }
        public Arc(Vec startPt, Vec endPt, Vec ptOnArc)
        {
            Vec midPt1 = Vec.Multiply(Vec.Sum(endPt, ptOnArc), 0.5);
            Vec midPt2 = Vec.Multiply(Vec.Sum(startPt, ptOnArc), 0.5);

            Vec seg1 = Vec.Difference(ptOnArc, startPt);
            Vec seg2 = Vec.Difference(endPt, ptOnArc);

            Vec planeZ = Vec.Cross(seg1, seg2);

            Vec rad1 = Vec.Cross(planeZ, seg1);
            Vec rad2 = Vec.Cross(planeZ, seg2);

            Vec center = IntersectLines(midPt1, rad1, midPt2, rad2);
            Vec planeX = Vec.Difference(startPt, center);
            Vec planeY = Vec.Cross(planeZ, planeX);

            _plane = new Plane(center, planeX, planeY, planeZ);
            _radius = Vec.Difference(center, startPt).Length;
            _startAngle = 0;
            _endAngle = 2 * Vec.AngleBetween(rad1, rad2);
        }
        #endregion

        #region-methods
        private void EnsureAlignment()
        {
            if(StartAngle == 0) { return; }
            Plane = new Plane(Plane.Origin, Plane.X.RotateAbout(Plane.Z, StartAngle), 
                Plane.Y.RotateAbout(Plane.Z, StartAngle));
            EndAngle = EndAngle - StartAngle;
            StartAngle = 0;
        }
        public void TransformToPlane(Plane newPlane)
        {
            double eps = 1e-7;
            if(!PipeDataUtil.Equals(Vec.Dot(newPlane.Z, Plane.Z), 1, eps))
            {
                throw new InvalidOperationException("Cannot transform arc to a new plane");
            }
            if(Vec.Difference(newPlane.Origin, Plane.Origin).Length > eps)
            {
                throw new InvalidOperationException("The origins of the planes do not match");
            }

            double angle = Vec.AngleBetween(Plane.X, newPlane.X);
            if(angle < eps) { return; }
            if(Vec.Dot(Vec.Cross(Plane.X, newPlane.X), Plane.Z) < 0) { angle *= -1; }

            Plane = new Plane(Plane.Origin, Plane.X.RotateAbout(Plane.Z, angle),
                Plane.Y.RotateAbout(Plane.Z, angle));
            StartAngle -= angle;
            EndAngle -= angle;
        }

        public override bool Equals(IPipeMemberType other)
        {
            if(GetType() != other.GetType()) { return false; }
            Arc otherArc = (Arc)other;
            return _endAngle == otherArc.EndAngle && _startAngle == otherArc.StartAngle
                && _radius == otherArc.Radius && _plane.Equals(otherArc.Plane);
        }
        // returns the intersection of line passing through pt1 parallel to v1 with the line through pt2 passing parallel to v2
        // returns null if the lines don't intersect
        public static Vec IntersectLines(Vec pt1, Vec v1, Vec pt2, Vec v2)
        {
            Vec dirCross = Vec.Cross(v1, v2);
            if(dirCross.Length == 0) { return null; }

            Vec numerator = Vec.Cross(Vec.Difference(pt2, pt1), v2);
            double param1 = Vec.Dot(numerator, dirCross) * (numerator.Length / dirCross.Length);

            return Vec.Sum(pt1, Vec.Multiply(v1, param1));
        }
        #endregion
    }
}
