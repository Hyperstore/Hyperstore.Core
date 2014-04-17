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
    ///  Specifying StoreOptions.
    /// </summary>
    ///-------------------------------------------------------------------------------------------------
    [Flags]
    public enum StoreOptions
    {
        /// <summary>   A binary constant representing the none flag. </summary>
        None = 0,
        /// <summary>   A binary constant representing the enable extensions flag. </summary>
        EnableExtensions = 1
    }
}
