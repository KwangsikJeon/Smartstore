﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartstore.Core.Customers
{
    /// <summary>
    /// Customer service interface
    /// </summary>
    public partial interface ICustomerService
    {
        /// <summary>
        /// Gets customer by system name.
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <param name="tracked">Whether to load entity tracked. Non-tracking load will be cached.</param>
        /// <returns>Found customer</returns>
        Task<Customer> GetCustomerBySystemNameAsync(string systemName, bool tracked = true);
    }
}
