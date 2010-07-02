// 
// State.cs
//  
// Author:
//       carl <>
// 
// Copyright (c) 2010 carl
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace Client.Sim
{
	using System;
	using System.Collections.Generic;
	using OpenTK;
	
	
	public class ObjectStatePacket : ICloneable
	{
		public object Clone ()
		{
			return MemberwiseClone();
		}
		
		public int Uid;
		public string TypeId;
		public Vector2 Position;
		public int Angle;
		public int Flags;
	}
	
	public class VehicleStatePacket : ObjectStatePacket
	{
		public static readonly int HurtFlag = 1 << 0; 
		
		public VehicleStatePacket() 
		{
		}
		
		public VehicleStatePacket(ObjectStatePacket os)
		{
			Uid = os.Uid;
			TypeId = os.TypeId;
			Position = os.Position;
			Angle = os.Angle;
		}
		
		public new object Clone ()
		{
			return MemberwiseClone();
		}
		
		public int HitPoints;
		public int MaxHitPoints;
	}

	public class BuildableStatePacket : ObjectStatePacket
	{
		public static readonly int FireFlag = 1 << 0; 
		
		public BuildableStatePacket() 
		{
		}
		
		public BuildableStatePacket(ObjectStatePacket os)
		{
			Uid = os.Uid;
			TypeId = os.TypeId;
			Position = os.Position;
			Angle = os.Angle;
		}
		
		public new object Clone ()
		{
			return MemberwiseClone();
		}
	}
	/*
	public class AddObjectPacket
	{
		public AddObjectPacket(int uid, string typeId) 
		{
			Uid = uid;
			TypeId = typeId;
		}
		
		public int Uid;
		public string TypeId;
	}
*/	
	
	public enum EffectPacketType : byte
	{
		Undefined = 0,
		AddBullet = 1,
		AddExplosion = 2,
		Remove = 255
	}
	
	public class EffectPacket
	{
		public EffectPacket(EffectPacketType type, Vector2 p, Vector2 v)
		{
			Position = p;
			Velocity = v;
			Type = type;
		}
		/*
		public byte[] ToByteArray()
		{
			byte[] b = new byte[3+4+4+1];
			b[0] = Uid & 255;
			b[1] = (Uid >> 8) & 255;
			b[2] = (Uid >> 16) & 255;
			b[3] = Type & 255;
			int px = (int)Position.X;
			b[4] = px & 255;
			b[5] = (px >> 16) & 255;
			int py = (int)Position.Y;
			b[6] = py & 255;
			b[7] = (py >> 16) & 255;
			
			var vxb = Vector2h.GetBytes(new Vector2h(Velocity));
			Debug.Assert(vxb.Length == 4);
			
			int vx = (int)Velocity.X;
			b[4] = vx & 255;
			b[5] = (vx >> 16) & 255;
			int vy = (int)Velocity.Y;
			b[6] = vy & 255;
			b[7] = (vy >> 16) & 255;
			return b;
		}
		*/
		public int Uid;
		public Vector2 Position; 
		public Vector2 Velocity;
		public EffectPacketType Type;
	}
	
	public class StateUpdate
	{
		public SortedDictionary<int, ObjectStatePacket> UpdatedObjects;
		public List<EffectPacket> EffectPackets;
		public double Timestamp;
	}
}

