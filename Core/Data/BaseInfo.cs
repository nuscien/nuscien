using System;
using System.Collections.Generic;
using System.Text;

using Trivial.Reflection;

namespace NuScien.Data
{
    public abstract class BaseInfo : BaseObservableProperties
    {
        public string Id { get; }
    }
}
