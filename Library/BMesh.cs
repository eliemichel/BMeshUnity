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

using System.Collections.Generic;
using UnityEngine;

/**
 * Non-manifold boundary representation of a 3D mesh with arbitrary attributes.
 * This structure intends to make procedural mesh creation and arbitrary edits
 * as easy as possible while remaining efficient enough. See other comments
 * along the file to see how to use it.
 * 
 * This file only contains the data structure and basic operations such as
 * adding/removing elements. For more advanced operations, see BMeshOperators.
 * For operations related to Unity, like converting to UnityEngine.Mesh, see
 * the BMeshUnity class.
 * 
 * The basic structure is described in the paper:
 * 
 *     Gueorguieva, Stefka and Marcheix, Davi. 1994. "Non-manifold boundary
 *     representation for solid modeling."
 *     
 * We use the same terminology as Blender's dev documentation:
 *     https://wiki.blender.org/wiki/Source/Modeling/BMesh/Design
 *     
 * Arbitrary attributes can be attached topological entity, namely vertices,
 * edges, loops and faces. If you are used to Houdini's terminology, note that
 * what is called "vertex" here corresponds to Houdini's points, while what
 * Houdini calls "vertex" is close to BMesh's "loops".
 *     
 * NB: The only dependency to Unity is the Vector3 structure, it can easily be
 * made independent.
 * 
 * NB: This class is not totally protected from misuse. We prefered fostering
 * ease of use over safety, so take care when you start feeling that you are
 * not fully understanding what you are doing, you'll likely mess with the
 * structure. For instance, do not add edges directly to the mesh.edges
 * list but use AddEdge, etc.
 * 
 * NB: This file is long, use the #region to fold it conveniently.
 */
namespace BMeshLib
{
    public class BMesh
    {
        // Topological entities
        public List<Vertex> vertices;
        public List<Edge> edges;
        public List<Loop> loops;
        public List<Face> faces;

        // Attribute definitions. The content of attributes is stored in the
        // topological objects (Vertex, Edge, etc.) in the 'attribute' field.
        // These lists are here to ensure consistency.
        public List<AttributeDefinition> vertexAttributes;
        public List<AttributeDefinition> edgeAttributes;
        public List<AttributeDefinition> loopAttributes;
        public List<AttributeDefinition> faceAttributes;

        public BMesh()
        {
            vertices = new List<Vertex>();
            loops = new List<Loop>();
            edges = new List<Edge>();
            faces = new List<Face>();

            vertexAttributes = new List<AttributeDefinition>();
            edgeAttributes = new List<AttributeDefinition>();
            loopAttributes = new List<AttributeDefinition>();
            faceAttributes = new List<AttributeDefinition>();
        }

        ///////////////////////////////////////////////////////////////////////////
        #region [Topology Methods]

        /// <summary>
        /// Adds the specified <see cref="Vertex"/> to the <see cref="BMesh"/>.
        /// </summary>
        /// <param name="vert">The <see cref="Vertex"/> to add to the <see cref="BMesh"/>.</param>
        /// <returns>The <see cref="Vertex"/> that was added.</returns>
        public Vertex AddVertex(Vertex vert)
        {
            EnsureVertexAttributes(vert);
            vertices.Add(vert);
            return vert;
        }
        
        /// <summary>
        /// Adds a new <see cref="Vertex"/> to the <see cref="BMesh"/> with the specified position.
        /// </summary>
        /// <param name="point">The position of the <see cref="Vertex"/> to
        /// create and add to the <see cref="BMeshLib"/>.</param>
        /// <returns>The <see cref="Vertex"/> that was created and added.</returns>
        public Vertex AddVertex(Vector3 point)
        {
            return AddVertex(new Vertex(point));
        }
        
        /// <summary>
        /// Adds a new <see cref="Vertex"/> to the <see cref="BMesh"/> with the specified position.
        /// </summary>
        /// <param name="x">The x position of the <see cref="Vertex"/> to create.</param>
        /// <param name="y">The y position of the <see cref="Vertex"/> to create.</param>
        /// <param name="z">The z position of the <see cref="Vertex"/> to create.</param>
        /// <returns>The <see cref="Vertex"/> that was created and added.</returns>
        public Vertex AddVertex(float x, float y, float z)
        {
            return AddVertex(new Vector3(x, y, z));
        }
        
        /// <summary>
        /// Adds a new <see cref="Edge"/> comprised of the specified vertices.
        /// If there is already an <see cref="Edge"/> between them, return it
        /// without creating a new one.
        /// </summary>
        /// <remarks>
        /// If the vertices are not part of the <see cref="BMeshLib"/>,
        /// the behavior is undefined.
        /// </remarks>
        /// <param name="vert1">The <see cref="Vertex"/> used for one end of the <see cref="Edge"/>.</param>
        /// <param name="vert2">The <see cref="Vertex"/> used for one end of the <see cref="Edge"/>.</param>
        /// <returns>
        /// An <see cref="Edge"/> in the <see cref="BMesh"/> that is comprised of <paramref name="vert1"/>
        /// and <paramref name="vert2"/>.
        /// </returns>
        public Edge AddEdge(Vertex vert1, Vertex vert2)
        {
            Debug.Assert(vert1 != vert2);

            var edge = FindEdge(vert1, vert2);
            if (edge != null) return edge;

            edge = new Edge
            {
                vert1 = vert1,
                vert2 = vert2
            };
            EnsureEdgeAttributes(edge);
            edges.Add(edge);

            // Insert in vert1's edge list
            if (vert1.edge == null)
            {
                vert1.edge = edge;
                edge.next1 = edge.prev1 = edge;
            }
            else
            {
                edge.next1 = vert1.edge.Next(vert1);
                edge.prev1 = vert1.edge;
                edge.next1.SetPrev(vert1, edge);
                edge.prev1.SetNext(vert1, edge);
            }

            // Same for vert2 -- TODO avoid code duplication
            if (vert2.edge == null)
            {
                vert2.edge = edge;
                edge.next2 = edge.prev2 = edge;
            }
            else
            {
                edge.next2 = vert2.edge.Next(vert2);
                edge.prev2 = vert2.edge;
                edge.next2.SetPrev(vert2, edge);
                edge.prev2.SetNext(vert2, edge);
            }

            return edge;
        }

        /// <summary>
        /// Adds a new <see cref="Edge"/> comprised of the vertices at the specified indices.
        /// If there is already an <see cref="Edge"/> between them, return it
        /// without creating a new one.
        /// </summary>
        /// <remarks>
        /// If the vertices are not part of the <see cref="BMeshLib"/>,
        /// the behavior is undefined.
        /// </remarks>
        /// <param name="v1">The index of the <see cref="Vertex"/> to use for one end of the <see cref="Edge"/>.</param>
        /// <param name="v2">The index of the <see cref="Vertex"/> to use for one end of the <see cref="Edge"/>.</param>
        /// <returns>
        /// An <see cref="Edge"/> in the <see cref="BMesh"/> that is comprised of the
        /// <see cref="Vertex"/> at index <paramref name="v1"/> and the <see cref="Vertex"/>
        /// at index <paramref name="v2"/>.
        /// </returns>
        public Edge AddEdge(int v1, int v2)
        {
            return AddEdge(vertices[v1], vertices[v2]);
        }
        
        /// <summary>
        /// Adds a new <see cref="Face"/> with loops that is comprised of the specified <see cref="Vertex"/>s.
        /// </summary>
        /// <param name="fVerts">The vertices that are used to create the <see cref="Face"/>.</param>
        /// <returns>The <see cref="Face"/> created using the specified vertices.</returns>
        /// <remarks>
        /// The vertices must be part of the mesh, otherwise the behavior is undefined.
        /// </remarks>
        public Face AddFace(Vertex[] fVerts)
        {
            if (fVerts.Length == 0) return null;
            foreach (var v in fVerts) Debug.Assert(v != null);

            var fEdges = new Edge[fVerts.Length];

            int i, i_prev = fVerts.Length - 1;
            for (i = 0; i < fVerts.Length; ++i)
            {
                fEdges[i_prev] = AddEdge(fVerts[i_prev], fVerts[i]);
                i_prev = i;
            }

            var f = new Face();
            EnsureFaceAttributes(f);
            faces.Add(f);

            for (i = 0; i < fVerts.Length; ++i)
            {
                Loop loop = new Loop(fVerts[i], fEdges[i], f);
                EnsureLoopAttributes(loop);
                loops.Add(loop);
            }

            f.vertcount = fVerts.Length;
            return f;
        }

        /// <summary>
        /// Adds a new <see cref="Face"/> with loops that is comprised of the specified <see cref="Vertex"/>s.
        /// </summary>
        /// <param name="v0">One of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <param name="v1">One of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <returns>The <see cref="Face"/> created using the specified vertices.</returns>
        /// <remarks>
        /// The vertices must be part of the mesh, otherwise the behavior is undefined.
        /// </remarks>
        public Face AddFace(Vertex v0, Vertex v1)
        {
            return AddFace(new Vertex[] { v0, v1 });
        }

        /// <summary>
        /// Adds a new <see cref="Face"/> with loops that is comprised of the specified <see cref="Vertex"/>s.
        /// </summary>
        /// <param name="v0">One of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <param name="v1">One of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <param name="v2">One of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <returns>The <see cref="Face"/> created using the specified vertices.</returns>
        /// <remarks>
        /// The vertices must be part of the mesh, otherwise the behavior is undefined.
        /// </remarks>
        public Face AddFace(Vertex v0, Vertex v1, Vertex v2)
        {
            return AddFace(new Vertex[] { v0, v1, v2 });
        }

        /// <summary>
        /// Adds a new <see cref="Face"/> with loops that is comprised of the specified <see cref="Vertex"/>s.
        /// </summary>
        /// <param name="v0">One of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <param name="v1">One of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <param name="v2">One of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <param name="v3">One of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <returns>The <see cref="Face"/> created using the specified vertices.</returns>
        /// <remarks>
        /// The vertices must be part of the mesh, otherwise the behavior is undefined.
        /// </remarks>
        public Face AddFace(Vertex v0, Vertex v1, Vertex v2, Vertex v3)
        {
            return AddFace(new Vertex[] { v0, v1, v2, v3 });
        }

        /// <summary>
        /// Adds a new <see cref="Face"/> with loops that is comprised of the specified <see cref="Vertex"/>s.
        /// </summary>
        /// <param name="i0">The index of one of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <param name="i1">The index of one of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <returns>The <see cref="Face"/> created using the specified vertices.</returns>
        /// <remarks>
        /// The vertices must be part of the mesh, otherwise the behavior is undefined.
        /// </remarks>
        public Face AddFace(int i0, int i1)
        {
            return AddFace(new Vertex[] { vertices[i0], vertices[i1] });
        }

        /// <summary>
        /// Adds a new <see cref="Face"/> with loops that is comprised of the specified <see cref="Vertex"/>s.
        /// </summary>
        /// <param name="i0">The index of one of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <param name="i1">The index of one of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <param name="i2">The index of one of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <returns>The <see cref="Face"/> created using the specified vertices.</returns>
        /// <remarks>
        /// The vertices must be part of the mesh, otherwise the behavior is undefined.
        /// </remarks>
        public Face AddFace(int i0, int i1, int i2)
        {
            return AddFace(new Vertex[] { vertices[i0], vertices[i1], vertices[i2] });
        }

        /// <summary>
        /// Adds a new <see cref="Face"/> with loops that is comprised of the specified <see cref="Vertex"/>s.
        /// </summary>
        /// <param name="i0">The index of one of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <param name="i1">The index of one of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <param name="i2">The index of one of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <param name="i3">The index of one of the <see cref="Vertex"/>s to use to create the <see cref="Face"/>.</param>
        /// <returns>The <see cref="Face"/> created using the specified vertices.</returns>
        /// <remarks>
        /// The vertices must be part of the mesh, otherwise the behavior is undefined.
        /// </remarks>
        public Face AddFace(int i0, int i1, int i2, int i3)
        {
            return AddFace(new Vertex[] { vertices[i0], vertices[i1], vertices[i2], vertices[i3] });
        }
        
        /// <summary>
        /// Searches for an <see cref="Edge"/> that consists of the specified
        /// <see cref="Vertex"/>s and returns the first occurence.
        /// </summary>
        /// <param name="vert1">
        /// One of the <see cref="Vertex"/>s that consist the <see cref="Edge"/> to search for.
        /// </param>
        /// <param name="vert2">
        /// One of the <see cref="Vertex"/>s that consist the <see cref="Edge"/> to search for.
        /// </param>
        /// <returns>
        /// The first <see cref="Edge"/> that consists of <paramref name="vert1"/>
        /// and <paramref name="vert2"/>, if found; otherwise <c>null</c>.
        /// </returns>
        public Edge FindEdge(Vertex vert1, Vertex vert2)
        {
            Debug.Assert(vert1 != vert2);
            if (vert1.edge == null || vert2.edge == null) return null;

            Edge e1 = vert1.edge;
            Edge e2 = vert2.edge;
            do
            {
                if (e1.ContainsVertex(vert2)) return e1;
                if (e2.ContainsVertex(vert1)) return e2;
                e1 = e1.Next(vert1);
                e2 = e2.Next(vert2);
            } while (e1 != vert1.edge && e2 != vert2.edge);
            return null;
        }
        
        /// <summary>
        /// Removes the specified <see cref="Vertex"/> from the <see cref="BMesh"/>,
        /// also removes all <see cref="Edge"/>s, <see cref="Loop"/>s
        /// and <see cref="Face"/>s that use it.
        /// </summary>
        /// <param name="v">The <see cref="Vertex"/> to remove from the <see cref="BMesh"/>.</param>
        /// <remarks>
        /// If the specified <see cref="Vertex"/> is not part of the <see cref="BMesh"/>, the behavior is undefined.
        /// </remarks>
        public void RemoveVertex(Vertex v)
        {
            while (v.edge != null)
            {
                RemoveEdge(v.edge);
            }

            vertices.Remove(v);
        }

        /// <summary>
        /// Removes the specified <see cref="Edge"/> from the <see cref="BMesh"/>,
        /// also removes all <see cref="Loop"/>s and <see cref="Face"/>s that use it.
        /// </summary>
        /// <param name="e">The <see cref="Edge"/> to remove from the <see cref="BMesh"/>.</param>
        /// <remarks>
        /// If the specified <see cref="Edge"/> is not part of the <see cref="BMesh"/>, the behavior is undefined.
        /// </remarks>
        public void RemoveEdge(Edge e)
        {
            while (e.loop != null)
            {
                RemoveLoop(e.loop);
            }

            // Remove reference in vertices
            if (e == e.vert1.edge) e.vert1.edge = e.next1 != e ? e.next1 : null;
            if (e == e.vert2.edge) e.vert2.edge = e.next2 != e ? e.next2 : null;

            // Remove from linked lists
            e.prev1.SetNext(e.vert1, e.next1);
            e.next1.SetPrev(e.vert1, e.prev1);

            e.prev2.SetNext(e.vert2, e.next2);
            e.next2.SetPrev(e.vert2, e.prev2);

            edges.Remove(e);
        }

        /**
         * Removing a loop also removes associated face.
         * used internally only, just RemoveFace(loop.face) outside of here.
         */
        void RemoveLoop(Loop l)
        {
            if (l.face != null) // null iff loop is called from RemoveFace
            {
                // Trigger removing other loops, and this one again with l.face == null
                RemoveFace(l.face);
                return;
            }

            // remove from radial linked list
            if (l.radial_next == l)
            {
                l.edge.loop = null;
            }
            else
            {
                l.radial_prev.radial_next = l.radial_next;
                l.radial_next.radial_prev = l.radial_prev;
                if (l.edge.loop == l)
                {
                    l.edge.loop = l.radial_next;
                }
            }

            // forget other loops of the same face so thet they get released from memory
            l.next = null;
            l.prev = null;

            loops.Remove(l);
        }

        /// <summary>
        /// Removes the specified <see cref="Face"/> from the <see cref="BMesh"/>,
        /// also removes all associated <see cref="Loop"/>s.
        /// </summary>
        /// <param name="f">The <see cref="Face"/> to remove from the <see cref="BMesh"/>.</param>
        /// <remarks>
        /// If the specified <see cref="Face"/> is not part of the <see cref="BMesh"/>, the behavior is undefined.
        /// </remarks>
        public void RemoveFace(Face f)
        {
            Loop l = f.loop;
            Loop nextL = null;
            while (nextL != f.loop)
            {
                nextL = l.next;
                l.face = null; // prevent infinite recursion, because otherwise RemoveLoop calls RemoveFace
                RemoveLoop(l);
                l = nextL;
            }
            faces.Remove(f);
        }
        #endregion
        
        ///////////////////////////////////////////////////////////////////////////
        #region [Vertex Attribute Methods]

        /**
         * The same series of method repeats for Vertices, Edges, Loops and Faces.
         * Maybe there's a nice way to factorize, but in the meantime I'll at least
         * factorize the comments, so the following work for all types of
         * topological entities.
         */

        /**
         * Check whether the mesh as an attribute enforced to any vertices with the
         * given name. If this is true, one can safely use v.attribtues[attribName]
         * without checking v.attributes.ContainsKey() first.
         */
        public bool HasVertexAttribute(string attribName)
        {
            foreach (var a in vertexAttributes)
            {
                if (a.name == attribName)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasVertexAttribute(AttributeDefinition attrib)
        {
            return HasVertexAttribute(attrib.name);
        }

        /**
         * Add a new attribute and return it, so that one can write oneliners like
         *     AddVertexAttribute("foo", Float, 3).defaultValue = ...
         * NB: It does not return the attribute from the mesh definition if it
         * existed already. Maybe this can be considered as a bug, maybe not.
         */
        public AttributeDefinition AddVertexAttribute(AttributeDefinition attrib)
        {
            if (HasVertexAttribute(attrib)) return attrib; // !!
            vertexAttributes.Add(attrib);
            foreach (Vertex v in vertices)
            {
                if (v.attributes == null) v.attributes = new Dictionary<string, AttributeValue>(); // move in Vertex ctor?
                v.attributes[attrib.name] = AttributeValue.Copy(attrib.defaultValue);
            }
            return attrib;
        }

        public AttributeDefinition AddVertexAttribute(string name, AttributeBaseType baseType, int dimensions)
        {
            return AddVertexAttribute(new AttributeDefinition(name, baseType, dimensions));
        }

        /**
         * Called internally when adding a new vertex to ensure that the vertex has
         * all required attribute. If not, the default value is used to add it.
         */
        void EnsureVertexAttributes(Vertex v)
        {
            if (v.attributes == null) v.attributes = new Dictionary<string, AttributeValue>();
            foreach (var attr in vertexAttributes)
            {
                if (!v.attributes.ContainsKey(attr.name))
                {
                    v.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
                }
                else if (!attr.type.CheckValue(v.attributes[attr.name]))
                {
                    Debug.LogWarning("Vertex attribute '" + attr.name + "' is not compatible with mesh attribute definition, ignoring.");
                    // different type, overriding value with default
                    v.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
                }
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////
        #region [Edge Attribute Methods]

        public bool HasEdgeAttribute(string attribName)
        {
            foreach (var a in edgeAttributes)
            {
                if (a.name == attribName)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasEdgeAttribute(AttributeDefinition attrib)
        {
            return HasEdgeAttribute(attrib.name);
        }

        public AttributeDefinition AddEdgeAttribute(AttributeDefinition attrib)
        {
            if (HasEdgeAttribute(attrib)) return attrib;
            edgeAttributes.Add(attrib);
            foreach (Edge e in edges)
            {
                if (e.attributes == null) e.attributes = new Dictionary<string, AttributeValue>(); // move in Edge ctor?
                e.attributes[attrib.name] = AttributeValue.Copy(attrib.defaultValue);
            }
            return attrib;
        }

        public AttributeDefinition AddEdgeAttribute(string name, AttributeBaseType baseType, int dimensions)
        {
            return AddEdgeAttribute(new AttributeDefinition(name, baseType, dimensions));
        }

        void EnsureEdgeAttributes(Edge e)
        {
            if (e.attributes == null) e.attributes = new Dictionary<string, AttributeValue>();
            foreach (var attr in edgeAttributes)
            {
                if (!e.attributes.ContainsKey(attr.name))
                {
                    e.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
                }
                else if (!attr.type.CheckValue(e.attributes[attr.name]))
                {
                    Debug.LogWarning("Edge attribute '" + attr.name + "' is not compatible with mesh attribute definition, ignoring.");
                    // different type, overriding value with default
                    e.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
                }
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////
        #region [Loop Attribute Methods]

        public bool HasLoopAttribute(string attribName)
        {
            foreach (var a in loopAttributes)
            {
                if (a.name == attribName)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasLoopAttribute(AttributeDefinition attrib)
        {
            return HasLoopAttribute(attrib.name);
        }

        public AttributeDefinition AddLoopAttribute(AttributeDefinition attrib)
        {
            if (HasLoopAttribute(attrib)) return attrib;
            loopAttributes.Add(attrib);
            foreach (Loop l in loops)
            {
                if (l.attributes == null) l.attributes = new Dictionary<string, AttributeValue>(); // move in Loop ctor?
                l.attributes[attrib.name] = AttributeValue.Copy(attrib.defaultValue);
            }
            return attrib;
        }

        public AttributeDefinition AddLoopAttribute(string name, AttributeBaseType baseType, int dimensions)
        {
            return AddLoopAttribute(new AttributeDefinition(name, baseType, dimensions));
        }

        void EnsureLoopAttributes(Loop l)
        {
            if (l.attributes == null) l.attributes = new Dictionary<string, AttributeValue>();
            foreach (var attr in loopAttributes)
            {
                if (!l.attributes.ContainsKey(attr.name))
                {
                    l.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
                }
                else if (!attr.type.CheckValue(l.attributes[attr.name]))
                {
                    Debug.LogWarning("Loop attribute '" + attr.name + "' is not compatible with mesh attribute definition, ignoring.");
                    // different type, overriding value with default
                    l.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
                }
            }
        }
        #endregion

        ///////////////////////////////////////////////////////////////////////////
        #region [Face Attribute Methods]

        public bool HasFaceAttribute(string attribName)
        {
            foreach (var a in faceAttributes)
            {
                if (a.name == attribName)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasFaceAttribute(AttributeDefinition attrib)
        {
            return HasFaceAttribute(attrib.name);
        }

        public AttributeDefinition AddFaceAttribute(AttributeDefinition attrib)
        {
            if (HasFaceAttribute(attrib)) return attrib;
            faceAttributes.Add(attrib);
            foreach (Face f in faces)
            {
                if (f.attributes == null) f.attributes = new Dictionary<string, AttributeValue>(); // move in Face ctor?
                f.attributes[attrib.name] = AttributeValue.Copy(attrib.defaultValue);
            }
            return attrib;
        }

        public AttributeDefinition AddFaceAttribute(string name, AttributeBaseType baseType, int dimensions)
        {
            return AddFaceAttribute(new AttributeDefinition(name, baseType, dimensions));
        }

        void EnsureFaceAttributes(Face f)
        {
            if (f.attributes == null) f.attributes = new Dictionary<string, AttributeValue>();
            foreach (var attr in faceAttributes)
            {
                if (!f.attributes.ContainsKey(attr.name))
                {
                    f.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
                }
                else if (!attr.type.CheckValue(f.attributes[attr.name]))
                {
                    Debug.LogWarning("Face attribute '" + attr.name + "' is not compatible with mesh attribute definition, ignoring.");
                    // different type, overriding value with default
                    f.attributes[attr.name] = AttributeValue.Copy(attr.defaultValue);
                }
            }
        }
        #endregion
    }

}