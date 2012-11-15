using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure.Security {
    public static class Protect {
        public static void AgainstInvalidOperation(string message) {
            throw new InvalidOperationException(message);
        }
    }
}
