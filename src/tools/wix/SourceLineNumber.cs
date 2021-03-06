//-------------------------------------------------------------------------------------------------
// <copyright file="SourceLineNumber.cs" company="Outercurve Foundation">
//   Copyright (c) 2004, Outercurve Foundation.
//   This software is released under Microsoft Reciprocal License (MS-RL).
//   The license and further copyright text can be found in the file
//   LICENSE.TXT at the root directory of the distribution.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace WixToolset
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents file name and line number for source file
    /// </summary>
    public sealed class SourceLineNumber
    {
        /// <summary>
        /// Constructor for a source with no line information.
        /// </summary>
        /// <param name="fileName">File name of the source.</param>
        public SourceLineNumber(string fileName)
        {
            this.FileName = fileName;
        }

        /// <summary>
        /// Constructor for a source with line information.
        /// </summary>
        /// <param name="fileName">File name of the source.</param>
        /// <param name="lineNumber">Line number of the source.</param>
        public SourceLineNumber(string fileName, int lineNumber)
        {
            this.FileName = fileName;
            this.LineNumber = lineNumber;
        }

        /// <summary>
        /// Gets the file name of the source.
        /// </summary>
        /// <value>File name for the source.</value>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets or sets the line number of the source.
        /// </summary>
        /// <value>Line number of the source.</value>
        public int? LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the parent source line number that included this source line number.
        /// </summary>
        public SourceLineNumber Parent { get; set; }

        /// <summary>
        /// Gets the file name and line information.
        /// </summary>
        /// <value>File name and line information.</value>
        public string QualifiedFileName
        {
            get
            {
                return this.LineNumber.HasValue ? String.Concat(this.FileName, "*", this.LineNumber) : this.FileName;
            }
        }

        /// <summary>
        /// Creates a source line number from an encoded string.
        /// </summary>
        /// <param name="encodedSourceLineNumbers">Encoded string to parse.</param>
        public static SourceLineNumber CreateFromEncoded(string encodedSourceLineNumbers)
        {
            string[] linesSplit = encodedSourceLineNumbers.Split('|');

            SourceLineNumber first = null;
            SourceLineNumber parent = null;
            for (int i = 0; i < linesSplit.Length; ++i)
            {
                string[] filenameSplit = linesSplit[i].Split('*');
                SourceLineNumber source;

                if (2 == filenameSplit.Length)
                {
                    source = new SourceLineNumber(filenameSplit[0], Convert.ToInt32(filenameSplit[1]));
                }
                else
                {
                    source = new SourceLineNumber(filenameSplit[0]);
                }

                if (null != parent)
                {
                    parent.Parent = source;
                }

                parent = source;
                if (null == first)
                {
                    first = parent;
                }
            }

            return first;
        }

        /// <summary>
        /// Creates a source line number from a URI.
        /// </summary>
        /// <param name="uri">Uri to convert into source line number</param>
        public static SourceLineNumber CreateFromUri(string uri)
        {
            if (String.IsNullOrEmpty(uri))
            {
                return null;
            }

            // make the local path look like a normal local path
            string localPath = new Uri(uri).LocalPath;
            localPath = localPath.TrimStart(Path.AltDirectorySeparatorChar).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            return new SourceLineNumber(localPath);
        }

        /// <summary>
        /// Returns the SourceLineNumber and parents encoded as a string.
        /// </summary>
        public string GetEncoded()
        {
            StringBuilder sb = new StringBuilder(this.QualifiedFileName);

            for (SourceLineNumber source = this.Parent; null != source; source = source.Parent)
            {
                sb.Append("|");
                sb.Append(source.QualifiedFileName);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determines if two SourceLineNumbers are equivalent.
        /// </summary>
        /// <param name="obj">Object to compare.</param>
        /// <returns>True if SourceLineNumbers are equivalent.</returns>
        public override bool Equals(object obj)
        {
            SourceLineNumber other = obj as SourceLineNumber;
            return null != other &&
                   this.LineNumber.HasValue == other.LineNumber.HasValue &&
                   (!this.LineNumber.HasValue || this.LineNumber == other.LineNumber) &&
                   this.FileName.Equals(other.FileName, StringComparison.OrdinalIgnoreCase) &&
                   (null == this.Parent && null == other.Parent || this.Parent.Equals(other.Parent));
        }

        /// <summary>
        /// Serves as a hash code for a particular type.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return this.GetEncoded().GetHashCode();
        }
    }
}
