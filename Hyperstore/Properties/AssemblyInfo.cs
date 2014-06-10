// Copyright 2014 Zenasoft.  All rights reserved.
//
// This file is part of Hyperstore.
//
//    Hyperstore is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Hyperstore is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Hyperstore.  If not, see <http://www.gnu.org/licenses/>.

#region Imports

using System.Reflection;
using System.Runtime.CompilerServices;

#endregion

[assembly: AssemblyTitle("Hyperstore")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Zenasoft")]
[assembly: AssemblyProduct("Hyperstore")]
[assembly: AssemblyCopyright("Copyright ©  2013-2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
//[assembly: AssemblyFileVersion("1.0.0.0")]
#if !BUILD
[assembly: InternalsVisibleTo("Hyperstore.Tests")]
[assembly: InternalsVisibleTo("Hyperstore.Platform")]
#endif