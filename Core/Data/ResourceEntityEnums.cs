using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using NuScien.Reflection;
using Trivial.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Data
{
    /// <summary>
    /// The entity states.
    /// </summary>
    public enum ResourceEntityStates
    {
        /// <summary>
        /// The entity does not exist or is removed.
        /// </summary>
        Deleted = 0,

        /// <summary>
        /// The entity is a draft.
        /// </summary>
        Draft = 1,

        /// <summary>
        /// The entity is applying for to approval.
        /// </summary>
        Request = 2,

        /// <summary>
        /// The entity is in service.
        /// </summary>
        Normal = 3
    }

    /// <summary>
    /// The entity states.
    /// </summary>
    public enum ResourceEntityOrders
    {
        /// <summary>
        /// The default order.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Order by last modification time descending (new-old).
        /// </summary>
        Latest = 1,

        /// <summary>
        /// Order by last modification time ascending (old-new).
        /// </summary>
        Time = 2,

        /// <summary>
        /// Order by name ascending (a-z).
        /// </summary>
        Name = 3,

        /// <summary>
        /// Order by name descending (z-a).
        /// </summary>
        Z2A = 4
    }
}
