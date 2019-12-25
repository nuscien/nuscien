using System;
using System.Collections.Generic;
using System.Text;

using NuScien.Data;

namespace NuScien.Users
{
    public enum Genders
    {
        Unknown = 0,
        Male = 1,
        Female = 2,
        Shemale = 4,
        Asexual = 5,
        Other = 7
    }

    public class UserInfo : BaseInfo
    {
        public string LoginName { get; set; }

        public string Nickname { get; set; }

        public Genders Gender { get; set; }

        public DateTime? Birthday { get; set; }
    }
}
