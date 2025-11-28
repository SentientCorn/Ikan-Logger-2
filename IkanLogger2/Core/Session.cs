using IkanLogger2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IkanLogger2.Core
{
    public static class Session
    {
        public static User CurrentUser { get; set; }
        public static bool IsLoggedIn => CurrentUser != null;

        public void clearSession()
        {
            CurrentUser = null;
        }
    }
}
