﻿using System;
#if DOTNETCORE
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace System.ComponentModel
{
	internal class Browsable : Attribute
	{
		public Browsable(bool browsable) { }
	}
}
#endif
