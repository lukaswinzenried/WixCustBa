﻿//---------------------------------------------------------------------
// <copyright file="BitmapResource.cs" company="Outercurve Foundation">
//   Copyright (c) 2004, Outercurve Foundation.
//   This software is released under Microsoft Reciprocal License (MS-RL).
//   The license and further copyright text can be found in the file
//   LICENSE.TXT at the root directory of the distribution.
// </copyright>
// <summary>
// Part of the Deployment Tools Foundation project.
// </summary>
//---------------------------------------------------------------------

namespace WixToolset.Dtf.Resources
{
    using System;
    using System.IO;

    /// <summary>
    /// A subclass of Resource which provides specific methods for manipulating the resource data.
    /// </summary>
    /// <remarks>
    /// The resource is of type <see cref="ResourceType.Bitmap"/> (RT_GROUPICON).
    /// </remarks>
    public sealed class BitmapResource : Resource
    {
        private const int SizeOfBitmapFileHeader = 14; // this is the sizeof(BITMAPFILEHEADER)

        /// <summary>
        /// Creates a new BitmapResource object without any data. The data can be later loaded from a file.
        /// </summary>
        /// <param name="name">Name of the resource. For a numeric resource identifier, prefix the decimal number with a "#".</param>
        /// <param name="locale">Locale of the resource</param>
        public BitmapResource(string name, int locale)
            : this(name, locale, null)
        {
        }

        /// <summary>
        /// Creates a new BitmapResource object with data. The data can be later saved to a file.
        /// </summary>
        /// <param name="name">Name of the resource. For a numeric resource identifier, prefix the decimal number with a "#".</param>
        /// <param name="locale">Locale of the resource</param>
        /// <param name="data">Raw resource data</param>
        public BitmapResource(string name, int locale, byte[] data)
            : base(ResourceType.Bitmap, name, locale, data)
        {
        }

        /// <summary>
        /// Reads the bitmap from a .bmp file.
        /// </summary>
        /// <param name="path">Path to a bitmap file (.bmp).</param>
        public void ReadFromFile(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                // Move past the BITMAPFILEHEADER, and copy the rest of the bitmap as the resource data. Resource
                // functions expect only the BITMAPINFO struct which exists just beyond the BITMAPFILEHEADER
                // struct in bitmap files.
                fs.Seek(BitmapResource.SizeOfBitmapFileHeader, SeekOrigin.Begin);

                base.Data = new byte[fs.Length - BitmapResource.SizeOfBitmapFileHeader];
                fs.Read(base.Data, 0, base.Data.Length);
            }
        }
    }
}
