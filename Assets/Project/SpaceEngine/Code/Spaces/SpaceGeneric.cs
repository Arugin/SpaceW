﻿#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//     notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: 2017.01.11
// Creation Time: 22:38
// Creator: zameran
#endregion

using System;

using UnityEngine;

[Serializable]
public class SpaceGeneric
{
    public Guid GUID { get; private set; }

    public Vector3d Position { get; protected set; }

    public SpaceGeneric Parent;

    public GameObject GameObject;

    public virtual byte Size { get { return 1; } }
    public virtual long SpaceSize { get { return 1; } }
    public byte HalfSize { get { return (byte)(Size / 2); } }
    public long SideSize { get { return Size * SpaceSize; } }
    public long HalfSpaceSize { get { return SpaceSize / (Size * 2); } } // NOTE : Hack, but it works...

    public Vector3d Shift { get { return new Vector3d(HalfSpaceSize, HalfSpaceSize, HalfSpaceSize); } }

    public SpaceGeneric()
    {
        GUID = Guid.NewGuid();

        Position = Vector3d.zero;

        Parent = null;

        GameObject = null;
    }

    public SpaceGeneric(Vector3d position)
    {
        GUID = Guid.NewGuid();

        Position = position;

        Parent = null;

        GameObject = null;
    }

    public void UpdateHierarchy()
    {
        if (Parent == null) return;
        if (Parent.GameObject == null) return;

        GameObject.transform.parent = Parent.GameObject.transform;
        GameObject.transform.position = Position;
        GameObject.transform.rotation = Quaternion.identity;
    }

    public virtual void Init()
    {
    }

    public virtual void UpdateFromNode(Vector3d position)
    {
        Position = position;
    }

    public virtual void DrawDebug()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(Position, Vector3.one * SpaceSize);
    }
}

[Serializable]
public class Block : SpaceGeneric<SpaceGeneric>
{
    public override byte Size { get { return 1; } }

    public override long SpaceSize { get { return 2048; } }

    public override void DrawDebug()
    {
        base.DrawDebug();

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(Position, Vector3.one * SpaceSize);
    }
}

[Serializable]
public class Chunk : SpaceGeneric<Block>
{
    public override byte Size { get { return 4; } }
    public override long SpaceSize { get { return Size * 2048; } }

    public override void DrawDebug()
    {
        base.DrawDebug();

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Position, Vector3.one * SpaceSize);
    }
}

[Serializable]
public class ChunkKilo : SpaceGeneric<Chunk>
{
    public override byte Size { get { return 4; } }
    public override long SpaceSize { get { return Size * (4 * 2048); } }

    public override void DrawDebug()
    {
        base.DrawDebug();

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Position, Vector3.one * SpaceSize);
    }
}

[Serializable]
public class ChunkMega : SpaceGeneric<ChunkKilo>
{
    public override byte Size { get { return 4; } }
    public override long SpaceSize { get { return Size * (4 * (4 * 2048)); } }

    public override void DrawDebug()
    {
        base.DrawDebug();

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(Position, Vector3.one * SpaceSize);
    }
}

[Serializable]
public class ChunkGiga : SpaceGeneric<ChunkMega>
{
    public override byte Size { get { return 4; } }
    public override long SpaceSize { get { return Size * (4 * (4 * (4 * 2048))); } }

    public override void DrawDebug()
    {
        base.DrawDebug();

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(Position, Vector3.one * SpaceSize);
    }
}

[Serializable]
public class SpaceGeneric<TChildType> : SpaceGeneric where TChildType : SpaceGeneric, new()
{
    public TChildType[,,] LeafNodes;

    public override byte Size { get { return 1; } }
    public override long SpaceSize { get { return 1; } }

    public override void Init()
    {
        LeafNodes = new TChildType[Size, Size, Size];

        for (var x = -HalfSize; x < HalfSize; x++)
        {
            for (var y = -HalfSize; y < HalfSize; y++)
            {
                for (var z = -HalfSize; z < HalfSize; z++)
                {
                    var idx = x + HalfSize;
                    var idy = y + HalfSize;
                    var idz = z + HalfSize;

                    var node = InitNode<TChildType>(x, y, z);

                    LeafNodes[idx, idy, idz] = node;
                }
            }
        }

        base.Init();
    }

    public TNodeType InitNodeAtOrigin<TNodeType>() where TNodeType : SpaceGeneric, new()
    {
        return InitNode<TNodeType>(0, 0, 0);
    }

    private TNodeType InitNode<TNodeType>(int x, int y, int z) where TNodeType : SpaceGeneric, new()
    {
        var mod = SpaceSize / Size;
        var position = new Vector3d(x * mod, y * mod, z * mod) + Shift;

        return InitNode<TNodeType>(position + Position);
    }

    private TNodeType InitNode<TNodeType>(Vector3d center) where TNodeType : SpaceGeneric, new()
    {
        var node = new TNodeType();
        var gameObject = new GameObject(string.Format("{0} : {1}", typeof(TNodeType).Name, center));

        node.GameObject = gameObject;
        node.Parent = this;

        if (typeof(TNodeType) == GetType())
        {
            return node;
        }

        node.Init();
        node.UpdateFromNode(center);
        node.UpdateHierarchy();

        return node;
    }

    public override void UpdateFromNode(Vector3d position)
    {
        if (LeafNodes != null && LeafNodes.Length != 0)
        {
            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    for (var z = 0; z < Size; z++)
                    {
                        var node = LeafNodes[x, y, z];

                        if (node != null)
                        {
                            node.UpdateFromNode(position + (node.Position - Position));
                        }
                    }
                }
            }
        }

        base.UpdateFromNode(position);
    }

    public override void DrawDebug()
    {
        if (LeafNodes != null && LeafNodes.Length != 0)
        {
            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    for (var z = 0; z < Size; z++)
                    {
                        var node = LeafNodes[x, y, z];

                        if (node != null)
                        {
                            node.DrawDebug();
                        }
                    }
                }
            }
        }

        base.DrawDebug();
    }
}