﻿// Copyright (c) 2015 Robert Rouhani <robert.rouhani@gmail.com> and other contributors (see CONTRIBUTORS file).
// Licensed under the MIT License - https://raw.github.com/Robmaister/SharpNav/master/LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpNav.Collections;
using SharpNav.Geometry;
using SharpNav.Pathfinding;

namespace SharpNav.IO
{
    /// <summary>
    /// Subclass of NavMeshSerializer that implements 
    /// serialization/deserializtion in text files with json format
    /// </summary>
    public class NavMeshJsonSerializer : NavMeshSerializer
    {
        public override void Serialize(string path, TiledNavMesh mesh)
        {
            JObject root = new JObject();

            SerializeNavMeshField(root, mesh, "parameters");
            SerializeNavMeshField(root, mesh, "origin");
            SerializeNavMeshField(root, mesh, "tileWidth");
            SerializeNavMeshField(root, mesh, "tileHeight");
            SerializeNavMeshField(root, mesh, "maxTiles");
            SerializeNavMeshField(root, mesh, "tileLookupTableSize");
            SerializeNavMeshField(root, mesh, "tileLookupTableMask");
            SerializeNavMeshField(root, mesh, "saltBits");
            SerializeNavMeshField(root, mesh, "tileBits");
            SerializeNavMeshField(root, mesh, "polyBits");
            SerializeNavMeshField(root, mesh, "nextFree");

            var posLookup = (MeshTile[])GetPrivateField(mesh, typeof(TiledNavMesh), "posLookup");
            root.Add("posLookup", SerializeMeshTilesArray(posLookup));

            var tiles = (MeshTile[]) GetPrivateField(mesh, typeof(TiledNavMesh), "tiles");
            root.Add("tiles", SerializeMeshTilesArray(tiles));
            
            File.WriteAllText(path, root.ToString());
        }

        public override TiledNavMesh Deserialize(string path)
        {
            JObject root = JObject.Parse(File.ReadAllText(path));
            var mesh = (TiledNavMesh)FormatterServices.GetUninitializedObject(typeof(TiledNavMesh));

            DeserializeNavMeshField(root, mesh, "parameters", typeof(TiledNavMesh.TiledNavMeshParams));
            DeserializeNavMeshField(root, mesh, "origin", typeof(Vector3));
            DeserializeNavMeshField(root, mesh, "tileWidth", typeof(float));
            DeserializeNavMeshField(root, mesh, "tileHeight", typeof(float));
            DeserializeNavMeshField(root, mesh, "maxTiles", typeof(int));
            DeserializeNavMeshField(root, mesh, "tileLookupTableSize", typeof(int));
            DeserializeNavMeshField(root, mesh, "tileLookupTableMask", typeof(int));
            DeserializeNavMeshField(root, mesh, "saltBits", typeof(int));
            DeserializeNavMeshField(root, mesh, "tileBits", typeof(int));
            DeserializeNavMeshField(root, mesh, "polyBits", typeof(int));
            DeserializeNavMeshField(root, mesh, "nextFree", typeof(MeshTile));

            JArray posLookupToken = (JArray)root.GetValue("posLookup");
            List<MeshTile> posLookup = posLookupToken.Select(DeserializeMeshTile).ToList();
            SetPrivateField(mesh, typeof(TiledNavMesh), "posLookup", posLookup.ToArray());

            JArray tilesToken = (JArray) root.GetValue("tiles");
            List<MeshTile> tiles = tilesToken.Select(DeserializeMeshTile).ToList();
            SetPrivateField(mesh, typeof(TiledNavMesh), "tiles", tiles.ToArray());

            return mesh;
        }

        private void SerializeNavMeshField(JObject root, TiledNavMesh mesh, string fieldName)
        {
            var field = GetPrivateField(mesh, typeof(TiledNavMesh), fieldName);
            if (field != null)
            {
                root.Add(fieldName, JToken.FromObject(field));
            }
        }

        private void DeserializeNavMeshField(JObject root, TiledNavMesh mesh, string fieldName, Type fieldType)
        {
            JToken token = root.GetValue(fieldName);
            if (token != null)
            {
                var value = token.ToObject(fieldType);
                SetPrivateField(mesh, typeof(TiledNavMesh), fieldName, value);
            }
        }

        private JArray SerializeMeshTilesArray(MeshTile[] tiles)
        {
            var tilesArray = new JArray();
            foreach (var tile in tiles)
            {
                tilesArray.Add(SerializeMeshTile(tile));
            }
            return tilesArray;
        }

        private JObject SerializeMeshTile(MeshTile tile)
        {
            var result = new JObject();
            result.Add("Salt", tile.Salt);
            result.Add("LinksFreeList", tile.LinksFreeList);
            result.Add("Header", JToken.FromObject(tile.Header));
            result.Add("Polys", JToken.FromObject(tile.Polys));
            result.Add("Verts", JToken.FromObject(tile.Verts));
            result.Add("Links", JToken.FromObject(tile.Links));
            result.Add("DetailMeshes", JToken.FromObject(tile.DetailMeshes));
            result.Add("DetailVerts", JToken.FromObject(tile.DetailVerts));
            result.Add("DetailTris", JToken.FromObject(tile.DetailTris));
            result.Add("OffMeshConnections", JToken.FromObject(tile.OffMeshConnections));

            var treeNodes = (BVTree.Node[])GetPrivateField(tile.BVTree, "nodes");
            JObject treeObject = new JObject();
            treeObject.Add("nodes", JToken.FromObject(treeNodes));

            result.Add("BVTree", treeObject);

            return result;
        }

        private MeshTile DeserializeMeshTile(JToken token)
        {
            JObject jObject = (JObject) token;
            MeshTile result = new MeshTile();
            result.Salt = jObject.GetValue("Salt").Value<int>();
            result.LinksFreeList = jObject.GetValue("LinksFreeList").Value<int>();
            result.Header = jObject.GetValue("Header").ToObject<PathfindingCommon.NavMeshInfo>();
            result.Polys = jObject.GetValue("Polys").ToObject<Poly[]>();
            result.Verts = jObject.GetValue("Verts").ToObject<Vector3[]>();
            result.Links = jObject.GetValue("Links").ToObject<Link[]>();
            result.DetailMeshes = jObject.GetValue("DetailMeshes").ToObject<PolyMeshDetail.MeshData[]>();
            result.DetailVerts = jObject.GetValue("DetailVerts").ToObject<Vector3[]>();
            result.DetailTris = jObject.GetValue("DetailTris").ToObject<PolyMeshDetail.TriangleData[]>();
            result.OffMeshConnections = jObject.GetValue("OffMeshConnections").ToObject<OffMeshConnection[]>();
    
            var tree = (BVTree) FormatterServices.GetUninitializedObject(typeof(BVTree));
            var treeObject = (JObject) jObject.GetValue("BVTree");
            var nodes = treeObject.GetValue("nodes").ToObject<BVTree.Node[]>();

            SetPrivateField(tree, "nodes", nodes);
            result.BVTree = tree;

            return result;
        }
    }
}
