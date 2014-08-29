//-------------------------------------------------------------------------------------------------
// file:	DomainModel\StoreOptions.cs
//
// summary:	Implements the store options class
//-------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyperstore.Modeling
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>
    ///  Specifying Store Options.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [Flags]
    public enum StoreOptions
    {
        /// <summary>
        ///  No option
        /// </summary>
        None = 0,
        /// <summary>
        ///  Store supports scoping for domain model
        /// </summary>
        EnableScopings = 1
    }
}
