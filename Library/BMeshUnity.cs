/*
 * Copyright (c) 2020 -- Élie Michel <elie@exppad.com>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static BMesh;

/**
 * This file is similarily to BMeshOperators a bank of operators that can be
 * applied to BMesh objects. It follows the same rules.
 */
public class BMeshUnity
{
    ///////////////////////////////////////////////////////////////////////////
    #region [SetInMeshFilter]
       
    /**
     * Convert a BMesh into a Unity Mesh and set it in the provided MeshFilter
     * WARNING: Only works with tri or quad meshes!
     * read attributes uv, uv2 from vertices and materialId from faces.
     * 
     * NB: UVs are read from vertices, because in a Unity mesh when two face
     * corners have different UVs, they are different vertices. If you worked
     * with UVs as Loop attributes, you must first split points and migrate UVs
     * to vertex attributes.
     */
    public static void SetInMeshFilter(BMesh mesh, MeshFilter mf)
    {
        // Points
        Vector2[] uvs = null;
        Vector2[] uvs2 = null;
        Vector3[] normals = null;
        Color[] colors = null;
        Vector3[] points = new Vector3[mesh.vertices.Count];
        if (mesh.HasVertexAttribute("uv"))
        {
            uvs = new Vector2[mesh.vertices.Count];
        }
        if (mesh.HasVertexAttribute("uv2"))
        {
            uvs2 = new Vector2[mesh.vertices.Count];
        }
        if (mesh.HasVertexAttribute("normal"))
        {
            normals = new Vector3[mesh.vertices.Count];
        }
        if (mesh.HasVertexAttribute("color"))
        {
            colors = new Color[mesh.vertices.Count];
        }
        int i = 0;
        foreach (var vert in mesh.vertices)
        {
            vert.id = i;
            points[i] = vert.point;
            if (uvs != null)
            {
                var uv = vert.attributes["uv"] as FloatAttributeValue;
                uvs[i] = new Vector2(uv.data[0], uv.data[1]);
            }
            if (uvs2 != null)
            {
                var uv2 = vert.attributes["uv2"] as FloatAttributeValue;
                uvs2[i] = new Vector2(uv2.data[0], uv2.data[1]);
            }
            if (normals != null)
            {
                var normal = vert.attributes["normal"] as FloatAttributeValue;
                normals[i] = normal.AsVector3();
            }
            if (colors != null)
            {
                var color = vert.attributes["color"] as FloatAttributeValue;
                colors[i] = color.AsColor();
            }
            ++i;
        }

        // Triangles
        int maxMaterialId = 0;
        bool hasMaterialAttr = mesh.HasFaceAttribute("materialId");
        if (hasMaterialAttr)
        {
            foreach (var f in mesh.faces)
            {
                maxMaterialId = Mathf.Max(maxMaterialId, f.attributes["materialId"].asInt().data[0]);
            }
        }

        int[] tricounts = new int[maxMaterialId + 1];
        foreach (var f in mesh.faces)
        {
            Debug.Assert(f.vertcount == 3 || f.vertcount == 4, "Only meshes with triangles/quads can be converted to a unity mesh");
            int mat = hasMaterialAttr ? f.attributes["materialId"].asInt().data[0] : 0;
            tricounts[mat] += f.vertcount - 2;
        }
        int[][] triangles = new int[maxMaterialId + 1][];
        for (int mat = 0; mat < triangles.Length; ++mat)
        {
            triangles[mat] = new int[3 * tricounts[mat]];
            tricounts[mat] = 0;
        }
        // from now on tricounts[i] is the index of the next triangle to fill in the i-th triangle list
        foreach (var f in mesh.faces)
        {
            int mat = hasMaterialAttr ? f.attributes["materialId"].asInt().data[0] : 0;
            Debug.Assert(f.vertcount == 3 || f.vertcount == 4);
            {
                var l = f.loop;
                triangles[mat][3 * tricounts[mat] + 0] = l.vert.id; l = l.next;
                triangles[mat][3 * tricounts[mat] + 2] = l.vert.id; l = l.next;
                triangles[mat][3 * tricounts[mat] + 1] = l.vert.id; l = l.next;
                ++tricounts[mat];
            }
            if (f.vertcount == 4)
            {
                var l = f.loop.next.next;
                triangles[mat][3 * tricounts[mat] + 0] = l.vert.id; l = l.next;
                triangles[mat][3 * tricounts[mat] + 2] = l.vert.id; l = l.next;
                triangles[mat][3 * tricounts[mat] + 1] = l.vert.id; l = l.next;
                ++tricounts[mat];
            }
        }

        // Apply mesh
        Mesh unityMesh = new Mesh();
        mf.mesh = unityMesh;
        unityMesh.vertices = points;
        if (uvs != null) unityMesh.uv = uvs;
        if (uvs2 != null) unityMesh.uv2 = uvs2;
        if (normals != null) unityMesh.normals = normals;
        if (colors != null) unityMesh.colors = colors;
        unityMesh.subMeshCount = triangles.Length;

        // Fix an issue when renderer has more materials than there are submeshes
        var renderer = mf.GetComponent<MeshRenderer>();
        if (renderer)
        {
            unityMesh.subMeshCount = Mathf.Max(unityMesh.subMeshCount, renderer.materials.Length);
        }

        for (int mat = 0; mat  < triangles.Length; ++mat)
        {
            unityMesh.SetTriangles(triangles[mat], mat);
        }

        if (normals == null)
        {
            unityMesh.RecalculateNormals();
        }
    }

    #endregion

    ///////////////////////////////////////////////////////////////////////////
    #region [Merge]

    /**
     * Merge a Unity Mesh into a BMesh. Can be used with an empty BMesh to
     * create a BMesh from a Unity Mesh
     * TODO: Add support for uvs etc.
     */
    public static void Merge(BMesh mesh, Mesh unityMesh, bool flipFaces = false)
    {
        Vector3[] unityVertices = unityMesh.vertices;
        Vector2[] unityUvs = unityMesh.uv;
        Vector2[] unityUvs2 = unityMesh.uv2;
        Vector3[] unityNormals = unityMesh.normals;
        Color[] unityColors = unityMesh.colors;
        bool hasUvs = unityUvs != null && unityUvs.Length > 0;
        bool hasUvs2 = unityUvs2 != null && unityUvs2.Length > 0;
        bool hasNormals = unityNormals != null && unityNormals.Length > 0;
        bool hasColors = unityColors != null && unityColors.Length > 0;
        int[] unityTriangles = unityMesh.triangles;
        var verts = new Vertex[unityVertices.Length];
        if (hasUvs)
        {
            mesh.AddVertexAttribute(new AttributeDefinition("uv", AttributeBaseType.Float, 2));
        }
        if (hasUvs2)
        {
            mesh.AddVertexAttribute(new AttributeDefinition("uv2", AttributeBaseType.Float, 2));
        }
        if (hasNormals)
        {
            mesh.AddVertexAttribute(new AttributeDefinition("normal", AttributeBaseType.Float, 3));
        }
        if (hasColors)
        {
            mesh.AddVertexAttribute(new AttributeDefinition("color", AttributeBaseType.Float, 4));
        }
        for (int i = 0; i < unityVertices.Length; ++i)
        {
            Vector3 p = unityVertices[i];
            verts[i] = mesh.AddVertex(p);
            if (hasUvs)
            {
                verts[i].attributes["uv"].asFloat().FromVector2(unityUvs[i]);
            }
            if (hasUvs2)
            {
                verts[i].attributes["uv2"].asFloat().FromVector2(unityUvs2[i]);
            }
            if (hasNormals)
            {
                verts[i].attributes["normal"].asFloat().FromVector3(unityNormals[i]);
            }
            if (hasColors)
            {
                verts[i].attributes["color"].asFloat().FromColor(unityColors[i]);
            }
        }
        for (int i = 0; i < unityTriangles.Length / 3; ++i)
        {
            mesh.AddFace(
                verts[unityTriangles[3 * i + (flipFaces ? 1 : 0)]],
                verts[unityTriangles[3 * i + (flipFaces ? 0 : 1)]],
                verts[unityTriangles[3 * i + 2]]
            );
        }
    }

    #endregion

    ///////////////////////////////////////////////////////////////////////////
    #region [DrawGizmos]

    /**
     * Draw details about the BMesh structure un the viewport.
     * To be used inside of OnDrawGizmozs() in a MonoBehavior script.
     * You'll most likely need to add beforehand:
     *     Gizmos.matrix = transform.localToWorldMatrix
     */
    public static void DrawGizmos(BMesh mesh)
    {
        Gizmos.color = Color.yellow;
        foreach (var e in mesh.edges)
        {
            Gizmos.DrawLine(e.vert1.point, e.vert2.point);
        }
        Gizmos.color = Color.red;
        foreach (var l in mesh.loops)
        {
            Vertex vert = l.vert;
            Vertex other = l.edge.OtherVertex(vert);
            Gizmos.DrawRay(vert.point, (other.point - vert.point) * 0.1f);

            Loop nl = l.next;
            Vertex nother = nl.edge.ContainsVertex(vert) ? nl.edge.OtherVertex(vert) : nl.edge.OtherVertex(other);
            Vector3 no = vert.point + (other.point - vert.point) * 0.1f;
            Gizmos.DrawRay(no, (nother.point - no) * 0.1f);
        }
        Gizmos.color = Color.green;
        int i = 0;
        foreach (var f in mesh.faces)
        {
            Vector3 c = f.Center();
            Gizmos.DrawLine(c, f.loop.vert.point);
            Gizmos.DrawRay(c, (f.loop.next.vert.point - c) * 0.2f);
#if UNITY_EDITOR
            //Handles.Label(c, "f" + i);
            ++i;
#endif // UNITY_EDITOR
        }

        i = 0;
        foreach (Vertex v in mesh.vertices)
        {
#if UNITY_EDITOR
            //var uv = v.attributes["uv"] as BMesh.FloatAttributeValue;
            //Handles.Label(v.point, "" + i);
            ++i;
#endif // UNITY_EDITOR
        }
    }

    #endregion
}
