﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNav
{
	[Flags]
	public enum AreaFlags : byte
	{
		Null = 0,
		Walkable = 1
	}
}