using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;

namespace APSIM.PerformanceTests.Portal
{
    public static class Genfuncs
    {

        ///###############################################################
        /// <summary>
        /// Convert a List to a DataTable.
        /// </summary>
        /// <remarks>
        /// Based on MIT-licensed code presented at http://www.chinhdo.com/20090402/convert-list-to-datatable/ as "ToDataTable"
        /// <para/>Code modifications made by Nick Campbell.
        /// <para/>Source code provided on this web site (chinhdo.com) is under the MIT license.
        /// <para/>Copyright © 2010 Chinh Do
        /// <para/>Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
        /// <para/>The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
        /// <para/>THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
        /// <para/>(As per http://www.chinhdo.com/20080825/transactional-file-manager/)
        /// </remarks>
        /// <typeparam name="T">Type representing the type to convert.</typeparam>
        /// <param name="l_oItems">List of requested type representing the values to convert.</param>
        /// <returns></returns>
        ///###############################################################
        /// <LastUpdated>February 15, 2010</LastUpdated>
        public static DataTable ToDataTable<T>(List<T> l_oItems)
        {
            DataTable oReturn = new DataTable(typeof(T).Name);
            object[] a_oValues;
            int i;

            //#### Collect the a_oProperties for the passed T
            PropertyInfo[] a_oProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            //#### Traverse each oProperty, .Add'ing each .Name/.BaseType into our oReturn value
            //####     NOTE: The call to .BaseType is required as DataTables/DataSets do not support nullable types, so it's non-nullable counterpart Type is required in the .Column definition
            foreach (PropertyInfo oProperty in a_oProperties)
            {
                oReturn.Columns.Add(oProperty.Name, BaseType(oProperty.PropertyType));
            }

            //#### Traverse the l_oItems
            foreach (T oItem in l_oItems)
            {
                //#### Collect the a_oValues for this loop
                a_oValues = new object[a_oProperties.Length];

                //#### Traverse the a_oProperties, populating each a_oValues as we go
                for (i = 0; i < a_oProperties.Length; i++)
                {
                    a_oValues[i] = a_oProperties[i].GetValue(oItem, null);
                }

                //#### .Add the .Row that represents the current a_oValues into our oReturn value
                oReturn.Rows.Add(a_oValues);
            }

            //#### Return the above determined oReturn value to the caller
            return oReturn;
        }

        ///###############################################################
        /// <summary>
        /// Returns the underlying/base type of nullable types.
        /// </summary>
        /// <remarks>
        /// Based on MIT-licensed code presented at http://www.chinhdo.com/20090402/convert-list-to-datatable/ as "GetCoreType"
        /// <para/>Code modifications made by Nick Campbell.
        /// <para/>Source code provided on this web site (chinhdo.com) is under the MIT license.
        /// <para/>Copyright © 2010 Chinh Do
        /// <para/>Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
        /// <para/>The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
        /// <para/>THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
        /// <para/>(As per http://www.chinhdo.com/20080825/transactional-file-manager/)
        /// </remarks>
        /// <param name="oType">Type representing the type to query.</param>
        /// <returns>Type representing the underlying/base type.</returns>
        ///###############################################################
        /// <LastUpdated>February 15, 2010</LastUpdated>
        public static Type BaseType(Type oType)
        {
            //#### If the passed oType is valid, .IsValueType and is logicially nullable, .Get(its)UnderlyingType
            if (oType != null && oType.IsValueType &&
                oType.IsGenericType && oType.GetGenericTypeDefinition() == typeof(Nullable<>)
            )
            {
                return Nullable.GetUnderlyingType(oType);
            }
            //#### Else the passed oType was null or was not logicially nullable, so simply return the passed oType
            else
            {
                return oType;
            }
        }

    }
}