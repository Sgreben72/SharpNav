﻿#region License
/**
 * Copyright (c) 2013 Robert Rouhani <robert.rouhani@gmail.com> and other contributors (see CONTRIBUTORS file).
 * Licensed under the MIT License - https://raw.github.com/Robmaister/SharpNav/master/LICENSE
 */
#endregion

using System;
using System.Runtime.InteropServices;

namespace SharpNav
{
	/// <summary>
	/// Represents a voxel span in a <see cref="CompactHeightfield"/>.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct CompactSpan
	{
		public const byte NotConnected = 0xff; //TODO any clean way to do this without bloating up the struct?

		/// <summary>
		/// The span minimum.
		/// </summary>
		public int Minimum;

		/// <summary>
		/// The number of voxels contained in the span.
		/// </summary>
		public int Height;

		/// <summary>
		/// An int (split into 4 bytes) containing span connection data to neighboring cells.
		/// </summary>
		public byte ConnectionWest;
		public byte ConnectionNorth;
		public byte ConnectionEast;
		public byte ConnectionSouth;

		/// <summary>
		/// The region the span belongs to.
		/// </summary>
		public int Region;

		/// <summary>
		/// Initializes a new instance of the <see cref="CompactSpan"/> struct.
		/// </summary>
		/// <param name="minimum">The span minimum.</param>
		/// <param name="height">The number of voxels the span contains.</param>
		public CompactSpan(int minimum, int height)
		{
			this.Minimum = minimum;
			this.Height = height;
			this.ConnectionWest = NotConnected;
			this.ConnectionNorth = NotConnected;
			this.ConnectionEast = NotConnected;
			this.ConnectionSouth = NotConnected;
			this.Region = 0;
		}

		/// <summary>
		/// Gets a value indicating whether the span has an upper bound or goes to "infinity".
		/// </summary>
		public bool HasUpperBound
		{
			get
			{
				return Height != int.MaxValue;
			}
		}

		/// <summary>
		/// Gets the upper bound of the span.
		/// </summary>
		public int Maximum
		{
			get
			{
				return Minimum + Height;
			}
		}

		/// <summary>
		/// Creates a <see cref="CompactSpan"/> from a minimum boundary and a maximum boundary.
		/// </summary>
		/// <param name="min">The minimum.</param>
		/// <param name="max">The maximum.</param>
		/// <returns>A <see cref="CompactSpan"/>.</returns>
		public static CompactSpan FromMinMax(int min, int max)
		{
			CompactSpan s;
			FromMinMax(min, max, out s);
			return s;
		}

		/// <summary>
		/// Creates a <see cref="CompactSpan"/> from a minimum boundary and a maximum boundary.
		/// </summary>
		/// <param name="min">The minimum.</param>
		/// <param name="max">The maximum.</param>
		/// <param name="span">A <see cref="CompactSpan"/>.</param>
		public static void FromMinMax(int min, int max, out CompactSpan span)
		{
			span.Minimum = min;
			span.Height = max - min;
			span.ConnectionWest = NotConnected;
			span.ConnectionNorth = NotConnected;
			span.ConnectionEast = NotConnected;
			span.ConnectionSouth = NotConnected;
			span.Region = 0;
		}

		/// <summary>
		/// Sets connection data to a span contained in a neighboring cell.
		/// </summary>
		/// <param name="dir">The direction of the cell.</param>
		/// <param name="i">The index of the span in the neighboring cell.</param>
		/// <param name="s">The <see cref="CompactSpan"/> to set the data for.</param>
		public static void SetConnection(int dir, int i, ref CompactSpan s)
		{
			if (i > NotConnected)
				throw new ArgumentOutOfRangeException("Index of connecting span is too high to be stored. Try increasing cell height.", "i");

			dir %= 4;

			switch (dir)
			{
				case 0:
					s.ConnectionWest = (byte)i;
					break;
				case 1:
					s.ConnectionNorth = (byte)i;
					break;
				case 2:
					s.ConnectionEast = (byte)i;
					break;
				case 3:
					s.ConnectionSouth = (byte)i;
					break;
			}
		}

		/// <summary>
		/// Gets the connection data for a neighboring cell in a specified direction.
		/// </summary>
		/// <param name="dir">The direction.</param>
		/// <param name="s">The <see cref="CompactSpan"/> to get the connection data from.</param>
		/// <returns>The index of the span in the neighboring cell.</returns>
		public static int GetConnection(int dir, ref CompactSpan s)
		{
			dir %= 4;

			switch (dir)
			{
				case 0:
					return s.ConnectionWest;
				case 1:
					return s.ConnectionNorth;
				case 2:
					return s.ConnectionEast;
				case 3:
				default:
					return s.ConnectionSouth;
			}
		}

		/// <summary>
		/// Gets the connection data for a neighboring call in a specified direction.
		/// </summary>
		/// <param name="dir">The direction.</param>
		/// <returns>The index of the span in the neighboring cell.</returns>
		public int GetConnection(int dir)
		{
			return GetConnection(dir, ref this);
		}
	}
}