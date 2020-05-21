using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BMesh;

public class TestBMeshOperators
{
    static float epsilon = 1e-6f;
    static bool TestAttributeLerp()
    {
        var mesh = new BMesh();
        mesh.AddVertexAttribute(new AttributeDefinition("uv", AttributeBaseType.Float, 2));
        mesh.AddVertexAttribute(new AttributeDefinition("mat", AttributeBaseType.Int, 1));

        Vertex v0 = mesh.AddVertex(new Vector3(0, 0, 0));
        Vertex v1 = mesh.AddVertex(new Vector3(0, 0, 0));
        Vertex v2 = mesh.AddVertex(new Vector3(0, 0, 0));

        v0.attributes["uv"] = new FloatAttributeValue(0.12f, 0.0f);
        v2.attributes["uv"] = new FloatAttributeValue(0.33f, 1.0f);
        v0.attributes["mat"] = new IntAttributeValue(0);
        v2.attributes["mat"] = new IntAttributeValue(1);
        BMeshOperators.AttributeLerp(mesh, v1, v0, v2, 0.4f);
        var uv1 = v1.attributes["uv"] as FloatAttributeValue;
        var mat1 = v1.attributes["mat"] as IntAttributeValue;
        Debug.Assert(uv1.data.Length == 2 && uv1.data[0] == Mathf.Lerp(0.12f, 0.33f, 0.4f) && uv1.data[1] == 0.4f, "interpolate uv");
        Debug.Assert(mat1.data.Length == 1 && mat1.data[0] == 0, "interpolate mat");

        Debug.Log("TestBMeshOperators TestAttributeLerp passed.");
        return true;
    }

    static bool TestSubdivideQuad()
    {
        var mesh = new BMesh();

        Vertex v0 = mesh.AddVertex(new Vector3(-1, 0, -1));
        Vertex v1 = mesh.AddVertex(new Vector3(-1, 0, 1));
        Vertex v2 = mesh.AddVertex(new Vector3(1, 0, 1));
        Vertex v3 = mesh.AddVertex(new Vector3(1, 0, -1));
        mesh.AddFace(v0, v1, v2, v3);

        BMeshOperators.Subdivide(mesh);

        Debug.Assert(mesh.vertices.Count == 9, "vertex count");
        Debug.Assert(mesh.edges.Count == 12, "edge count");
        Debug.Assert(mesh.loops.Count == 16, "loop count");
        Debug.Assert(mesh.faces.Count == 4, "face count");

        foreach (Face f in mesh.faces)
        {
            Debug.Assert(f.vertcount == 4, "faces are quads");
        }

        Debug.Log("TestBMeshOperators TestSubdivideQuad passed.");
        return true;
    }

    static bool TestSubdivideTris()
    {
        var mesh = new BMesh();
        mesh.AddVertexAttribute(new AttributeDefinition("uv", AttributeBaseType.Float, 2));

        Vertex v0 = mesh.AddVertex(new Vector3(-1, 0, -1));
        Vertex v1 = mesh.AddVertex(new Vector3(-1, 0, 1));
        Vertex v2 = mesh.AddVertex(new Vector3(1, 0, 1));
        Vertex v3 = mesh.AddVertex(new Vector3(1, 0, -1));
        Face f0 = mesh.AddFace(v0, v1, v2);
        Face f1 = mesh.AddFace(v2, v1, v3);

        foreach (var v in mesh.vertices)
        {
            v.attributes["uv"] = new FloatAttributeValue(v.point.x, v.point.z);
        }

        BMeshOperators.Subdivide(mesh);

        Debug.Assert(mesh.vertices.Count == 11, "vertex count");
        Debug.Assert(mesh.edges.Count == 16, "edge count");
        Debug.Assert(mesh.loops.Count == 24, "loop count");
        Debug.Assert(mesh.faces.Count == 6, "face count");

        foreach (Face f in mesh.faces)
        {
            Debug.Assert(f.vertcount == 4, "faces are quads");
        }

        foreach (var v in mesh.vertices)
        {
            var uv = v.attributes["uv"] as FloatAttributeValue;
            Debug.Assert(Mathf.Abs(uv.data[0] - v.point.x) < epsilon && Mathf.Abs(uv.data[1] - v.point.z) < epsilon, "attribute interpolation: " + uv.data[0] + " == " + v.point.x + " && " + uv.data[1] + " == " + v.point.z);
        }

        Debug.Log("TestBMeshOperators TestSubdivideTris passed.");
        return true;
    }

    public static bool Run()
    {
        if (!TestAttributeLerp()) return false;
        if (!TestSubdivideQuad()) return false;
        if (!TestSubdivideTris()) return false;
        Debug.Log("All TestBMeshOperators passed.");
        return true;
    }
}
