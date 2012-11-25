﻿// --------------------------------------------------------------------------------------------
// <copyright file="ObjectLoader.cs" company="Effort Team">
//     Copyright (C) 2012 Effort Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------

namespace Effort.DataLoaders
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Effort.Internal.DbManagement.Schema;

    /// <summary>
    ///     Loads data from a table data loader and materializes it.
    /// </summary>
    internal static class ObjectLoader
    {
        /// <summary>
        ///     Loads the table data from the specified table data loader and materializes it
        ///     bases on the specified metadata.
        /// </summary>
        /// <param name="loaderFactory"> The loader factory. </param>
        /// <param name="table"> The table metadata. </param>
        /// <returns> The materialized data. </returns>
        public static IEnumerable<object> Load(
            ITableDataLoaderFactory loaderFactory, 
            DbTableInformation table)
        {
            List<ColumnDescription> columns = new List<ColumnDescription>();
            PropertyInfo[] properties = table.EntityType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                string name = property.Name;
                Type type = property.PropertyType;

                // TODO: external 
                if (type == typeof(NMemory.Data.Timestamp) || 
                    type == typeof(NMemory.Data.Binary))
                {
                    type = typeof(byte[]);
                }

                ColumnDescription column = new ColumnDescription(name, type);
                columns.Add(column);
            }

            TableDescription tableDescription = 
                new TableDescription(table.TableName, columns);

            ITableDataLoader loader = loaderFactory.CreateTableDataLoader(tableDescription);

            foreach (object[] data in loader.GetData())
            {
                object[] entityProperties = new object[data.Length];

                for (int i = 0; i < columns.Count; i++)
                {
                    // TODO: external
                    if (properties[i].PropertyType == typeof(NMemory.Data.Timestamp))
                    {
                        entityProperties[i] = (NMemory.Data.Timestamp)(byte[])data[i];
                    }
                    else if (properties[i].PropertyType == typeof(NMemory.Data.Binary))
                    {
                        entityProperties[i] = (NMemory.Data.Binary)(byte[])data[i];
                    }
                    else
                    {
                        entityProperties[i] = data[i];
                    }
                }
                
                yield return table.EntityInitializer.DynamicInvoke(entityProperties);
            }
        }
    }
}
