//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.IO;

namespace Fabrica.Watch.SmartInspect
{
	internal class FileHelper
	{
		private const string ALREADY_EXISTS_SUFFIX = "a";

		/* Do not change */
		private const string DATETIME_FORMAT = "yyyy-MM-dd-HH-mm-ss";
		private const char DATETIME_SEPARATOR = '-';
		private const int DATETIME_TOKENS = 6;

		public static DateTime GetFileDate(string baseName, string path)
		{
			DateTime fileDate;

			if (!TryGetFileDate(baseName, path, out fileDate))
			{
				throw new SmartInspectException("Invalid filename");
			}

			return fileDate;
		}

		private static bool TryGetFileDate(string baseName, string path,
			out DateTime fileDate)
		{
			fileDate = DateTime.MinValue; /* Required */

			string fileName = Path.GetFileName(path);

			baseName = Path.ChangeExtension(
				Path.GetFileName(baseName), 
				null);

			/* In order to avoid possible bugs with the creation
			 * time of file names (log files on Windows or Samba
			 * shares, for instance), we parse the name of the log
			 * file and do not use its creation time. */

			int index = fileName.IndexOf(baseName);

			if (index != 0)
			{
				return false;
			}

			string value = Path.ChangeExtension(
				fileName.Substring(baseName.Length + 1),
				null);

			/* Strip any added ALREADY_EXISTS_SUFFIX characters.
			 * This can happen if we are non-append mode and need
			 * to add this special character/suffix in order to
			 * not override an existing file. */

			if (value.Length > DATETIME_FORMAT.Length)
			{
				value = value.Substring(0, DATETIME_FORMAT.Length);
			}

			if (!TryParseFileDate(value, out fileDate))
			{
				return false;
			}

			return true;
		}

		private static bool TryParseFileDate(string fileDate, 
			out DateTime dateTime)
		{
			dateTime = DateTime.MinValue; /* Required */

			if (fileDate == null)
			{
				return false;
			}

			if (fileDate.Length != DATETIME_FORMAT.Length)
			{
				return false;
			}

			for (int i = 0; i < fileDate.Length; i++)
			{ 
				char c = fileDate[i];
				if (!Char.IsDigit(c) && c != DATETIME_SEPARATOR)
				{
					return false;
				}
			}

			string[] values = fileDate.Split(DATETIME_SEPARATOR);

			if (values == null || values.Length != DATETIME_TOKENS)
			{
				return false;
			}

			try
			{
				dateTime = new DateTime(
					Int32.Parse(values[0]), /* Year */
					Int32.Parse(values[1]), /* Month */
					Int32.Parse(values[2]), /* Day */
					Int32.Parse(values[3]), /* Hour */
					Int32.Parse(values[4]), /* Minute */
					Int32.Parse(values[5])  /* Second */
				);
			}
			catch (ArgumentOutOfRangeException)
			{
				return false;
			}

			return true;
		}

		private static bool IsValidFile(string baseName, string path)
		{
			DateTime fileDate;
			return TryGetFileDate(baseName, path, out fileDate);
		}

		public static string GetFileName(string baseName, bool append)
		{
			/* In rotating mode, we need to differentiate between
			 * append and non-append mode. In append mode, we try
			 * to use an already existing file. In non-append mode,
			 * we just use a new file with the current timestamp
			 * appended. */

			if (append)
			{
				string fileName = FindFileName(baseName);

				if (fileName != null)
				{
					return fileName;
				}
			}

			return ExpandFileName(baseName); /* Fallback */
		}

		private static string FindFileName(string baseName)
		{
			string[] files = GetFiles(baseName);

			if (files == null || files.Length == 0)
			{
				return null;
			}

			return files[files.Length - 1]; /* The newest */
		}

		private static string ExpandFileName(string baseName)
		{
			string result = String.Concat(
				Path.ChangeExtension(baseName, null),
				"-",
				DateTime.UtcNow.ToString(DATETIME_FORMAT),
				Path.GetExtension(baseName)
			);

			/* Append a special character/suffix to the expanded
			 * file name if the file already exists in order to
			 * not override an existing log file. */

			while (File.Exists(result))
			{
				result = String.Concat(
					Path.ChangeExtension(result, null),
					ALREADY_EXISTS_SUFFIX,
					Path.GetExtension(result)
				);
			}

			return result;
		}

		private static string[] GetFiles(string baseName)
		{
			string fileName = Path.GetFileName(baseName);

			string pattern = String.Concat(
				Path.ChangeExtension(fileName, null),
				"-*",
				Path.GetExtension(fileName)
			);

			string dir = Path.GetDirectoryName(baseName);

			if (dir == null || dir.Length == 0)
			{
				dir = ".";
			}

			string[] files = Directory.GetFiles(dir, pattern);
			Array.Sort(files);

			/* Only return files with a valid file name (see
			 * IsValidFile) and ignore all others.
			 * Directory.GetFiles lists all files which match the
			 * given path, so it might return files which are not
			 * really related to our current log file. */

			return ValidateFiles(baseName, files);
		}

		private static string[] ValidateFiles(string baseName,
			string[] files)
		{
			int valid = 0;

			for (int i = 0; i < files.Length; i++)
			{
				if (IsValidFile(baseName, files[i]))
				{
					valid++;
				}
				else 
				{
					files[i] = null; /* See below */
				}
			}

			if (valid == files.Length)
			{
				return files; /* No need to copy */
			}

			string[] result = new string[valid];

			int j = 0;
			for (int i = 0; i < files.Length; i++)
			{
				if (files[i] != null) /* See above */
				{
					result[j] = files[i];
					j++;
				}
			}

			return result;
		}

		public static void DeleteFiles(string baseName, int maxParts)
		{
			string[] files = GetFiles(baseName);

			if (files == null)
			{
				return;
			}

			for (int i = 0; i < files.Length; i++)
			{
				if (i + maxParts >= files.Length)
				{
					break;
				}

				try
				{
					File.Delete(files[i]);
				}
				catch
				{
					/* Can't do anything about it. We try to delete
					 * it the next time this method is called. */
				}
			}
		}
	}
}
