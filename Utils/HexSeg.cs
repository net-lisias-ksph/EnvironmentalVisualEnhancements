﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utils
{
    class Triangle
    {
        Vector3[] Points;
        Vector3[] SubPoints;
        Triangle[] Subs;
        /* 1       2
         * *---*---*
         *  \  1  /
         *   *0 2*
         *    \ /
         *     *
         *     0
         */     
        public Triangle()
        {
            Points = new Vector3[3];
        }

        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Points = new Vector3[3];
            Points[0] = p1;
            Points[1] = p2;
            Points[2] = p3;
        }

        public void Subdivide(int i=1)
        {
            SubPoints = new Vector3[3];
            SubPoints[0] = Points[0] + ((Points[1] - Points[0])/2f);
            SubPoints[1] = Points[1] + ((Points[2] - Points[1])/2f);
            SubPoints[2] = Points[2] + ((Points[0] - Points[2])/2f);

            Subs = new Triangle[4];
            Subs[0] = new Triangle(Points[0], SubPoints[0], SubPoints[2]);
            Subs[1] = new Triangle(Points[1], SubPoints[1], SubPoints[0]);
            Subs[2] = new Triangle(Points[2], SubPoints[2], SubPoints[1]);
            Subs[3] = new Triangle(SubPoints[0], SubPoints[1], SubPoints[2]);
            if(i > 1)
            {
                foreach(Triangle sub in Subs)
                {
                    sub.Subdivide(i - 1);
                }
            }
        }

        internal void AppendPoints(List<Vector3> pointList)
        {
            foreach(Vector3 point in Points)
            {
                if(!pointList.Contains(point))
                {
                    pointList.Add(point);
                }
            }
            if (Subs != null)
            {
                foreach (Triangle sub in Subs)
                {
                    sub.AppendPoints(pointList);
                }
            }
        }

        internal void AppendTrianglesAndPoints(List<int> indiceList, List<Vector3> pointList)
        {
            if (Subs != null)
            {
                foreach (Triangle sub in Subs)
                {
                    sub.AppendTrianglesAndPoints(indiceList,pointList);
                }
            }
            else  //we are at the lowest subdivision level
            {
                foreach (Vector3 point in Points)
                {
                    if (!pointList.Contains(point))
                    {
                        pointList.Add(point);
                    }
                    indiceList.Add(pointList.IndexOf(point));
                }
            }
        }
    }

    public class HexSeg
    {

        Triangle[] Triangles;
        float Radius;

        public HexSeg(float radius, int sub)
        {
            Radius = radius;
            Triangles = new Triangle[6];
            float halfRad = Radius / 2f;
            float opp = Mathf.Sqrt(.75f) * Radius;

            Vector3 p0 = new Vector3(-halfRad, 0, opp);
            Vector3 p1 = new Vector3(halfRad, 0, opp);
            Vector3 p2 = new Vector3(Radius, 0, 0);
            Vector3 p3 = new Vector3(halfRad, 0, -opp);
            Vector3 p4 = new Vector3(-halfRad, 0, -opp);
            Vector3 p5 = new Vector3(-Radius, 0, 0);
            Vector3 c = new Vector3(0, 0, 0);

            Triangles[0] = new Triangle(c, p0, p1);
            Triangles[1] = new Triangle(c, p1, p2);
            Triangles[2] = new Triangle(c, p2, p3);
            Triangles[3] = new Triangle(c, p3, p4);
            Triangles[4] = new Triangle(c, p4, p5);
            Triangles[5] = new Triangle(c, p5, p0);
            foreach (Triangle triangle in Triangles)
            {
                triangle.Subdivide(sub);
            }
        }

        public List<Vector3> GetPoints()
        {
            List<Vector3> pointList = new List<Vector3>();
            foreach(Triangle triangle in Triangles)
            {
                triangle.AppendPoints(pointList);
            }
            return pointList;
        }

        public Mesh BuildMesh()
        {
            List<Vector3> pointList = new List<Vector3>();
            List<int> indiceList = new List<int>();

            foreach (Triangle triangle in Triangles)
            {
                triangle.AppendTrianglesAndPoints(indiceList, pointList);
            }

            Mesh hexSegMesh = new Mesh();
            hexSegMesh.vertices = pointList.ToArray();
            hexSegMesh.triangles = indiceList.ToArray();

            return hexSegMesh;
        }

        public Mesh BuildPointsMesh()
        {
            List<Vector3> pointList = GetPoints();

            // Sort points by distance from the center of the mesh
            // Since we snap the mesh to always be around the camera when it moves
            // and we are using a geometry shader which process vertices in the order they are defined
            // This effectively gives us free back to front sorting for the transparencies
            pointList.Sort((vec1, vec2) => vec1.magnitude.CompareTo(vec2.magnitude));
            pointList.Reverse();

            int[] indices = new int[pointList.Count];

            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }

            Mesh hexSegMesh = new Mesh();
            hexSegMesh.SetVertices(pointList);
            hexSegMesh.SetIndices(indices, MeshTopology.Points, 0);

            return hexSegMesh;
        }
    }
}
